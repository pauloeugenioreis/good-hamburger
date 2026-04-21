using System.Threading;
using System.Threading.Tasks;
using GoodHamburger.Domain.Dtos;
using GoodHamburger.Web.Services;

namespace GoodHamburger.Web.Interfaces;

public interface IOrderClient
{
    Task<ApiResult<OrderResponseDto>> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult<PagedResponse<OrderResponseDto>>> GetOrdersAsync(
        long? id = null,
        string? status = null, 
        string? orderNumber = null,
        string? searchTerm = null,
        int page = 1, 
        int pageSize = 10, 
        CancellationToken cancellationToken = default);
    Task<ApiResult<bool>> UpdateOrderStatusAsync(long id, UpdateOrderStatusDto request, CancellationToken cancellationToken = default);
}
