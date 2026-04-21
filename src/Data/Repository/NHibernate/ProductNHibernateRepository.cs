using System.Linq.Expressions;
using NHibernate;
using NHibernate.Linq;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Data.Repository.NHibernate;

/// <summary>
/// NHibernate implementation of Product repository
/// Enterprise-grade ORM with advanced features
/// </summary>
public class ProductNHibernateRepository : IRepository<Product>
{
    private readonly ISession _session;

    public ProductNHibernateRepository(ISession session)
    {
        _session = session;
    }

    public async Task<Product?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _session.GetAsync<Product>(id, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _session.Query<Product>().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> FindAsync(
        Expression<Func<Product, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _session.Query<Product>()
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product> AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _session.SaveAsync(entity, cancellationToken);
        return entity;
    }

    public async Task<IEnumerable<Product>> AddRangeAsync(IEnumerable<Product> entities, CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        foreach (var entity in entityList)
        {
            entity.CreatedAt = DateTime.UtcNow;
            await _session.SaveAsync(entity, cancellationToken);
        }
        return entityList;
    }

    public async Task UpdateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _session.UpdateAsync(entity, cancellationToken);
    }

    public async Task DeleteAsync(Product entity, CancellationToken cancellationToken = default)
    {
        await _session.DeleteAsync(entity, cancellationToken);
    }

    public async Task DeleteRangeAsync(IEnumerable<Product> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            await _session.DeleteAsync(entity, cancellationToken);
        }
    }

    public async Task<(IEnumerable<Product> Items, int Total)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var total = await _session.Query<Product>().CountAsync(cancellationToken);

        var items = await _session.Query<Product>()
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
