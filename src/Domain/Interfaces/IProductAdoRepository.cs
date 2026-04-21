using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Domain.Interfaces;

/// <summary>
/// ADO.NET-specific interface for Product repository.
/// Inherits from IRepository of Product but uses specific interface to avoid Scrutor auto-registration.
/// </summary>
public interface IProductAdoRepository : IRepository<Product>
{
}
