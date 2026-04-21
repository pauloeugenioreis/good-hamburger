using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GoodHamburger.Domain.Dtos;
using Xunit;

namespace GoodHamburger.Integration.Tests.Controllers;

/// <summary>
/// Integration tests for ProductController
/// </summary>
public class ProductControllerTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly HttpClient _client;

    public ProductControllerTests(WebApplicationFactoryFixture factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Product");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_ValidProduct_ReturnsCreated()
    {
        // Arrange
        var product = new CreateProductRequest
        {
            Name = "X Burger",
            Description = "Sanduíche técnico",
            Price = 5.00m,
            Category = "Sanduíches",
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Product", product);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdProduct = await response.Content.ReadFromJsonAsync<ProductResponseDto>();
        createdProduct.Should().NotBeNull();
        createdProduct!.Name.Should().Be("X Burger");
        createdProduct.Price.Should().Be(5.00m);
        createdProduct.Category.Should().Be("Sanduíches");
    }

    [Fact]
    public async Task GetMenu_ReturnsTechnicalTestMenuGroupedByCategories()
    {
        // Arrange
        await CreateProductAsync("X Burger", 5.00m, "Sanduíches");
        await CreateProductAsync("X Egg", 4.50m, "Sanduíches");
        await CreateProductAsync("X Bacon", 7.00m, "Sanduíches");
        await CreateProductAsync("Batata frita", 2.00m, "Batatas");
        await CreateProductAsync("Refrigerante", 2.50m, "Refrigerantes");

        // Act
        var response = await _client.GetAsync("/api/v1/Product/menu");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var menu = await response.Content.ReadFromJsonAsync<MenuResponseDto>();
        menu.Should().NotBeNull();
        menu!.Sandwiches.Should().HaveCount(3);
        menu.SideDishes.Should().HaveCount(2);
        menu.Sandwiches.Select(x => x.Name).Should().ContainInOrder("X Bacon", "X Burger", "X Egg");
        menu.Sandwiches.Select(x => x.Price).Should().Contain(new[] { 7.00m, 5.00m, 4.50m });
        menu.SideDishes.Select(x => x.Name).Should().ContainInOrder("Batata frita", "Refrigerante");
        menu.SideDishes.Single(x => x.Name == "Batata frita").Price.Should().Be(2.00m);
        menu.SideDishes.Single(x => x.Name == "Refrigerante").Price.Should().Be(2.50m);
    }

    [Fact]
    public async Task GetById_ExistingProduct_ReturnsProduct()
    {
        // Arrange - Create a product first
        var product = new CreateProductRequest
        {
            Name = "Test Product for GetById",
            Description = "Test",
            Price = 19.99m,
            Category = "Sanduíches",
            IsActive = true
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/Product", product);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductResponseDto>();

        // Act
        var response = await _client.GetAsync($"/api/v1/Product/{createdProduct!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrievedProduct = await response.Content.ReadFromJsonAsync<ProductResponseDto>();
        retrievedProduct.Should().NotBeNull();
        retrievedProduct!.Id.Should().Be(createdProduct.Id);
    }

    [Fact]
    public async Task GetById_NonExistentProduct_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Product/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ExistingProduct_ReturnsSuccess()
    {
        // Arrange - Create a product first
        var product = new CreateProductRequest
        {
            Name = "Original Name",
            Description = "Original Description",
            Price = 15.00m,
            Category = "Sanduíches",
            IsActive = true
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/Product", product);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductResponseDto>();

        var productToUpdate = new UpdateProductRequest
        {
            Name = "Updated Name",
            Description = createdProduct!.Description,
            Price = 25.00m,
            Category = createdProduct.Category,
            IsActive = createdProduct.IsActive
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/Product/{createdProduct.Id}", productToUpdate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update by getting the product
        var getResponse = await _client.GetAsync($"/api/v1/Product/{createdProduct.Id}");
        var updatedProduct = await getResponse.Content.ReadFromJsonAsync<ProductResponseDto>();
        updatedProduct!.Name.Should().Be("Updated Name");
        updatedProduct.Price.Should().Be(25.00m);
    }

    [Fact]
    public async Task Delete_ExistingProduct_ReturnsNoContent()
    {
        // Arrange - Create a product first
        var product = new CreateProductRequest
        {
            Name = "Product to Delete",
            Description = "Will be deleted",
            Price = 10.00m,
            Category = "Batatas",
            IsActive = true
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/Product", product);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductResponseDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/Product/{createdProduct!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's deleted
        var getResponse = await _client.GetAsync($"/api/v1/Product/{createdProduct.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ExportToExcel_WithoutFilters_ReturnsExcelFile()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Product/ExportToExcel");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        var content = await response.Content.ReadAsByteArrayAsync();
        content.Should().NotBeEmpty();
        content.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExportToExcel_WithFilters_ReturnsFilteredExcelFile()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Product/ExportToExcel?isActive=true&category=Sanduíches");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        var content = await response.Content.ReadAsByteArrayAsync();
        content.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateStatus_ExistingProduct_ReturnsNoContent()
    {
        // Arrange
        var createRequest = new CreateProductRequest
        {
            Name = "Status Product",
            Description = "Status test",
            Price = 17.50m,
            Category = "Refrigerantes",
            IsActive = true
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/Product", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponseDto>();

        var statusRequest = new UpdateProductStatusRequest
        {
            IsActive = false
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/v1/Product/{created!.Id}/status", statusRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/v1/Product/{created.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<ProductResponseDto>();
        updated!.IsActive.Should().BeFalse();
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
