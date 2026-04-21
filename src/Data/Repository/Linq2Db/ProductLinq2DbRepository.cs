using System.Linq.Expressions;
using LinqToDB;
using GoodHamburger.Data.Context;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Data.Repository.Linq2Db;

/// <summary>
/// Linq2Db implementation of Product repository
/// Combines LINQ expressiveness with Dapper-like performance
/// </summary>
public class ProductLinq2DbRepository : IRepository<Product>
{
    private readonly ApplicationDataConnection _db;

    public ProductLinq2DbRepository(ApplicationDataConnection db)
    {
        _db = db;
    }

    public async Task<Product?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _db.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Products.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> FindAsync(
        Expression<Func<Product, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _db.Products
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product> AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.Id = await _db.InsertWithInt64IdentityAsync(entity, token: cancellationToken);
        return entity;
    }

    public async Task<IEnumerable<Product>> AddRangeAsync(IEnumerable<Product> entities, CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        foreach (var entity in entityList)
        {
            entity.CreatedAt = DateTime.UtcNow;
        }

        await _db.BulkCopyAsync(entityList, cancellationToken);
        return entityList;
    }

    public async Task UpdateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.UpdateAsync(entity, token: cancellationToken);
    }

    public async Task DeleteAsync(Product entity, CancellationToken cancellationToken = default)
    {
        await _db.DeleteAsync(entity, token: cancellationToken);
    }

    public async Task DeleteRangeAsync(IEnumerable<Product> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            await _db.DeleteAsync(entity, token: cancellationToken);
        }
    }

    public async Task<(IEnumerable<Product> Items, int Total)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var total = await _db.Products.CountAsync(cancellationToken);

        var items = await _db.Products
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Linq2Db doesn't have a unit of work pattern
        // Changes are immediately persisted
        return Task.FromResult(0);
    }
}
