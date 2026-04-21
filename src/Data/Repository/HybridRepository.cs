using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using GoodHamburger.Domain;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Events;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Data.Repository;

/// <summary>
/// Hybrid Repository - Saves to EF Core and optionally records events for audit trail
/// </summary>
public class HybridRepository<TEntity> : Repository<TEntity> where TEntity : EntityBase
{
    private readonly IEventStore _eventStore;
    private readonly EventSourcingSettings _settings;
    private readonly IExecutionContextService? _executionContextService;
    private readonly List<Func<CancellationToken, Task>> _pendingEventDispatchers = new();

    public HybridRepository(
        DbContext context,
        IEventStore eventStore,
        EventSourcingSettings settings,
        IExecutionContextService? executionContextService = null)
        : base(context)
    {
        _eventStore = eventStore;
        _settings = settings;
        _executionContextService = executionContextService;
    }

    public override async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        // 1. Save to EF Core (traditional approach)
        var result = await base.AddAsync(entity, cancellationToken);

        // 2. Record event if Event Sourcing is enabled and entity is in audit list
        if (ShouldAuditEntity(typeof(TEntity).Name))
        {
            EnqueueEvent(ct => RecordCreatedEvent(entity, ct));
        }

        return result;
    }

    public override async Task<IEnumerable<TEntity>> AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        var result = await base.AddRangeAsync(entityList, cancellationToken);

        if (ShouldAuditEntity(typeof(TEntity).Name))
        {
            foreach (var entity in entityList)
            {
                var capturedEntity = entity;
                EnqueueEvent(ct => RecordCreatedEvent(capturedEntity, ct));
            }
        }

        return result;
    }

    public override async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        // Detect changes before update
        Dictionary<string, object>? changes = null;
        if (ShouldAuditEntity(typeof(TEntity).Name))
        {
            changes = await DetectChangesAsync(entity, cancellationToken);
        }

        // 1. Update in EF Core
        await base.UpdateAsync(entity, cancellationToken);

        // 2. Record event
        if (changes != null && changes.Count > 0)
        {
            var changesSnapshot = new Dictionary<string, object>(changes);
            EnqueueEvent(ct => RecordUpdatedEvent(entity, changesSnapshot, ct));
        }

        return;
    }

    public override async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        // 1. Delete from EF Core
        await base.DeleteAsync(entity, cancellationToken);

        // 2. Record event
        if (ShouldAuditEntity(typeof(TEntity).Name))
        {
            EnqueueEvent(ct => RecordDeletedEvent(entity, ct));
        }

        return;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);

        if (_pendingEventDispatchers.Count == 0)
        {
            return result;
        }

        try
        {
            foreach (var dispatcher in _pendingEventDispatchers)
            {
                await dispatcher(cancellationToken);
            }
        }
        finally
        {
            _pendingEventDispatchers.Clear();
        }

        return result;
    }

    private bool ShouldAuditEntity(string entityType)
    {
        if (!_settings.Enabled)
        {
            return false;
        }

        // If AuditEntities is empty, audit all entities
        if (_settings.AuditEntities.Count == 0)
        {
            return true;
        }

        // Otherwise, check if entity is in the list
        return _settings.AuditEntities.Contains(entityType);
    }

    private void EnqueueEvent(Func<CancellationToken, Task> dispatcher)
    {
        _pendingEventDispatchers.Add(dispatcher);
    }

    private Task RecordCreatedEvent(TEntity entity, CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TEntity).Name;
        var entityId = entity.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);

        return entity switch
        {
            Order order => AppendDomainEventAsync(entityType, entityId, CreateOrderCreatedEvent(order), cancellationToken),
            Product product => AppendDomainEventAsync(entityType, entityId, CreateProductCreatedEvent(product), cancellationToken),
            _ => AppendDomainEventAsync(entityType, entityId, entity, cancellationToken)
        };
    }

    private Task RecordUpdatedEvent(
        TEntity entity,
        Dictionary<string, object> changes,
        CancellationToken cancellationToken)
    {
        var entityType = typeof(TEntity).Name;
        var entityId = entity.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);

        return entity switch
        {
            Order => AppendDomainEventAsync(entityType, entityId, new OrderUpdatedEvent
            {
                OrderId = entity.Id,
                Changes = changes
            }, cancellationToken),
            Product => AppendDomainEventAsync(entityType, entityId, new ProductUpdatedEvent
            {
                ProductId = entity.Id,
                Changes = changes
            }, cancellationToken),
            _ => AppendDomainEventAsync(entityType, entityId, new { EntityId = entity.Id, Changes = changes }, cancellationToken)
        };
    }

    private Task RecordDeletedEvent(TEntity entity, CancellationToken cancellationToken)
    {
        var entityType = typeof(TEntity).Name;
        var entityId = entity.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);

        return entity switch
        {
            Order => AppendDomainEventAsync(entityType, entityId, new OrderDeletedEvent { OrderId = entity.Id }, cancellationToken),
            Product => AppendDomainEventAsync(entityType, entityId, new ProductDeletedEvent { ProductId = entity.Id }, cancellationToken),
            _ => AppendDomainEventAsync(entityType, entityId, new { EntityId = entity.Id }, cancellationToken)
        };
    }

    private Task AppendDomainEventAsync<TEvent>(
        string aggregateType,
        string aggregateId,
        TEvent payload,
        CancellationToken cancellationToken) where TEvent : class
    {
        var metadata = GetMetadata();

        return _eventStore.AppendEventAsync(
            aggregateType: aggregateType,
            aggregateId: aggregateId,
            eventData: payload,
            userId: GetCurrentUserId(),
            metadata: metadata,
            cancellationToken: cancellationToken);
    }

    private async Task<Dictionary<string, object>> DetectChangesAsync(
        TEntity entity,
        CancellationToken cancellationToken)
    {
        var changes = new Dictionary<string, object>();
        var entry = Context.Entry(entity);

        // Detached entities don't have original values tracked, so fall back to the database snapshot
        Microsoft.EntityFrameworkCore.ChangeTracking.PropertyValues? databaseValues = null;

        foreach (var property in entry.Properties)
        {
            var currentValue = property.CurrentValue;
            object? originalValue;

            if (entry.State == EntityState.Detached)
            {
                databaseValues ??= await entry.GetDatabaseValuesAsync(cancellationToken);

                if (databaseValues == null)
                {
                    return changes;
                }

                originalValue = databaseValues[property.Metadata.Name];
            }
            else
            {
                originalValue = property.OriginalValue;
            }

            if (!Equals(currentValue, originalValue))
            {
                changes[property.Metadata.Name] = new
                {
                    Old = originalValue,
                    New = currentValue
                };
            }
        }

        return changes;
    }

    private OrderCreatedEvent CreateOrderCreatedEvent(Order order)
    {
        return new OrderCreatedEvent
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            CustomerPhone = order.CustomerPhone,
            ShippingAddress = order.ShippingAddress,
            Subtotal = order.Subtotal,
            Total = order.Total,
            Notes = order.Notes,
            Items = order.Items?.Select(i => new OrderItemData
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Subtotal = i.Subtotal
            }).ToList() ?? new List<OrderItemData>()
        };
    }

    private ProductCreatedEvent CreateProductCreatedEvent(Product product)
    {
        return new ProductCreatedEvent
        {
            ProductId = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Category = product.Category,
            IsActive = product.IsActive
        };
    }

    private string? GetCurrentUserId()
    {
        return _executionContextService?.GetCurrentUserId() ?? "system";
    }

    private Dictionary<string, string> GetMetadata()
    {
        if (!_settings.StoreMetadata)
        {
            return new Dictionary<string, string>();
        }

        return _executionContextService?.GetMetadata() ?? new Dictionary<string, string>
        {
            ["Timestamp"] = DateTime.UtcNow.ToString("O"),
            ["MachineName"] = Environment.MachineName
        };
    }
}
