using Microsoft.AspNetCore.Mvc;
using GoodHamburger.Domain.Dtos;

namespace GoodHamburger.Api.Controllers;

/// <summary>
/// Base controller with common functionality
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult HandleResult<T>(T? result)
    {
        if (object.Equals(result, default(T)))
        {
            return NotFound();
        }

        return Ok(result);
    }

    protected IActionResult HandlePagedResult<T>(IEnumerable<T> items, long total, int page, int pageSize)
    {
        var safeTotal = total;

        if (safeTotal < 0)
        {
            safeTotal = 0;
        }

        long totalPages = 0;

        if (pageSize > 0)
        {
            totalPages = (long)Math.Ceiling(safeTotal / (double)pageSize);
        }

        int safeTotalPages;

        if (totalPages > int.MaxValue)
        {
            safeTotalPages = int.MaxValue;
        }
        else
        {
            safeTotalPages = (int)totalPages;
        }

        var response = new PagedResponse<T>
        {
            Items = items,
            Total = safeTotal,
            Page = page,
            PageSize = pageSize,
            TotalPages = safeTotalPages
        };

        return Ok(response);
    }
}
