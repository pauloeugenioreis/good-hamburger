using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;

namespace GoodHamburger.Domain.Dtos;

/// <summary>
/// DTO for creating a new order
/// </summary>
public record CreateOrderRequest
{
    /// <summary>
    /// Customer name
    /// </summary>
    public required string CustomerName { get; init; }

    /// <summary>
    /// Customer email
    /// </summary>
    public required string CustomerEmail { get; init; }

    /// <summary>
    /// Customer phone number
    /// </summary>
    public string? Phone { get; init; }

    /// <summary>
    /// Shipping address
    /// </summary>
    public required string ShippingAddress { get; init; }

    /// <summary>
    /// Order items
    /// </summary>
    public required List<OrderItemDto> Items { get; init; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; init; }
}

/// <summary>
/// DTO for order item
/// </summary>
public record OrderItemDto(
    long ProductId,
    int Quantity,
    decimal UnitPrice
);

/// <summary>
/// DTO for updating order status
/// </summary>
public record UpdateOrderStatusDto(
    string Status,
    string? Reason
);

/// <summary>
/// Response DTO for order with calculated totals
/// </summary>
public record OrderResponseDto
{
    public long Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public string? CustomerPhone { get; init; }
    public string ShippingAddress { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public List<OrderItemResponseDto> Items { get; init; } = new();
    public decimal Subtotal { get; init; }
    public decimal Discount { get; init; }
    public decimal DiscountPercentage { get; init; }
    public decimal Total { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Response DTO for order item with product details
/// </summary>
public record OrderItemResponseDto
{
    public long Id { get; init; }
    public long ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Subtotal { get; init; }
}

/// <summary>
/// DTO representing flattened order data for Excel export.
/// </summary>
public record OrderExportDto
{
    [ExcelColumn(Name = "Id do Pedido")]
    public long OrderId { get; init; }
    [ExcelColumn(Name = "Número do Pedido")]
    public string OrderNumber { get; init; } = string.Empty;
    [ExcelColumn(Name = "Nome do Cliente")]
    public string CustomerName { get; init; } = string.Empty;
    [ExcelColumn(Name = "E-mail do Cliente")]
    public string CustomerEmail { get; init; } = string.Empty;
    [ExcelColumn(Name = "Status")]
    public string Status { get; init; } = string.Empty;
    
    [ExcelColumn(Name = "Id do Produto")]
    public long ProductId { get; init; }
    [ExcelColumn(Name = "Produto")]
    public string ProductName { get; init; } = string.Empty;
    [ExcelColumn(Name = "Quantidade")]
    public int Quantity { get; init; }
    [ExcelColumn(Name = "Preço Unitário")]
    public decimal UnitPrice { get; init; }
    [ExcelColumn(Name = "Subtotal do Item")]
    public decimal ItemSubtotal { get; init; }
    
    [ExcelColumn(Name = "Subtotal do Pedido")]
    public decimal OrderSubtotal { get; init; }
    [ExcelColumn(Name = "Total do Pedido")]
    public decimal OrderTotal { get; init; }
    [ExcelColumn(Name = "Data do Pedido")]
    public DateTime OrderDate { get; init; }
}
