using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GoodHamburger.Domain.Dtos;
using GoodHamburger.Web.Services;

namespace GoodHamburger.Web.Interfaces;

public interface IProductClient
{
    Task<ApiResult<MenuResponseDto>> GetMenuAsync(CancellationToken cancellationToken = default);
    
    Task<ApiResult<IReadOnlyList<ProductResponseDto>>> GetProductsAsync(
        bool? isActive = null,
        string? category = null,
        CancellationToken cancellationToken = default);

    Task<ApiResult<PagedResponse<ProductResponseDto>>> GetProductsAsync(
        int page,
        int pageSize,
        bool? isActive = null,
        string? category = null,
        CancellationToken cancellationToken = default);

    Task<ApiResult<ProductResponseDto>> GetProductByIdAsync(long id, CancellationToken cancellationToken = default);
    
    Task<ApiResult<ProductResponseDto>> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    
    Task<ApiResult<bool>> UpdateProductAsync(long id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    
    Task<ApiResult<bool>> DeleteProductAsync(long id, CancellationToken cancellationToken = default);
    
    Task<ApiResult<bool>> UpdateProductStatusAsync(long id, UpdateProductStatusRequest request, CancellationToken cancellationToken = default);
}
