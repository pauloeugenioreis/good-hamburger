using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using GoodHamburger.Application.Services;
using GoodHamburger.Domain;
using GoodHamburger.Domain.Dtos;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Exceptions;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.UnitTests.Services;

public class OrderServiceDiscountTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly OrderService _service;

    public OrderServiceDiscountTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();

        var appSettings = Options.Create(new AppSettings
        {
            Infrastructure = new InfrastructureSettings
            {
                Order = new OrderSettings
                {
                    FreeShippingThreshold = 0m
                }
            }
        });

        _orderRepositoryMock
            .Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FakeRepositoryTransaction());

        _orderRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order order, CancellationToken _) =>
            {
                order.Id = 1;
                return order;
            });

        _orderRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _productRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _service = new OrderService(
            _orderRepositoryMock.Object,
            _productRepositoryMock.Object,
            new Mock<ILogger<OrderService>>().Object,
            appSettings);
    }

    [Fact]
    public async Task CreateOrderAsync_WithSandwichFriesAndSoftDrink_AppliesTwentyPercentDiscount()
    {
        // Arrange
        SetupProduct(1, "X Burger", 5.00m, "Sanduíches");
        SetupProduct(2, "Batata frita", 2.00m, "Batatas");
        SetupProduct(3, "Refrigerante", 2.50m, "Refrigerantes");

        var request = CreateOrderRequestWithItems(
            new OrderItemDto(1, 1, 999m),
            new OrderItemDto(2, 1, 999m),
            new OrderItemDto(3, 1, 999m));

        // Act
        var result = await _service.CreateOrderAsync(request, CancellationToken.None);

        // Assert
        result.Subtotal.Should().Be(7.60m);
        result.Discount.Should().Be(1.90m);
        result.DiscountPercentage.Should().Be(20m);
        result.Total.Should().Be(7.60m);
    }

    [Fact]
    public async Task CreateOrderAsync_WithSandwichAndSoftDrink_AppliesFifteenPercentDiscount()
    {
        // Arrange
        SetupProduct(1, "X Egg", 4.50m, "Sanduíches");
        SetupProduct(2, "Refrigerante", 2.50m, "Refrigerantes");

        var request = CreateOrderRequestWithItems(
            new OrderItemDto(1, 1, 0m),
            new OrderItemDto(2, 1, 0m));

        // Act
        var result = await _service.CreateOrderAsync(request, CancellationToken.None);

        // Assert
        result.Subtotal.Should().Be(5.95m);
        result.Discount.Should().Be(1.05m);
        result.DiscountPercentage.Should().Be(15m);
    }

    [Fact]
    public async Task CreateOrderAsync_WithSandwichAndFries_AppliesTenPercentDiscount()
    {
        // Arrange
        SetupProduct(1, "X Bacon", 7.00m, "Sanduíches");
        SetupProduct(2, "Batata frita", 2.00m, "Batatas");

        var request = CreateOrderRequestWithItems(
            new OrderItemDto(1, 1, 0m),
            new OrderItemDto(2, 1, 0m));

        // Act
        var result = await _service.CreateOrderAsync(request, CancellationToken.None);

        // Assert
        result.Subtotal.Should().Be(8.10m);
        result.Discount.Should().Be(0.90m);
        result.DiscountPercentage.Should().Be(10m);
    }

    [Fact]
    public async Task CreateOrderAsync_WithTwoSandwiches_ThrowsValidationException()
    {
        // Arrange
        SetupProduct(1, "X Burger", 5.00m, "Sanduíches");
        SetupProduct(2, "X Egg", 4.50m, "Sanduíches");

        var request = CreateOrderRequestWithItems(
            new OrderItemDto(1, 1, 0m),
            new OrderItemDto(2, 1, 0m));

        // Act
        Func<Task> act = async () => await _service.CreateOrderAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*apenas um sanduíche*");
    }

    [Fact]
    public async Task CreateOrderAsync_WithTwoFries_ThrowsValidationException()
    {
        // Arrange
        SetupProduct(1, "X Burger", 5.00m, "Sanduíches");
        SetupProduct(2, "Batata frita", 2.00m, "Batatas");
        SetupProduct(3, "Batata frita extra", 2.00m, "Batatas");

        var request = CreateOrderRequestWithItems(
            new OrderItemDto(1, 1, 0m),
            new OrderItemDto(2, 1, 0m),
            new OrderItemDto(3, 1, 0m));

        // Act
        Func<Task> act = async () => await _service.CreateOrderAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*apenas uma batata*");
    }

    [Fact]
    public async Task CreateOrderAsync_WithTwoSoftDrinks_ThrowsValidationException()
    {
        // Arrange
        SetupProduct(1, "X Burger", 5.00m, "Sanduíches");
        SetupProduct(2, "Refrigerante", 2.50m, "Refrigerantes");
        SetupProduct(3, "Refrigerante extra", 2.50m, "Refrigerantes");

        var request = CreateOrderRequestWithItems(
            new OrderItemDto(1, 1, 0m),
            new OrderItemDto(2, 1, 0m),
            new OrderItemDto(3, 1, 0m));

        // Act
        Func<Task> act = async () => await _service.CreateOrderAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*apenas um refrigerante*");
    }

    [Fact]
    public async Task CreateOrderAsync_WithoutSandwich_ThrowsValidationException()
    {
        // Arrange
        SetupProduct(1, "Batata frita", 2.00m, "Batatas");
        SetupProduct(2, "Refrigerante", 2.50m, "Refrigerantes");

        var request = CreateOrderRequestWithItems(
            new OrderItemDto(1, 1, 0m),
            new OrderItemDto(2, 1, 0m));

        // Act
        Func<Task> act = async () => await _service.CreateOrderAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*exatamente um sanduíche*");
    }

    private void SetupProduct(long id, string name, decimal price, string category)
    {
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product
            {
                Id = id,
                Name = name,
                Price = price,
                Category = category,
                IsActive = true
            });
    }

    private static CreateOrderRequest CreateOrderRequestWithItems(params OrderItemDto[] items)
    {
        return new CreateOrderRequest
        {
            CustomerName = "Teste",
            CustomerEmail = "teste@goodhamburger.com",
            Phone = "11999999999",
            ShippingAddress = "Rua Teste, 100",
            Items = items.ToList(),
            Notes = null
        };
    }

    private sealed class FakeRepositoryTransaction : IRepositoryTransaction
    {
        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
