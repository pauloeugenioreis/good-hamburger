namespace GoodHamburger.Domain.Interfaces;

/// <summary>
/// Marker interface for ADO.NET repositories
/// Inherits from IRepository but allows separate registration in DI container
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
public interface IRepositoryAdo<TEntity> : IRepository<TEntity> where TEntity : class
{
}
