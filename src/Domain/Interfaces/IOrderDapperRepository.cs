using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Domain.Interfaces;

/// <summary>
/// Dapper-specific interface for Order repository.
/// Inherits from IRepository of Order but uses specific interface to avoid Scrutor auto-registration.
/// </summary>
public interface IOrderDapperRepository : IRepository<Order>
{
}
