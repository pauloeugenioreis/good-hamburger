using System.Collections.Generic;
using System.Linq;

namespace GoodHamburger.Domain.Dtos;

/// <summary>
/// Standard paginated response wrapper
/// </summary>
/// <typeparam name="T">Type of items in the page</typeparam>
public class PagedResponse<T>
{
    /// <summary>
    /// Current page items
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public long Total { get; set; }

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }
}
