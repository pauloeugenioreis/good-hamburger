using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using GoodHamburger.Domain.Dtos;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;
using Xunit;

namespace GoodHamburger.Integration.Tests.Controllers;

/// <summary>
/// Integration tests for AuditController to guarantee the Event Sourcing surface stays functional.
/// </summary>
public class AuditControllerTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly HttpClient _client;

    public AuditControllerTests(WebApplicationFactoryFixture factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetEntityHistory_ReturnsAllRecordedEvents()
    {
        // Arrange
        var orderId = await SeedOrderLifecycleAsync(additionalUpdates: 1);

        // Act
        var response = await _client.GetAsync($"/api/v1/Audit/Order/{orderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var events = await response.Content.ReadFromJsonAsync<List<DomainEvent>>();
        events.Should().NotBeNull();
        events!.Count.Should().BeGreaterThan(1, "create + update events must exist");
        events.Select(e => e.EventType).Should().Contain(new[] { "OrderCreatedEvent", "OrderUpdatedEvent" });
    }

    [Fact]
    public async Task TimeTravel_ReturnsStateUpToCutoffTimestamp()
    {
        // Arrange
        var product = await CreateProductAsync();
        var order = await CreateOrderAsync(product.Id);
        var cutoff = DateTime.UtcNow;

        await Task.Delay(TimeSpan.FromMilliseconds(10));
        await UpdateOrderStatusAsync(order.Id, "Processing");

        // Act
        var response = await _client.GetAsync($"/api/v1/Audit/Order/{order.Id}/at/{Uri.EscapeDataString(cutoff.ToString("O"))}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<AuditStateResponse>();
        payload.Should().NotBeNull();
        payload!.EventCount.Should().Be(1);
        payload.Events.Should().ContainSingle();
        payload.Events[0].EventType.Should().Be("OrderCreatedEvent");
    }

    [Fact]
    public async Task ReplayEvents_ReturnsOrderedTimeline()
    {
        // Arrange
        var orderId = await SeedOrderLifecycleAsync(additionalUpdates: 2);

        // Act
        var response = await _client.PostAsync($"/api/v1/Audit/Order/{orderId}/replay", content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ReplayResponse>();
        payload.Should().NotBeNull();
        payload!.EventCount.Should().Be(payload.Events.Count);
        payload.Events.Should().BeInAscendingOrder(e => e.Version);
        payload.Events.Select(e => e.EventType).Should().Contain(new[] { "OrderCreatedEvent", "OrderUpdatedEvent" });
    }

    [Fact]
    public async Task GetEventsByVersion_ReturnsFilteredRange()
    {
        // Arrange
        var orderId = await SeedOrderLifecycleAsync(additionalUpdates: 2);

        // Act
        var response = await _client.GetAsync($"/api/v1/Audit/Order/{orderId}/versions/1/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var events = await response.Content.ReadFromJsonAsync<List<DomainEvent>>();
        events.Should().NotBeNull();
        events!.Should().ContainSingle();
        events[0].Version.Should().Be(1);
    }

    [Fact]
    public async Task GetEventsByType_ReturnsOrderEvents()
    {
        // Arrange
        await SeedOrderLifecycleAsync(additionalUpdates: 1);

        // Act
        var response = await _client.GetAsync("/api/v1/Audit/type/Order?limit=50");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var events = await response.Content.ReadFromJsonAsync<List<DomainEvent>>();
        events.Should().NotBeNull();
        events!.Should().NotBeEmpty();
        events.Should().OnlyContain(e => e.AggregateType == "Order");
    }

    [Fact]
    public async Task GetEventsByUser_ReturnsOkPayload()
    {
        // Arrange
        await SeedOrderLifecycleAsync(additionalUpdates: 1);

        // Act
        var response = await _client.GetAsync("/api/v1/Audit/user/integration-user?limit=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<AuditUserResponse>();
        payload.Should().NotBeNull();
        payload!.UserId.Should().Be("integration-user");
        payload.EventCount.Should().BeGreaterThanOrEqualTo(0);
        payload.Events.Should().NotBeNull();
    }

    [Fact]
    public async Task GetStatistics_ReturnsAggregatedMetrics()
    {
        // Arrange
        await SeedOrderLifecycleAsync(additionalUpdates: 1);

        // Act
        var response = await _client.GetAsync("/api/v1/Audit/statistics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<EventStatistics>();
        payload.Should().NotBeNull();
        payload!.TotalEvents.Should().BeGreaterThan(0);
        payload.EventsByAggregateType.Should().ContainKey("Order");
    }

    private async Task<long> SeedOrderLifecycleAsync(int additionalUpdates)
    {
        var product = await CreateProductAsync();
        var order = await CreateOrderAsync(product.Id);

        var statuses = new[] { "Processing", "Shipped", "Delivered" };
        for (var i = 0; i < additionalUpdates && i < statuses.Length; i++)
        {
            await UpdateOrderStatusAsync(order.Id, statuses[i]);
        }

        return order.Id;
    }

    private async Task<ProductResponseDto> CreateProductAsync()
    {
        var dto = new CreateProductRequest
        {
            Name = $"Audit Product {Guid.NewGuid():N}",
            Description = "Product created for audit integration tests",
            Price = 99.90m,
            Category = "Sanduíches",
            IsActive = true
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Product", dto);
        response.EnsureSuccessStatusCode();

        var product = await response.Content.ReadFromJsonAsync<ProductResponseDto>();
        return product ?? throw new InvalidOperationException("Product creation failed");
    }

    private async Task<OrderResponseDto> CreateOrderAsync(long productId)
    {
        var dto = new CreateOrderRequest
        {
            CustomerName = "Audit Customer",
            CustomerEmail = $"audit+{Guid.NewGuid():N}@example.com",
            Phone = "+5511999999999",
            ShippingAddress = "Rua Audit, 123 - São Paulo",
            Notes = "Integration test order",
            Items = new List<OrderItemDto> { new(productId, 1, 99.90m) }
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Order", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var order = await response.Content.ReadFromJsonAsync<OrderResponseDto>();
        return order ?? throw new InvalidOperationException("Order creation failed");
    }

    private async Task UpdateOrderStatusAsync(long orderId, string status)
    {
        var dto = new UpdateOrderStatusDto(status, $"Status changed to {status} during integration tests");
        var response = await _client.PatchAsJsonAsync($"/api/v1/Order/{orderId}/status", dto);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private sealed record AuditStateResponse(
        string EntityType,
        string EntityId,
        DateTime Timestamp,
        int EventCount,
        List<AuditStateEvent> Events);

    private sealed record AuditStateEvent(
        string EventType,
        int Version,
        DateTime OccurredOn,
        string? UserId);

    private sealed record ReplayResponse(
        string EntityType,
        string EntityId,
        int EventCount,
        string ReconstructedState,
        List<ReplayEvent> Events);

    private sealed record ReplayEvent(
        string EventType,
        int Version,
        DateTime OccurredOn);

    private sealed record AuditUserResponse(
        string UserId,
        int EventCount,
        List<JsonElement> Events);
}
