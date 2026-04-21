using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GoodHamburger.Domain.Dtos;
using GoodHamburger.Web.Interfaces;

namespace GoodHamburger.Web.Services;

public sealed class OrderClient : BaseApiClient, IOrderClient
{
    public OrderClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public Task<ApiResult<OrderResponseDto>> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
        => SendAsync<OrderResponseDto>(HttpMethod.Post, "api/v1/Order", request, cancellationToken);

    public Task<ApiResult<PagedResponse<OrderResponseDto>>> GetOrdersAsync(
        long? id = null,
        string? status = null, 
        string? orderNumber = null,
        string? searchTerm = null,
        int page = 1, 
        int pageSize = 10, 
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string?>
        {
            ["id"] = id?.ToString(),
            ["status"] = status,
            ["orderNumber"] = orderNumber,
            ["searchTerm"] = searchTerm,
            ["page"] = page.ToString(),
            ["pageSize"] = pageSize.ToString()
        };

        return SendAsync<PagedResponse<OrderResponseDto>>(HttpMethod.Get, BuildPath("api/v1/Order", query), null, cancellationToken);
    }

    public Task<ApiResult<bool>> UpdateOrderStatusAsync(long id, UpdateOrderStatusDto request, CancellationToken cancellationToken = default)
        => SendNoContentAsync(HttpMethod.Patch, $"api/v1/Order/{id}/status", request, cancellationToken);
}
