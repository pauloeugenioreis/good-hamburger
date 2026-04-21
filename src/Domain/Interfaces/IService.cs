namespace GoodHamburger.Domain.Interfaces;

/// <summary>
/// Generic service interface for business logic layer.
/// Composes query and command operations for backward compatibility.
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
public interface IService<TEntity> : IQueryService<TEntity>, ICommandService<TEntity>
    where TEntity : class
{
}
