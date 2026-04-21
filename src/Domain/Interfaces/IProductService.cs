using GoodHamburger.Domain.Dtos;
using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Domain.Interfaces;

/// <summary>
/// Product service interface with business logic
/// </summary>
public interface IProductService : IService<Product>
{
    Task<ProductResponseDto?> GetProductByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<(IEnumerable<ProductResponseDto> Items, int Total)> GetAllProductsAsync(bool? isActive, string? category, int? page = null, int? pageSize = null, CancellationToken cancellationToken = default);

    Task<ProductResponseDto> CreateProductAsync(CreateProductRequest dto, CancellationToken cancellationToken = default);

    Task UpdateProductAsync(long id, UpdateProductRequest dto, CancellationToken cancellationToken = default);

    Task UpdateProductStatusAsync(long id, UpdateProductStatusRequest dto, CancellationToken cancellationToken = default);
}
