---
description: "Scaffold a new CRUD entity across all Clean Architecture layers end-to-end"
agent: "agent"
argument-hint: "Entity name (e.g., Customer)"
tools: [read, search, edit, execute]
---

Create a new entity with full CRUD support across all layers:

1. **Domain**: Entity (inheriting EntityBase), DTO (record), FluentValidation validator, repository interface (`I{Name}Repository : IRepository<{Name}>`), service interface (`I{Name}Service : IService<{Name}>`)
2. **Data**: EF Core mapping (IEntityTypeConfiguration), DbSet registration, concrete repository (`{Name}Repository : Repository<{Name}>, I{Name}Repository`), migration
3. **Application**: Concrete service (`{Name}Service : Service<{Name}>, I{Name}Service`) — inject `I{Name}Repository`, NOT generic `IRepository<{Name}>`
4. **Api**: Controller (inheriting ApiControllerBase) — inject `I{Name}Service`, NOT generic `IService<{Name}>` — with GET, POST, PUT, DELETE endpoints
5. **Tests**: Unit tests mocking `I{Name}Service` (controller tests) and `I{Name}Repository` (service tests), plus integration tests

**IMPORTANT**: Always create concrete specialized interfaces and implementations. Never use generic `IRepository<T>` or `IService<T>` directly in services or controllers. The generic interfaces exist as base contracts only.

Follow the existing patterns from **Order** entity as reference (`IOrderRepository`, `OrderRepository`, `IOrderService`, `OrderService`). Ensure:
- API versioning with `[ApiVersion("1.0")]`
- CancellationToken in all async methods
- **Pagination**: GET list endpoints accept optional `page` and `pageSize` query params. Repository `GetByFilterAsync` returns `(Items, Total)` tuple. When page/pageSize provided → `HandlePagedResult`. When omitted → return all records. Excel export always fetches all records (no pagination).
- HandleResult for single-item responses, HandlePagedResult for paginated lists
- FluentValidation for all input DTOs
