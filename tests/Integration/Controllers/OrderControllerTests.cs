using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GoodHamburger.Domain.Dtos;
using GoodHamburger.Domain.Entities;
using Xunit;

namespace GoodHamburger.Integration.Tests.Controllers;

/// <summary>
/// Integration tests for OrderController
/// </summary>
public class OrderControllerTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly HttpClient _client;

    public OrderControllerTests(WebApplicationFactoryFixture factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Order");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_ValidOrder_ReturnsCreated()
    {
        // Arrange - Create menu products first
        var sandwichProductId = await CreateProductAsync("X Burger", 5.00m, "Sanduíches");
        var friesProductId = await CreateProductAsync("Batata frita", 2.00m, "Batatas");
        var softDrinkProductId = await CreateProductAsync("Refrigerante", 2.50m, "Refrigerantes");

        var orderDto = new CreateOrderRequest
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Phone = "+1234567890",
            ShippingAddress = "123 Main St, City, State 12345",
            Items = new List<OrderItemDto>
            {
                new(sandwichProductId, 1, 5.00m),
                new(friesProductId, 1, 2.00m),
                new(softDrinkProductId, 1, 2.50m)
            },
            Notes = "Test order"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Order", orderDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdOrder = await response.Content.ReadFromJsonAsync<OrderResponseDto>();
        createdOrder.Should().NotBeNull();
        createdOrder!.CustomerName.Should().Be("John Doe");
        createdOrder.Items.Should().HaveCount(3);
        createdOrder.Discount.Should().BeGreaterThan(0);
        createdOrder.DiscountPercentage.Should().Be(20m);
        createdOrder.Total.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetByCustomer_ReturnsOrders()
    {
        // Arrange - Create order first
        var createdProductId = await CreateProductAsync("X Burger Customer", 5.00m, "Sanduíches");

        var orderDto = new CreateOrderRequest
        {
            CustomerName = "Jane Smith",
            CustomerEmail = "jane@example.com",
            ShippingAddress = "456 Oak Ave",
            Items = new List<OrderItemDto> { new(createdProductId, 1, 5.00m) }
        };
        var orderCreateResponse = await _client.PostAsJsonAsync("/api/v1/Order", orderDto);
        orderCreateResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act
        var response = await _client.GetAsync("/api/v1/Order/customer/jane@example.com");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderResponseDto>>();
        orders.Should().NotBeNull();
        orders!.Should().NotBeEmpty("because we just created an order for this customer");
        orders[0].CustomerEmail.Should().Be("jane@example.com");
    }

    [Fact]
    public async Task UpdateStatus_ValidOrder_ReturnsSuccess()
    {
        // Arrange - Create order first
        var createdProductId = await CreateProductAsync("X Burger Status", 5.00m, "Sanduíches");

        var orderDto = new CreateOrderRequest
        {
            CustomerName = "Test User",
            CustomerEmail = "test@example.com",
            ShippingAddress = "789 Elm St",
            Items = new List<OrderItemDto> { new(createdProductId, 1, 5.00m) }
        };
        var orderResponse = await _client.PostAsJsonAsync("/api/v1/Order", orderDto);
        orderResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdOrder = await orderResponse.Content.ReadFromJsonAsync<OrderResponseDto>();
        createdOrder.Should().NotBeNull();

        var statusDto = new UpdateOrderStatusDto("Em Processamento", "Pedido em processamento");

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/v1/Order/{createdOrder!.Id}/status", statusDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update by getting the order
        var getResponse = await _client.GetAsync($"/api/v1/Order/{createdOrder.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedOrder = await getResponse.Content.ReadFromJsonAsync<OrderResponseDto>();
        updatedOrder!.Status.Should().Be("Em Processamento");
    }

    [Fact]
    public async Task GetStatistics_ReturnsStatistics()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Order/statistics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("totalOrders");
    }

    [Fact]
    public async Task ExportToExcel_WithoutFilters_ReturnsExcelFile()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Order/ExportToExcel");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        var content = await response.Content.ReadAsByteArrayAsync();
        content.Should().NotBeEmpty();
        content.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExportToExcel_WithStatusFilter_ReturnsFilteredExcelFile()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Order/ExportToExcel?status=Pendente");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        var content = await response.Content.ReadAsByteArrayAsync();
        content.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetById_ExistingOrder_ReturnsOrder()
    {
        // Arrange
        var createdProductId = await CreateProductAsync("X Burger By Id", 5.00m, "Sanduíches");

        var createOrder = new CreateOrderRequest
        {
            CustomerName = "Order By Id",
            CustomerEmail = "orderbyid@example.com",
            ShippingAddress = "Street 1",
            Items = new List<OrderItemDto> { new(createdProductId, 1, 5.00m) }
        };

        var createOrderResponse = await _client.PostAsJsonAsync("/api/v1/Order", createOrder);
        var createdOrder = await createOrderResponse.Content.ReadFromJsonAsync<OrderResponseDto>();

        // Act
        var response = await _client.GetAsync($"/api/v1/Order/{createdOrder!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<OrderResponseDto>();
        payload.Should().NotBeNull();
        payload!.Id.Should().Be(createdOrder.Id);
    }

    [Fact]
    public async Task CancelOrder_ExistingOrder_ReturnsCancelledOrder()
    {
        // Arrange
        var createdProductId = await CreateProductAsync("X Burger Cancel", 5.00m, "Sanduíches");

        var createOrder = new CreateOrderRequest
        {
            CustomerName = "Cancel Customer",
            CustomerEmail = "cancel@example.com",
            ShippingAddress = "Street 2",
            Items = new List<OrderItemDto> { new(createdProductId, 1, 5.00m) }
        };

        var createOrderResponse = await _client.PostAsJsonAsync("/api/v1/Order", createOrder);
        var createdOrder = await createOrderResponse.Content.ReadFromJsonAsync<OrderResponseDto>();

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/Order/{createdOrder!.Id}/cancel",
            "Cancelled by integration test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<OrderResponseDto>();
        payload.Should().NotBeNull();
        payload!.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task Create_WithDuplicateSandwich_ReturnsBadRequest()
    {
        // Arrange
        var burgerId = await CreateProductAsync("X Burger Duplicate", 5.00m, "Sanduíches");
        var eggId = await CreateProductAsync("X Egg Duplicate", 4.50m, "Sanduíches");

        var orderDto = new CreateOrderRequest
        {
            CustomerName = "Duplicate Test",
            CustomerEmail = "duplicate@example.com",
            ShippingAddress = "Street Duplicate",
            Items =
            [
                new OrderItemDto(burgerId, 1, 5.00m),
                new OrderItemDto(eggId, 1, 4.50m)
            ]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Order", orderDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("um sanduíche");
    }

    [Fact]
    public async Task Create_WithDuplicateFries_ReturnsBadRequest()
    {
        // Arrange
        var burgerId = await CreateProductAsync("X Burger Duplicate Fries", 5.00m, "Sanduíches");
        var friesId = await CreateProductAsync("Batata Duplicate Fries", 2.00m, "Batatas");
        var friesExtraId = await CreateProductAsync("Batata Duplicate Fries Extra", 2.00m, "Batatas");

        var orderDto = new CreateOrderRequest
        {
            CustomerName = "Duplicate Fries Test",
            CustomerEmail = "duplicate-fries@example.com",
            ShippingAddress = "Street Fries",
            Items =
            [
                new OrderItemDto(burgerId, 1, 5.00m),
                new OrderItemDto(friesId, 1, 2.00m),
                new OrderItemDto(friesExtraId, 1, 2.00m)
            ]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Order", orderDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("uma batata");
    }

    [Fact]
    public async Task Create_WithDuplicateSoftDrink_ReturnsBadRequest()
    {
        // Arrange
        var burgerId = await CreateProductAsync("X Burger Duplicate Soda", 5.00m, "Sanduíches");
        var sodaId = await CreateProductAsync("Refrigerante Duplicate Soda", 2.50m, "Refrigerantes");
        var sodaExtraId = await CreateProductAsync("Refrigerante Duplicate Soda Extra", 2.50m, "Refrigerantes");

        var orderDto = new CreateOrderRequest
        {
            CustomerName = "Duplicate Soda Test",
            CustomerEmail = "duplicate-soda@example.com",
            ShippingAddress = "Street Soda",
            Items =
            [
                new OrderItemDto(burgerId, 1, 5.00m),
                new OrderItemDto(sodaId, 1, 2.50m),
                new OrderItemDto(sodaExtraId, 1, 2.50m)
            ]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Order", orderDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("um refrigerante");
    }

    [Fact]
    public async Task Create_WithoutSandwich_ReturnsBadRequest()
    {
        // Arrange
        var friesId = await CreateProductAsync("Batata sem burger", 2.00m, "Batatas");
        var sodaId = await CreateProductAsync("Refri sem burger", 2.50m, "Refrigerantes");

        var orderDto = new CreateOrderRequest
        {
            CustomerName = "Missing Sandwich Test",
            CustomerEmail = "missing@example.com",
            ShippingAddress = "Street Missing",
            Items =
            [
                new OrderItemDto(friesId, 1, 2.00m),
                new OrderItemDto(sodaId, 1, 2.50m)
            ]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Order", orderDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("exatamente um sanduíche");
    }

    [Fact]
    public async Task GetMenu_ReturnsGroupedMenu()
    {
        // Arrange
        await CreateProductAsync("X Burger Menu", 5.00m, "Sanduíches");
        await CreateProductAsync("Batata Menu", 2.00m, "Batatas");
        await CreateProductAsync("Refri Menu", 2.50m, "Refrigerantes");

        // Act
        var response = await _client.GetAsync("/api/v1/Product/menu");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<MenuResponseDto>();
        payload.Should().NotBeNull();
        payload!.Sandwiches.Should().NotBeEmpty();
        payload.SideDishes.Should().NotBeEmpty();
    }

    private async Task<long> CreateProductAsync(string name, decimal price, string category)
    {
        var product = new CreateProductRequest
        {
            Name = name,
            Description = $"Product {name}",
            Price = price,
            Category = category,
            IsActive = true
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Product", product);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<ProductResponseDto>();
        created.Should().NotBeNull();
        return created!.Id;
    }
}
