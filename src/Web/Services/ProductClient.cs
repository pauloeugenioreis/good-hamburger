using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GoodHamburger.Domain.Dtos;
using GoodHamburger.Web.Interfaces;

namespace GoodHamburger.Web.Services;

public sealed class ProductClient : BaseApiClient, IProductClient
{
    public ProductClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public Task<ApiResult<MenuResponseDto>> GetMenuAsync(CancellationToken cancellationToken = default)
        => SendAsync<MenuResponseDto>(HttpMethod.Get, "api/v1/Product/menu", null, cancellationToken);

    public Task<ApiResult<IReadOnlyList<ProductResponseDto>>> GetProductsAsync(
        bool? isActive = null,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        var path = BuildPath("api/v1/Product", new Dictionary<string, string?>
        {
            ["isActive"] = isActive?.ToString().ToUpperInvariant(),
            ["category"] = category,
            ["_ts"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture)
        });

        return SendListAsync<ProductResponseDto>(HttpMethod.Get, path, cancellationToken);
    }

    public Task<ApiResult<PagedResponse<ProductResponseDto>>> GetProductsAsync(
        int page,
        int pageSize,
        bool? isActive = null,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        var path = BuildPath("api/v1/Product", new Dictionary<string, string?>
        {
            ["isActive"] = isActive?.ToString().ToUpperInvariant(),
            ["category"] = category,
            ["page"] = page.ToString(CultureInfo.InvariantCulture),
            ["pageSize"] = pageSize.ToString(CultureInfo.InvariantCulture),
            ["_ts"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture)
        });

        return SendAsync<PagedResponse<ProductResponseDto>>(HttpMethod.Get, path, null, cancellationToken);
    }

    public Task<ApiResult<ProductResponseDto>> GetProductByIdAsync(long id, CancellationToken cancellationToken = default)
        => SendAsync<ProductResponseDto>(HttpMethod.Get, $"api/v1/Product/{id}", null, cancellationToken);

    public Task<ApiResult<ProductResponseDto>> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
        => SendAsync<ProductResponseDto>(HttpMethod.Post, "api/v1/Product", request, cancellationToken);

    public Task<ApiResult<bool>> UpdateProductAsync(long id, UpdateProductRequest request, CancellationToken cancellationToken = default)
        => SendNoContentAsync(HttpMethod.Put, $"api/v1/Product/{id}", request, cancellationToken);

    public Task<ApiResult<bool>> DeleteProductAsync(long id, CancellationToken cancellationToken = default)
        => SendNoContentAsync(HttpMethod.Delete, $"api/v1/Product/{id}", null, cancellationToken);

    public Task<ApiResult<bool>> UpdateProductStatusAsync(long id, UpdateProductStatusRequest request, CancellationToken cancellationToken = default)
        => SendNoContentAsync(HttpMethod.Patch, $"api/v1/Product/{id}/status", request, cancellationToken);
}
