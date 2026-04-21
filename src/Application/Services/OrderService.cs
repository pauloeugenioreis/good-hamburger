using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GoodHamburger.Domain;
using GoodHamburger.Domain.Dtos;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Exceptions;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Application.Services;

/// <summary>
/// Order service with business logic and validation
/// </summary>
public class OrderService : Service<Order>, IOrderService
{
    private const string SandwichCategory = "Sanduíches";
    private const string FriesCategory = "Batatas";
    private const string SoftDrinkCategory = "Refrigerantes";

    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<OrderService> _logger;
    private readonly OrderSettings _orderSettings;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        ILogger<OrderService> logger,
        IOptions<AppSettings> appSettings)
        : base(orderRepository, logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _logger = logger;
        _orderSettings = appSettings.Value.Infrastructure.Order;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequest dto, CancellationToken cancellationToken = default)
    {
        // Validate items
        if (dto.Items == null || dto.Items.Count == 0)
        {
            throw new ValidationException("Order must have at least one item");
        }

        return await _orderRepository.ExecuteAsync(async () =>
        {
            await using var transaction = await _orderRepository.BeginTransactionAsync(cancellationToken);

            try
            {
                // Create order entity
                var order = new Order
                {
                    OrderNumber = GenerateOrderNumber(),
                    CustomerName = dto.CustomerName,
                    CustomerEmail = dto.CustomerEmail,
                    CustomerPhone = dto.Phone,
                    ShippingAddress = dto.ShippingAddress,
                    Status = OrderStatus.Pending,
                    Notes = dto.Notes,
                    Items = new List<OrderItem>()
                };

                var orderItemData = new List<(OrderItemDto Request, Product Product, string Category)>();

                foreach (var itemDto in dto.Items)
                {
                    var product = await _productRepository.GetByIdAsync(itemDto.ProductId, cancellationToken);

                    if (product == null)
                    {
                        throw new NotFoundException($"Product {itemDto.ProductId} not found");
                    }

                    if (!product.IsActive)
                    {
                        throw new BusinessException($"Product {product.Name} is not available");
                    }

                    var normalizedCategory = NormalizeMenuCategory(product.Category);
                    if (normalizedCategory is null)
                    {
                        throw new ValidationException(
                            $"Product '{product.Name}' is not part of the technical-test menu categories");
                    }

                    orderItemData.Add((itemDto, product, normalizedCategory));
                }

                ValidateOrderComposition(orderItemData);

                decimal rawSubtotal = 0;

                foreach (var (request, product, _) in orderItemData)
                {
                    var unitPrice = product.Price;
                    var itemSubtotal = unitPrice * request.Quantity;
                    rawSubtotal += itemSubtotal;

                    var orderItem = new OrderItem
                    {
                        ProductId = request.ProductId,
                        ProductName = product.Name,
                        Quantity = request.Quantity,
                        UnitPrice = unitPrice,
                        Subtotal = itemSubtotal
                    };

                    order.Items.Add(orderItem);
                }

                var discountRate = GetDiscountRate(orderItemData.Select(x => x.Category));
                var discountAmount = rawSubtotal * discountRate;
                var discountedSubtotal = rawSubtotal - discountAmount;

                // Calculate totals
                order.Subtotal = discountedSubtotal;


                order.Total = order.Subtotal;

                // Save order
                var createdOrder = await _orderRepository.AddAsync(order, cancellationToken);
                await _orderRepository.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Order {OrderNumber} created for {CustomerEmail}. Total: {Total:C}",
                    order.OrderNumber, order.CustomerEmail, order.Total);

                return MapToResponseDto(createdOrder);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }, cancellationToken);
    }

    public async Task<OrderResponseDto> UpdateOrderStatusAsync(long id, UpdateOrderStatusDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException($"Order {id} not found");
        }

        var previousStatus = order.Status;
        if (!OrderStatus.TryNormalize(dto.Status, out var normalizedStatus))
        {
            throw new ValidationException(
                $"Status must be one of: {string.Join(", ", OrderStatus.AllowedStatuses)}");
        }

        order.Status = normalizedStatus;

        if (!string.IsNullOrEmpty(dto.Reason))
        {
            order.Notes = $"{order.Notes}\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] Status changed to {normalizedStatus}: {dto.Reason}";
        }

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Order {OrderNumber} status updated: {OldStatus} -> {NewStatus}",
            order.OrderNumber, previousStatus, normalizedStatus);

        return MapToResponseDto(order);
    }

    public async Task<OrderResponseDto?> GetOrderDetailsAsync(long id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        return order != null ? MapToResponseDto(order) : null;
    }

    public async Task<IEnumerable<OrderResponseDto>> GetOrdersByCustomerAsync(string email, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetByCustomerEmailAsync(email, cancellationToken);
        return orders.Select(MapToResponseDto).ToList();
    }

    public async Task<IEnumerable<OrderResponseDto>> GetOrdersByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        if (!OrderStatus.TryNormalize(status, out var normalizedStatus))
        {
            throw new ValidationException(
                $"Status must be one of: {string.Join(", ", OrderStatus.AllowedStatuses)}");
        }

        var orders = await _orderRepository.GetByStatusAsync(normalizedStatus, cancellationToken);
        return orders.Select(MapToResponseDto).ToList();
    }

    public async Task<(IEnumerable<OrderResponseDto> Items, int Total)> GetAllOrderDetailsAsync(
        long? id = null,
        string? status = null, 
        string? orderNumber = null,
        string? searchTerm = null,
        int? page = null, 
        int? pageSize = null, 
        CancellationToken cancellationToken = default)
    {
        var (orders, total) = await _orderRepository.GetByFilterAsync(id, status, orderNumber, searchTerm, page, pageSize, cancellationToken);
        return (orders.Select(MapToResponseDto).ToList(), total);
    }

    public async Task<decimal> CalculateOrderTotalAsync(long orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException($"Order {orderId} not found");
        }

        return order.Total;
    }

    private static OrderResponseDto MapToResponseDto(Order order)
    {
        var rawSubtotal = order.Items.Sum(i => i.Subtotal);
        var discount = rawSubtotal - order.Subtotal;
        var discountPercentage = rawSubtotal > 0
            ? decimal.Round((discount / rawSubtotal) * 100, 2)
            : 0;

        return new OrderResponseDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            CustomerPhone = order.CustomerPhone,
            ShippingAddress = order.ShippingAddress,
            Status = order.Status,
            Items = order.Items.Select(i => new OrderItemResponseDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Subtotal = i.Subtotal
            }).ToList(),
            Subtotal = order.Subtotal,
            Discount = discount,
            DiscountPercentage = discountPercentage,


            Total = order.Total,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    private static string? NormalizeMenuCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return null;
        }

        return category.Trim().ToUpperInvariant() switch
        {
            "SANDUÍCHES" => SandwichCategory,
            "SANDUICHES" => SandwichCategory,
            "BATATAS" => FriesCategory,
            "REFRIGERANTES" => SoftDrinkCategory,
            _ => null
        };
    }

    private static void ValidateOrderComposition(IReadOnlyCollection<(OrderItemDto Request, Product Product, string Category)> items)
    {
        if (items.Any(i => i.Request.Quantity != 1))
        {
            throw new ValidationException("Each order can contain only one unit of each selected item");
        }

        var grouped = items.GroupBy(i => i.Category).ToDictionary(g => g.Key, g => g.Count());

        if (!grouped.ContainsKey(SandwichCategory))
        {
            throw new ValidationException("O pedido deve conter exatamente um sanduíche");
        }

        if (grouped.GetValueOrDefault(SandwichCategory) > 1)
        {
            throw new ValidationException("O pedido pode conter apenas um sanduíche");
        }

        if (grouped.GetValueOrDefault(FriesCategory) > 1)
        {
            throw new ValidationException("O pedido pode conter apenas uma batata");
        }

        if (grouped.GetValueOrDefault(SoftDrinkCategory) > 1)
        {
            throw new ValidationException("O pedido pode conter apenas um refrigerante");
        }

        if (items.Count > 3)
        {
            throw new ValidationException("O pedido pode conter no máximo um sanduíche, uma batata e um refrigerante");
        }
    }

    private static decimal GetDiscountRate(IEnumerable<string> categories)
    {
        var categorySet = categories.ToHashSet(StringComparer.Ordinal);

        var hasSandwich = categorySet.Contains(SandwichCategory);
        var hasFries = categorySet.Contains(FriesCategory);
        var hasSoftDrink = categorySet.Contains(SoftDrinkCategory);

        if (hasSandwich && hasFries && hasSoftDrink)
        {
            return 0.20m;
        }

        if (hasSandwich && hasSoftDrink)
        {
            return 0.15m;
        }

        if (hasSandwich && hasFries)
        {
            return 0.10m;
        }

        return 0m;
    }
}
