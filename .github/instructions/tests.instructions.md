---
description: "Use when writing or editing unit tests or integration tests. Covers xUnit v3, Moq, FluentAssertions patterns and test conventions."
applyTo: "tests/**"
---

# Testing Conventions

## Framework Stack
- **xUnit v3** — test framework
- **Moq** — mocking
- **FluentAssertions** — assertions
- **WebApplicationFactory** — integration tests

## Naming Convention
```
MethodName_Scenario_ExpectedResult
```
Example: `GetByIdAsync_WithValidId_ReturnsProduct`

## Structure (AAA)
```csharp
[Fact]
public async Task GetByIdAsync_WithValidId_ReturnsProduct()
{
    // Arrange
    var mockRepo = new Mock<IProductRepository>();
    mockRepo.Setup(r => r.GetByIdAsync(1, default))
        .ReturnsAsync(new Product { Id = 1 });

    // Act
    var result = await service.GetByIdAsync(1, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result!.Id.Should().Be(1);
}
```

## Rules
- One assertion concept per test (multiple FluentAssertions for the same concept is fine)
- Mock specialized interfaces (`I{Name}Repository`, `I{Name}Service`), not generic `IRepository<T>` or `IService<T>`
- Use `Mock<T>` — never hit real databases in unit tests
- Integration tests use `WebApplicationFactoryFixture` with EF Core InMemory
- Always test happy path, error cases, and edge cases
- Use `CancellationToken.None` in tests

## Running
```bash
dotnet test tests/UnitTests/UnitTests.csproj --configuration Release
dotnet test tests/Integration/Integration.csproj --configuration Release
```
