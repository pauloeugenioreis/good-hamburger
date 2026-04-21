using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using GoodHamburger.Api.Controllers;
using GoodHamburger.Domain.Dtos;
using GoodHamburger.Domain.Exceptions;
using GoodHamburger.Domain.Interfaces;
using Xunit;

namespace GoodHamburger.UnitTests.Controllers;

/// <summary>
/// Unit tests for ProductController
/// Tests controller logic with mocked dependencies
/// </summary>
public class ProductControllerTests
{
    private readonly Mock<IProductService> _mockService;
    private readonly Mock<ILogger<ProductController>> _mockLogger;
    private readonly ProductController _controller;

    public ProductControllerTests()
    {
        _mockService = new Mock<IProductService>();
        _mockLogger = new Mock<ILogger<ProductController>>();
        _controller = new ProductController(_mockService.Object, _mockLogger.Object);

        // Mock IUrlHelper for Create methods
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper
            .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns("http://localhost/api/v1/Product/1");
        _controller.Url = mockUrlHelper.Object;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOkResult_WhenProductsExist()
    {
        // Arrange
        var products = new List<ProductResponseDto>
        {
            new() { Id = 1, Name = "X Burger", Price = 5.00m, Category = "Sanduíches", IsActive = true },
            new() { Id = 2, Name = "X Egg", Price = 4.50m, Category = "Sanduíches", IsActive = true }
        };
        _mockService.Setup(s => s.GetAllProductsAsync(null, null, null, null, It.IsAny<CancellationToken>())).ReturnsAsync((products, products.Count));

        // Act
        var result = await _controller.GetAllAsync(null, null, null, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var productId = 1L;
        var product = new ProductResponseDto
        {
            Id = productId,
            Name = "X Bacon",
            Price = 7.00m,
            Category = "Sanduíches",
            IsActive = true
        };
        _mockService.Setup(s => s.GetProductByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        // Act
        var result = await _controller.GetByIdAsync(productId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var productId = 999L;
        _mockService.Setup(s => s.GetProductByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync((ProductResponseDto?)null);

        // Act
        var result = await _controller.GetByIdAsync(productId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var newProduct = new CreateProductRequest
        {
            Name = "X Burger",
            Description = "Sanduíche técnico",
            Price = 5.00m,
            Category = "Sanduíches",
            IsActive = true
        };
        var createdProduct = new ProductResponseDto
        {
            Id = 1,
            Name = newProduct.Name,
            Price = newProduct.Price,
            Category = newProduct.Category,
            IsActive = newProduct.IsActive
        };
        _mockService.Setup(s => s.CreateProductAsync(newProduct, It.IsAny<CancellationToken>())).ReturnsAsync(createdProduct);

        // Act
        var result = await _controller.CreateAsync(newProduct, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedResult>();
    }

    [Fact]
    public async Task GetMenuAsync_ReturnsOkResult_WithTechnicalTestMenu()
    {
        // Arrange
        var products = new List<ProductResponseDto>
        {
            new() { Id = 1, Name = "X Burger", Price = 5.00m, Category = "Sanduíches", IsActive = true },
            new() { Id = 2, Name = "X Egg", Price = 4.50m, Category = "Sanduíches", IsActive = true },
            new() { Id = 3, Name = "X Bacon", Price = 7.00m, Category = "Sanduíches", IsActive = true },
            new() { Id = 4, Name = "Batata frita", Price = 2.00m, Category = "Batatas", IsActive = true },
            new() { Id = 5, Name = "Refrigerante", Price = 2.50m, Category = "Refrigerantes", IsActive = true }
        };

        _mockService
            .Setup(s => s.GetAllProductsAsync(true, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, products.Count));

        // Act
        var result = await _controller.GetMenuAsync(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var payload = okResult!.Value as MenuResponseDto;
        payload.Should().NotBeNull();
        payload!.Sandwiches.Should().HaveCount(3);
        payload.SideDishes.Should().HaveCount(2);
        payload.Sandwiches.Select(x => x.Name).Should().ContainInOrder("X Bacon", "X Burger", "X Egg");
        payload.SideDishes.Select(x => x.Name).Should().ContainInOrder("Batata frita", "Refrigerante");
        payload.Sandwiches.Single(x => x.Name == "X Burger").Price.Should().Be(5.00m);
        payload.SideDishes.Single(x => x.Name == "Refrigerante").Price.Should().Be(2.50m);
    }

    [Fact]
    public async Task UpdateAsync_WithValidDto_ReturnsNoContent()
    {
        // Arrange
        var productId = 1L;
        var updatedProduct = new UpdateProductRequest
        {
            Name = "X Burger",
            Price = 5.00m,
            Category = "Sanduíches",
            IsActive = true
        };

        _mockService.Setup(s => s.UpdateProductAsync(productId, updatedProduct, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateAsync(productId, updatedProduct, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var productId = 999L;
        var product = new UpdateProductRequest
        {
            Name = "X Egg",
            Description = "Sanduíche técnico",
            Price = 4.50m,
            Category = "Sanduíches",
            IsActive = true
        };
        _mockService.Setup(s => s.UpdateProductAsync(productId, product, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Product with ID {productId} not found"));

        // Act
        var act = () => _controller.UpdateAsync(productId, product, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateStatusAsync_WithValidPayload_ReturnsNoContent()
    {
        // Arrange
        var productId = 1L;
        var dto = new UpdateProductStatusRequest { IsActive = false };

        _mockService.Setup(s => s.UpdateProductStatusAsync(productId, dto, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateStatusAsync(productId, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var productId = 1L;
        _mockService.Setup(s => s.DeleteAsync(productId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteAsync(productId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var productId = 999L;
        _mockService.Setup(s => s.DeleteAsync(productId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Entity with ID {productId} not found"));

        // Act
        var act = () => _controller.DeleteAsync(productId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
