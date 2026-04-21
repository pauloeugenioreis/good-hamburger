using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using GoodHamburger.Api.Controllers;
using GoodHamburger.Domain.Dtos;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;
using Xunit;

using OrderItemDto = GoodHamburger.Domain.Dtos.OrderItemDto;

namespace GoodHamburger.UnitTests.Controllers;

/// <summary>
/// Unit tests for OrderController
/// Tests controller logic with mocked custom service
/// </summary>
public class OrderControllerTests
{
    private readonly Mock<IOrderService> _mockOrderService;
    private readonly Mock<ILogger<OrderController>> _mockLogger;
    private readonly OrderController _controller;

    public OrderControllerTests()
    {
        _mockOrderService = new Mock<IOrderService>();
        _mockLogger = new Mock<ILogger<OrderController>>();
        _controller = new OrderController(_mockOrderService.Object, _mockLogger.Object);

        // Mock IUrlHelper for Create methods
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper
            .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns("http://localhost/api/v1/Order/1");
        _controller.Url = mockUrlHelper.Object;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOkResult_WhenOrdersExist()
    {
        // Arrange
        var orders = new List<Order>
        {
            new Order
            {
                Id = 1,
                OrderNumber = "ORD-001",
                CustomerName = "João Silva",
                CustomerEmail = "joao@email.com",
                ShippingAddress = "Rua A, 123",
                Status = "Pendente",
                Total = 100.00m
            }
        };
        var orderDetails = new OrderResponseDto
        {
            Id = 1,
            OrderNumber = "ORD-001",
            CustomerName = "João Silva",
            CustomerEmail = "joao@email.com",
            CustomerPhone = "(11) 99999-9999",
            ShippingAddress = "Rua A, 123",
            Status = "Pendente",
            Subtotal = 90.00m,


            Total = 100.00m,
            Notes = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = new List<OrderItemResponseDto>()
        };
        _mockOrderService.Setup(s => s.GetAllOrderDetailsAsync(null, null, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IEnumerable<OrderResponseDto>)new List<OrderResponseDto> { orderDetails }, 1));

        // Act
        var result = await _controller.GetAllAsync(null, null, null, null, null, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var orderId = 1L;
        var orderResponse = new OrderResponseDto
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            CustomerName = "João Silva",
            CustomerEmail = "joao@email.com",
            CustomerPhone = "(11) 99999-9999",
            ShippingAddress = "Rua A, 123",
            Status = "Pendente",
            Subtotal = 100.00m,


            Total = 125.00m,
            Notes = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = new List<OrderItemResponseDto>()
        };
        _mockOrderService.Setup(s => s.GetOrderDetailsAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(orderResponse);

        // Act
        var result = await _controller.GetByIdAsync(orderId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var orderId = 999L;
        _mockOrderService.Setup(s => s.GetOrderDetailsAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync((OrderResponseDto?)null);

        // Act
        var result = await _controller.GetByIdAsync(orderId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateAsync_WithValidOrder_ReturnsCreatedAtAction()
    {
        // Arrange
        var createRequest = new CreateOrderRequest
        {
            CustomerName = "João Silva",
            CustomerEmail = "joao@email.com",
            Phone = "(11) 99999-9999",
            ShippingAddress = "Rua A, 123",
            Items = new List<OrderItemDto>
            {
                new OrderItemDto(1, 2, 50.00m)
            },
            Notes = null
        };

        var createdOrder = new OrderResponseDto
        {
            Id = 1,
            OrderNumber = "ORD-001",
            CustomerName = createRequest.CustomerName,
            CustomerEmail = createRequest.CustomerEmail,
            CustomerPhone = createRequest.Phone,
            ShippingAddress = createRequest.ShippingAddress,
            Status = "Pendente",
            Subtotal = 100.00m,


            Total = 125.00m,
            Notes = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = new List<OrderItemResponseDto>()
        };

        _mockOrderService.Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(createdOrder);

        // Act
        var result = await _controller.CreateAsync(createRequest, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedResult>();
    }

    [Fact]
    public async Task UpdateStatusAsync_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var orderId = 1L;
        var updateRequest = new UpdateOrderStatusDto("Enviado", "Pedido enviado");
        var updatedOrder = new OrderResponseDto
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            CustomerName = "João Silva",
            CustomerEmail = "joao@email.com",
            CustomerPhone = "(11) 99999-9999",
            ShippingAddress = "Rua A, 123",
            Status = "Enviado",
            Subtotal = 100.00m,


            Total = 125.00m,
            Notes = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = new List<OrderItemResponseDto>()
        };

        _mockOrderService.Setup(s => s.UpdateOrderStatusAsync(orderId, It.IsAny<UpdateOrderStatusDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedOrder);

        // Act
        var result = await _controller.UpdateStatusAsync(orderId, updateRequest, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task CancelOrderAsync_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var orderId = 1L;
        var cancelledOrder = new OrderResponseDto
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            CustomerName = "João Silva",
            CustomerEmail = "joao@email.com",
            CustomerPhone = "(11) 99999-9999",
            ShippingAddress = "Rua A, 123",
            Status = "Cancelado",
            Subtotal = 100.00m,


            Total = 125.00m,
            Notes = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = new List<OrderItemResponseDto>()
        };

        _mockOrderService.Setup(s => s.UpdateOrderStatusAsync(orderId, It.IsAny<UpdateOrderStatusDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cancelledOrder);

        // Act
        var result = await _controller.CancelOrderAsync(orderId, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByCustomerAsync_WithValidEmail_ReturnsOkResult()
    {
        // Arrange
        var email = "joao@email.com";
        var orders = new List<OrderResponseDto>
        {
            new OrderResponseDto
            {
                Id = 1,
                OrderNumber = "ORD-001",
                CustomerEmail = email,
                CustomerName = "João Silva",
                Status = "Entregue",
                Total = 100.00m,
                ShippingAddress = "Test",
                Subtotal = 90m,


                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Items = new List<OrderItemResponseDto>()
            }
        };

        _mockOrderService.Setup(s => s.GetOrdersByCustomerAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(orders);

        // Act
        var result = await _controller.GetByCustomerAsync(email, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
