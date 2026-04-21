using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Domain.Interfaces;

/// <summary>
/// Order-specific repository interface with custom methods
/// </summary>
public interface IOrderRepository : IRepository<Order>, ITransactionalRepository
{
    /// <summary>
    /// Get orders filtered by status, with optional pagination.
    /// When page/pageSize are null, returns all matching records.
    /// </summary>
    Task<(IEnumerable<Order> Items, int Total)> GetByFilterAsync(
        long? id = null,
        string? status = null, 
        string? orderNumber = null,
        string? searchTerm = null,
        int? page = null, 
        int? pageSize = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders by customer email
    /// </summary>
    Task<IEnumerable<Order>> GetByCustomerEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders by status
    /// </summary>
    Task<IEnumerable<Order>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders within date range
    /// </summary>
    Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get order with items by order number
    /// </summary>
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
}
