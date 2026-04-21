namespace GoodHamburger.Domain.Entities;

/// <summary>
/// Order entity representing a customer order
/// </summary>
public class Order : EntityBase
{
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string Status { get; set; } = "Pendente"; // Pendente, Em Processamento, Enviado, Entregue, Cancelado
    public decimal Subtotal { get; set; }

    public decimal Total { get; set; }
    public string? Notes { get; set; }

    // Navigation property
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

/// <summary>
/// Order item entity representing products in an order
/// </summary>
public class OrderItem : EntityBase
{
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }

    // Navigation properties
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
