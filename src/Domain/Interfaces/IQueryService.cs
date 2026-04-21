namespace GoodHamburger.Domain.Interfaces;

/// <summary>
/// Query-only service operations (read side of CQRS-lite)
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
public interface IQueryService<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<TEntity> Items, int Total)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
