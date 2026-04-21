using System.Linq.Expressions;
using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Domain.Interfaces;

/// <summary>
/// Generic repository interface for MongoDB document stores.
/// Uses string Id (ObjectId) and has no SaveChangesAsync — MongoDB persists immediately.
/// Designed as a parallel to IRepository&lt;T&gt; (EF Core) for NoSQL workloads.
/// </summary>
/// <typeparam name="TEntity">Document entity type inheriting MongoEntityBase</typeparam>
public interface IMongoRepository<TEntity> where TEntity : MongoEntityBase
{
    Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<(IEnumerable<TEntity> Items, long Total)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<long> CountAsync(CancellationToken cancellationToken = default);
}
