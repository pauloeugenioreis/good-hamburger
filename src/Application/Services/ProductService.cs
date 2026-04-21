using Microsoft.Extensions.Logging;
using GoodHamburger.Domain.Dtos;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Exceptions;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Application.Services;

/// <summary>
/// Product service with business logic, filtering and mapping
/// </summary>
public class ProductService(
    IProductRepository repository,
    ILogger<ProductService> logger) : Service<Product>(repository, logger), IProductService
{
    public async Task<ProductResponseDto?> GetProductByIdAsync(long id, CancellationToken cancellationToken = default)
    {

        var product = await repository.GetByIdAsync(id, cancellationToken);
        return product is not null ? MapToResponse(product) : null;
    }

    public async Task<(IEnumerable<ProductResponseDto> Items, int Total)> GetAllProductsAsync(
        bool? isActive,
        string? category,
        int? page = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var (products, total) = await repository.GetByFilterAsync(isActive, category, page, pageSize, cancellationToken);
        return (products.Select(MapToResponse).ToList(), total);
    }

    public async Task<ProductResponseDto> CreateProductAsync(CreateProductRequest dto, CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Category = dto.Category,
            IsActive = dto.IsActive
        };

        var created = await repository.AddAsync(product, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Product {ProductName} created with ID {ProductId}", created.Name, created.Id);

        return MapToResponse(created);
    }

    public async Task UpdateProductAsync(long id, UpdateProductRequest dto, CancellationToken cancellationToken = default)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Product with ID {id} not found");

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.Category = dto.Category;
        product.IsActive = dto.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(product, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Product {ProductId} updated", id);
    }

    public async Task UpdateProductStatusAsync(long id, UpdateProductStatusRequest dto, CancellationToken cancellationToken = default)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Product with ID {id} not found");

        product.IsActive = dto.IsActive ?? product.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(product, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Product {ProductId} status changed to {Status}",
            id, product.IsActive ? "active" : "inactive");
    }

    private static ProductResponseDto MapToResponse(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        Category = product.Category,
        IsActive = product.IsActive,
        CreatedAt = product.CreatedAt,
        UpdatedAt = product.UpdatedAt
    };
}
