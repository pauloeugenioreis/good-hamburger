using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using GoodHamburger.Domain.Dtos;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Api.Controllers;

/// <summary>
/// Order management endpoints with custom service and repository
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class OrderController : ApiControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(
        IOrderService orderService,
        ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<OrderResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(
        [FromQuery] long? id,
        [FromQuery] string? status,
        [FromQuery] string? orderNumber,
        [FromQuery] string? searchTerm,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var (items, total) = await _orderService.GetAllOrderDetailsAsync(id, status, orderNumber, searchTerm, page, pageSize, cancellationToken);

        if (page.HasValue && pageSize.HasValue)
        {
            return HandlePagedResult(items, total, page.Value, pageSize.Value);
        }

        return Ok(items);
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderDetailsAsync(id, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", id);
            return NotFound(new { message = $"Order with ID {id} not found" });
        }

        return Ok(order);
    }

    /// <summary>
    /// Get orders by customer email
    /// </summary>
    [HttpGet("customer/{email}")]
    [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCustomerAsync(string email, CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetOrdersByCustomerAsync(email, cancellationToken);
        return Ok(orders);
    }

    /// <summary>
    /// Create new order
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateOrderRequest dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(new { error = true, messages = errors });
        }

        var order = await _orderService.CreateOrderAsync(dto, cancellationToken);

        _logger.LogInformation("Order {OrderNumber} created", order.OrderNumber);

        var location = Url.Action(
            nameof(GetByIdAsync),
            values: new { id = order.Id }) ?? $"/api/v1/order/{order.Id}";

        return Created(new Uri(location, UriKind.RelativeOrAbsolute), order);
    }

    /// <summary>
    /// Update order status
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatusAsync(
        long id,
        [FromBody] UpdateOrderStatusDto dto,
        CancellationToken cancellationToken)
    {
        await _orderService.UpdateOrderStatusAsync(id, dto, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Cancel order
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrderAsync(
        long id,
        [FromBody] string? reason,
        CancellationToken cancellationToken)
    {
        var dto = new UpdateOrderStatusDto("Cancelled", reason ?? "Cancelled by user");
        var order = await _orderService.UpdateOrderStatusAsync(id, dto, cancellationToken);

        _logger.LogInformation("Order {OrderNumber} cancelled", order.OrderNumber);

        return Ok(order);
    }

    /// <summary>
    /// Export orders to Excel
    /// </summary>
    [HttpGet("ExportToExcel")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<ActionResult> ExportToExcelAsync(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating Excel report for orders");

        var stopwatch = Stopwatch.StartNew();

        var (orders, _) = await _orderService.GetAllOrderDetailsAsync(status: status, cancellationToken: cancellationToken);

        var results = orders.ToList();

        // Flatten data for Excel
        var excelData = results.SelectMany(o => o.Items.Select(i => new OrderExportDto
        {
            OrderId = o.Id,
            OrderNumber = o.OrderNumber,
            CustomerName = o.CustomerName,
            CustomerEmail = o.CustomerEmail,
            Status = o.Status,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            ItemSubtotal = i.Subtotal,
            OrderSubtotal = o.Subtotal,
            OrderTotal = o.Total,
            OrderDate = o.CreatedAt
        })).ToList();

        var config = new OpenXmlConfiguration
        {
            FastMode = true,
            EnableAutoWidth = true,
            AutoFilter = true
        };

        var memoryStream = new MemoryStream();
        await memoryStream.SaveAsAsync(excelData, sheetName: "Orders", configuration: config);

        memoryStream.Seek(0, SeekOrigin.Begin);

        stopwatch.Stop();

        _logger.LogInformation(
            "Excel generated with {Count} orders in {ElapsedMs}ms",
            results.Count,
            stopwatch.ElapsedMilliseconds);

        return File(
            memoryStream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Orders_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    /// <summary>
    /// Get order statistics
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatisticsAsync(CancellationToken cancellationToken)
    {
        var (orderItems, _) = await _orderService.GetAllOrderDetailsAsync(cancellationToken: cancellationToken);
        var orders = orderItems.ToList();

        var stats = new
        {
            totalOrders = orders.Count,
            totalRevenue = orders.Sum(o => o.Total),
            averageOrderValue = orders.Count > 0 ? orders.Average(o => o.Total) : 0,
            ordersByStatus = orders.GroupBy(o => o.Status)
                .Select(g => new { status = g.Key, count = g.Count(), revenue = g.Sum(o => o.Total) }),
            topProducts = orders
                .SelectMany(o => o.Items)
                .GroupBy(i => new { i.ProductId, i.ProductName })
                .Select(g => new
                {
                    productId = g.Key.ProductId,
                    productName = g.Key.ProductName,
                    quantitySold = g.Sum(i => i.Quantity),
                    revenue = g.Sum(i => i.Subtotal)
                })
                .OrderByDescending(x => x.revenue)
                .Take(10)
        };

        return Ok(stats);
    }
}
