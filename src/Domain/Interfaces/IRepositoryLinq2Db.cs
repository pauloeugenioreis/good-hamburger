namespace GoodHamburger.Domain.Interfaces;

/// <summary>
/// Marker interface for Linq2Db repositories
/// Inherits from IRepository but allows separate registration in DI container
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
public interface IRepositoryLinq2Db<TEntity> : IRepository<TEntity> where TEntity : class
{
}
