using System;
using MiniExcelLibs.Attributes;

namespace GoodHamburger.Domain.Dtos;

/// <summary>
/// Request payload for creating a product.
/// </summary>
public record CreateProductRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public required string Category { get; init; }
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// Request payload for updating a product.
/// </summary>
public record UpdateProductRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public required string Category { get; init; }
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// DTO returned by the API when interacting with products.
/// </summary>
public record ProductResponseDto
{
    [ExcelColumn(Name = "Id")]
    public long Id { get; init; }
    [ExcelColumn(Name = "Nome")]
    public string Name { get; init; } = string.Empty;
    [ExcelColumn(Name = "Descrição")]
    public string? Description { get; init; }
    [ExcelColumn(Name = "Preço")]
    public decimal Price { get; init; }
    [ExcelColumn(Name = "Categoria")]
    public string Category { get; init; } = string.Empty;
    [ExcelColumn(Name = "Ativo")]
    public bool IsActive { get; init; }
    [ExcelColumn(Name = "Data Criação")]
    public DateTime CreatedAt { get; init; }
    [ExcelColumn(Name = "Data Atualização")]
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Request payload for toggling product active flag.
/// </summary>
public record UpdateProductStatusRequest
{
    public bool? IsActive { get; init; }
}

/// <summary>
/// DTO used by menu endpoint.
/// </summary>
public record MenuItemDto
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Category { get; init; } = string.Empty;
}

/// <summary>
/// Grouped menu response.
/// </summary>
public record MenuResponseDto
{
    public IReadOnlyCollection<MenuItemDto> Sandwiches { get; init; } = [];
    public IReadOnlyCollection<MenuItemDto> SideDishes { get; init; } = [];
}
