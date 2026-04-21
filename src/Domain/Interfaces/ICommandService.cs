namespace GoodHamburger.Domain.Interfaces;

/// <summary>
/// Command-only service operations (write side of CQRS-lite)
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
public interface ICommandService<TEntity> where TEntity : class
{
    Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(long id, TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
