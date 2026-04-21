---
name: new-entity
description: "Scaffold a new domain entity across all Clean Architecture layers. Use when: creating a new entity, adding a new resource, scaffolding CRUD for a new model. Creates entity, interface, repository, service, DTO, validator, mapping, controller, and tests."
argument-hint: "Entity name (e.g., Customer)"
---

# New Entity Scaffold

Creates all artifacts for a new entity following Clean Architecture patterns.

## When to Use
- Adding a new business entity to the system
- Creating a new CRUD resource end-to-end

## Repository & Service Pattern Rules

> **ALWAYS create concrete specialized artifacts.** Never use generic `IRepository<T>` or `IService<T>` directly in services or controllers.
> The generic interfaces exist as base contracts that developers may choose to use manually, but **scaffolding always generates concrete implementations**.

- Repository interface: `I{Name}Repository : IRepository<{Name}>` — in `src/Domain/Interfaces/`
- Repository class: `{Name}Repository : Repository<{Name}>, I{Name}Repository` — in `src/Data/Repository/`
- Service interface: `I{Name}Service : IService<{Name}>` — in `src/Domain/Interfaces/`
- Service class: `{Name}Service : Service<{Name}>, I{Name}Service` — injects `I{Name}Repository`, in `src/Application/Services/`
- Controller: injects `I{Name}Service` — in `src/Api/Controllers/`
- Tests: mock `Mock<I{Name}Service>` for controller tests, `Mock<I{Name}Repository>` for service tests

## Pagination Pattern

> **GET list endpoints support optional pagination.** When `page` and `pageSize` query params are provided, return `PagedResponse<T>`. When omitted, return all records.

- **Repository** `GetByFilterAsync`: accepts `int? page = null, int? pageSize = null`. Returns `(IEnumerable<T> Items, int Total)`. When page/pageSize are null, return all matching records.
- **Service**: propagates the tuple `(Items, Total)` from repository, mapping entities to DTOs.
- **Controller GET list**: accepts `[FromQuery] int? page, [FromQuery] int? pageSize`. If both present → `HandlePagedResult(items, total, page, pageSize)`. Otherwise → `Ok(items)`.
- **Excel export endpoints**: always call without page/pageSize (all records). Use `var (items, _) = await service.GetAllAsync(..., cancellationToken: ct);`
- Reference: check `src/Api/Controllers/ProductController.cs` and `src/Api/Controllers/OrderController.cs`

## Procedure

### 1. Domain Layer
1. Create entity in `src/Domain/Entities/{Name}.cs` inheriting `EntityBase`
2. Create DTO in `src/Domain/Dtos/{Name}Dto.cs` (use record)
3. Create validator in `src/Domain/Validators/{Name}Validator.cs` using FluentValidation
4. Create repository interface `I{Name}Repository : IRepository<{Name}>` in `src/Domain/Interfaces/I{Name}Repository.cs`
5. Create service interface `I{Name}Service : IService<{Name}>` in `src/Domain/Interfaces/I{Name}Service.cs`

### 2. Data Layer
6. Create EF mapping in `src/Data/Mappings/{Name}Mapping.cs` implementing `IEntityTypeConfiguration<{Name}>`
7. Add `DbSet<{Name}>` to `src/Data/Context/ApplicationDbContext.cs`
8. Create repository `{Name}Repository : Repository<{Name}>, I{Name}Repository` in `src/Data/Repository/{Name}Repository.cs`
9. Create migration: `dotnet ef migrations add Add{Name} --project src/Data --startup-project src/Api`

### 3. Application Layer
10. Create service `{Name}Service : Service<{Name}>, I{Name}Service` in `src/Application/Services/{Name}Service.cs` — inject `I{Name}Repository` (not generic `IRepository<{Name}>`)

### 4. Api Layer
11. Create controller in `src/Api/Controllers/{Name}Controller.cs` inheriting `ApiControllerBase` — inject `I{Name}Service` (not generic `IService<{Name}>`)
12. Add CRUD endpoints: GET (list with optional pagination + by id), POST, PUT, DELETE. The GET list endpoint must accept optional `page` and `pageSize` query params — when provided, use `HandlePagedResult`; when omitted, return all records

### 5. Tests
13. Create unit tests in `tests/UnitTests/Controllers/{Name}ControllerTests.cs` — mock `I{Name}Service`
14. Create integration tests in `tests/Integration/Controllers/{Name}IntegrationTests.cs`

## Reference Templates
- Entity template: check `src/Domain/Entities/Order.cs`
- Repository interface: check `src/Domain/Interfaces/IOrderRepository.cs`
- Repository implementation: check `src/Data/Repository/OrderRepository.cs`
- Service interface: check `src/Domain/Interfaces/IOrderService.cs`
- Service implementation: check `src/Application/Services/OrderService.cs`
- Controller template: check `src/Api/Controllers/OrderController.cs`
- Mapping template: check `src/Data/Mappings/`
- Test template: check `tests/UnitTests/Controllers/OrderControllerTests.cs`
