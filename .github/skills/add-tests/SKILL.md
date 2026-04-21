---
name: add-tests
description: "Generate comprehensive test suites for existing code. Use when: adding tests for untested code, improving coverage, writing unit tests, writing integration tests, testing new features."
argument-hint: "Class or component to test (e.g., OrderService)"
---

# Add Tests

Generates comprehensive unit and/or integration tests for existing code.

## When to Use
- Existing code lacks test coverage
- New feature needs test validation
- Improving code coverage metrics

## Procedure

### 1. Analyze Target
1. Read the source code to identify:
   - Public methods to test
   - Dependencies to mock (use specialized interfaces: `I{Name}Repository`, `I{Name}Service` — never generic `IRepository<T>` or `IService<T>`)
   - Code paths (happy, error, edge cases)
   - Return types and expected behaviors

### 2. Determine Test Type
2. Choose test type based on what's being tested:
   - **Unit test**: Services, validators, domain logic → `tests/UnitTests/`
   - **Integration test**: Controllers, API endpoints → `tests/Integration/`

### 3. Create Unit Tests
3. Create test file at `tests/UnitTests/{Layer}/{ClassName}Tests.cs`
4. Follow project patterns:
   - Mock all dependencies with `Mock<T>`
   - Use FluentAssertions for all assertions
   - Name: `MethodName_Scenario_ExpectedResult`
   - Structure: Arrange-Act-Assert

### 4. Create Integration Tests
5. Create test file at `tests/Integration/Controllers/{Controller}IntegrationTests.cs`
6. Use `WebApplicationFactoryFixture` for HTTP calls
7. Test actual HTTP status codes and response bodies

### 5. Validate
8. Run tests: `dotnet test tests/UnitTests/UnitTests.csproj`
9. Run integration tests: `dotnet test tests/Integration/Integration.csproj`
10. Verify all tests pass

## Coverage Goals
- Minimum 80% line coverage for services
- All public API endpoints must have integration tests
- All validators must have unit tests for valid and invalid inputs

## Reference
- Existing unit tests: `tests/UnitTests/`
- Existing integration tests: `tests/Integration/`
- Test README: [tests/UnitTests/README.md](tests/UnitTests/README.md)
