using System.Linq.Expressions;
using LinqToDB;
using GoodHamburger.Data.Context;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Data.Repository.Linq2Db;

/// <summary>
/// Linq2Db implementation of Order repository
/// Demonstrates handling of complex entities with relationships
/// </summary>
public class OrderLinq2DbRepository : IRepository<Order>
{
    private readonly ApplicationDataConnection _db;

    public OrderLinq2DbRepository(ApplicationDataConnection db)
    {
        _db = db;
    }

    public async Task<Order?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order != null)
        {
            order.Items = await _db.OrderItems
                .Where(oi => oi.OrderId == id)
                .ToListAsync(cancellationToken);
        }

        return order;
    }

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _db.Orders.ToListAsync(cancellationToken);
        var orderIds = orders.Select(o => o.Id).ToList();

        var items = await _db.OrderItems
            .Where(oi => orderIds.Contains(oi.OrderId))
            .ToListAsync(cancellationToken);

        foreach (var order in orders)
        {
            order.Items = items.Where(i => i.OrderId == order.Id).ToList();
        }

        return orders;
    }

    public async Task<IEnumerable<Order>> FindAsync(
        Expression<Func<Order, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var orders = await _db.Orders
            .Where(predicate)
            .ToListAsync(cancellationToken);

        if (!orders.Any())
        {
            return orders;
        }

        var orderIds = orders.Select(o => o.Id).ToList();
        var items = await _db.OrderItems
            .Where(oi => orderIds.Contains(oi.OrderId))
            .ToListAsync(cancellationToken);

        foreach (var order in orders)
        {
            order.Items = items.Where(i => i.OrderId == order.Id).ToList();
        }

        return orders;
    }

    public async Task<Order> AddAsync(Order entity, CancellationToken cancellationToken = default)
    {
        await _db.BeginTransactionAsync(cancellationToken);

        try
        {
            // Insert order
            entity.CreatedAt = DateTime.UtcNow;
            entity.Id = await _db.InsertWithInt64IdentityAsync(entity, token: cancellationToken);

            // Insert order items
            if (entity.Items?.Any() == true)
            {
                foreach (var item in entity.Items)
                {
                    item.OrderId = entity.Id;
                    item.CreatedAt = DateTime.UtcNow;
                    await _db.InsertAsync(item, token: cancellationToken);
                }
            }

            await _db.CommitTransactionAsync(cancellationToken);
            return entity;
        }
        catch
        {
            await _db.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IEnumerable<Order>> AddRangeAsync(IEnumerable<Order> entities, CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        foreach (var entity in entityList)
        {
            await AddAsync(entity, cancellationToken);
        }
        return entityList;
    }

    public async Task UpdateAsync(Order entity, CancellationToken cancellationToken = default)
    {
        await _db.BeginTransactionAsync(cancellationToken);

        try
        {
            // Update order
            entity.UpdatedAt = DateTime.UtcNow;
            await _db.UpdateAsync(entity, token: cancellationToken);

            // Delete existing items
            await _db.OrderItems
                .Where(oi => oi.OrderId == entity.Id)
                .DeleteAsync(cancellationToken);

            // Re-insert items
            if (entity.Items?.Any() == true)
            {
                foreach (var item in entity.Items)
                {
                    item.OrderId = entity.Id;
                    item.UpdatedAt = DateTime.UtcNow;
                    await _db.InsertAsync(item, token: cancellationToken);
                }
            }

            await _db.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _db.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task DeleteAsync(Order entity, CancellationToken cancellationToken = default)
    {
        await _db.BeginTransactionAsync(cancellationToken);

        try
        {
            // Delete order items first
            await _db.OrderItems
                .Where(oi => oi.OrderId == entity.Id)
                .DeleteAsync(cancellationToken);

            // Delete order
            await _db.DeleteAsync(entity, token: cancellationToken);

            await _db.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _db.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task DeleteRangeAsync(IEnumerable<Order> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            await DeleteAsync(entity, cancellationToken);
        }
    }

    public async Task<(IEnumerable<Order> Items, int Total)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var total = await _db.Orders.CountAsync(cancellationToken);

        var orders = await _db.Orders
            .OrderBy(o => o.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var orderIds = orders.Select(o => o.Id).ToList();
        var items = await _db.OrderItems
            .Where(oi => orderIds.Contains(oi.OrderId))
            .ToListAsync(cancellationToken);

        foreach (var order in orders)
        {
            order.Items = items.Where(i => i.OrderId == order.Id).ToList();
        }

        return (orders, total);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Linq2Db doesn't have a unit of work pattern
        // Changes are immediately persisted
        return Task.FromResult(0);
    }
}
