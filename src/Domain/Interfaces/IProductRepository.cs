using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Domain.Interfaces;

/// <summary>
/// Product-specific repository interface
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    /// <summary>
    /// Get products filtered by active status and/or category, with optional pagination.
    /// When page/pageSize are null, returns all matching records.
    /// </summary>
    Task<(IEnumerable<Product> Items, int Total)> GetByFilterAsync(bool? isActive, string? category, int? page = null, int? pageSize = null, CancellationToken cancellationToken = default);
}
