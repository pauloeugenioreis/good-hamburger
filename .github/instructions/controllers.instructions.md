---
description: "Use when creating or modifying API controllers. Covers base controller patterns, versioning, route conventions, and response handling."
applyTo: "src/Api/Controllers/**"
---

# Controller Conventions

## Base Controller
- All controllers must inherit `ApiControllerBase`
- Use `HandleResult<T>` for single-item responses
- Use `HandlePagedResult<T>` for paginated list responses — only when `page` and `pageSize` query params are provided
- When pagination params are omitted, return all records with `Ok(items)`

## Routing & Versioning
- Apply `[ApiVersion("1.0")]` attribute on every controller
- Use `[Route("api/v{version:apiVersion}/[controller]")]`
- Follow REST conventions: GET, POST, PUT, DELETE

## Patterns
```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ExampleController(IExampleService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllAsync(
        [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct)
    {
        var (items, total) = await service.GetAllAsync(page, pageSize, ct);
        if (page.HasValue && pageSize.HasValue)
            return HandlePagedResult(items, total, page.Value, pageSize.Value);
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken ct)
        => HandleResult(await service.GetByIdAsync(id, ct));
}
```

## Rules
- Inject specialized service interfaces (`I{Name}Service`) — never generic `IService<T>` or repositories directly
- Return `IActionResult` from all action methods
- Include `CancellationToken` in all async endpoints
- Use `[ProducesResponseType]` for Swagger documentation
