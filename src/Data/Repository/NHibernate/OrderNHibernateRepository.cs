using System.Linq.Expressions;
using NHibernate;
using NHibernate.Linq;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Data.Repository.NHibernate;

/// <summary>
/// NHibernate implementation of Order repository
/// Demonstrates handling of complex entities with relationships
/// </summary>
public class OrderNHibernateRepository : IRepository<Order>
{
    private readonly ISession _session;

    public OrderNHibernateRepository(ISession session)
    {
        _session = session;
    }

    public async Task<Order?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _session.Query<Order>()
            .Where(o => o.Id == id)
            .Fetch(o => o.Items)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _session.Query<Order>()
            .Fetch(o => o.Items)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> FindAsync(
        Expression<Func<Order, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _session.Query<Order>()
            .Where(predicate)
            .Fetch(o => o.Items)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order> AddAsync(Order entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;

        if (entity.Items?.Any() == true)
        {
            foreach (var item in entity.Items)
            {
                item.CreatedAt = DateTime.UtcNow;
            }
        }

        await _session.SaveAsync(entity, cancellationToken);
        return entity;
    }

    public async Task<IEnumerable<Order>> AddRangeAsync(IEnumerable<Order> entities, CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        foreach (var entity in entityList)
        {
            entity.CreatedAt = DateTime.UtcNow;

            if (entity.Items?.Any() == true)
            {
                foreach (var item in entity.Items)
                {
                    item.CreatedAt = DateTime.UtcNow;
                }
            }

            await _session.SaveAsync(entity, cancellationToken);
        }
        return entityList;
    }

    public async Task UpdateAsync(Order entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;

        if (entity.Items?.Any() == true)
        {
            foreach (var item in entity.Items)
            {
                item.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _session.UpdateAsync(entity, cancellationToken);
    }

    public async Task DeleteAsync(Order entity, CancellationToken cancellationToken = default)
    {
        await _session.DeleteAsync(entity, cancellationToken);
    }

    public async Task DeleteRangeAsync(IEnumerable<Order> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            await _session.DeleteAsync(entity, cancellationToken);
        }
    }

    public async Task<(IEnumerable<Order> Items, int Total)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var total = await _session.Query<Order>().CountAsync(cancellationToken);

        var items = await _session.Query<Order>()
            .Fetch(o => o.Items)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _session.FlushAsync(cancellationToken);
        return 0; // NHibernate doesn't return affected rows count
    }
}
