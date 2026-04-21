using System.Collections.Generic;
using System.Linq;

namespace GoodHamburger.Domain.Dtos;

/// <summary>
/// Generic paged response wrapper for paginated API endpoints.
/// </summary>
public class PagedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public long Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
