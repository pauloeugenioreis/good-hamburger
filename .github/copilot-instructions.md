# Project Guidelines

## Overview

Enterprise .NET 10 API template using **Clean Architecture** with multi-database support, OpenTelemetry observability, event sourcing, and multicloud storage.

## Architecture

Five-layer Clean Architecture — dependencies flow inward only:

```text
Api → Infrastructure → Application → Data → Domain
```

- **Domain**: Entities, interfaces, validators, exceptions. Zero external dependencies.
- **Data**: EF Core repositories, DbContext, migrations, seeders. Implements Domain interfaces.
- **Application**: Business services (`Service<T>` base), DTOs, orchestration logic.
- **Infrastructure**: 17 modular extensions, middleware, filters, cloud services (storage, queue, telemetry).
- **Api**: Controllers inheriting `ApiControllerBase`, Program.cs entry point.

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for detailed diagrams and principles.

## Code Style

- **Language**: C# latest with nullable reference types enabled
- **Naming**: PascalCase for public members, `_camelCase` for private fields
- **Async**: All I/O-bound methods must be async with `Async` suffix
- **DI**: Use Scrutor for auto-registration via convention. Register in `DependencyInjectionExtensions.cs`
- **Validation**: FluentValidation for all input DTOs. Validators in `Domain/Validators/`
- **Error handling**: Throw domain exceptions from `Domain/Exceptions/`. `GlobalExceptionHandler` middleware returns RFC 7807 `ProblemDetails`

## Patterns

- **Repository**: `I{Name}Repository : IRepository<{Name}>` / `{Name}Repository : Repository<{Name}>, I{Name}Repository` — every entity gets its own concrete repository interface and implementation. Generic `IRepository<T>` exists as base contract for developers to use manually if needed, but scaffolding always creates concrete specialized artifacts
- **Service**: `I{Name}Service : IService<{Name}>` / `{Name}Service : Service<{Name}>, I{Name}Service` — every entity gets its own concrete service interface and implementation. Services inject `I{Name}Repository`, never generic `IRepository<T>`. Generic `IService<T>` exists as base contract for manual use only
- **Base Controller**: `ApiControllerBase` with `HandleResult<T>` and `HandlePagedResult<T>` helpers. Controllers inject `I{Name}Service`, never generic `IService<T>`. GET list endpoints support optional pagination: when `page` and `pageSize` query params are provided, return `PagedResponse<T>` via `HandlePagedResult`; when omitted, return all records
- **Modular Extensions**: Each infrastructure concern is a separate extension method in `Infrastructure/Extensions/`
- **Hybrid ORM**: EF Core (default) + Dapper (performance). See [docs/ORM-GUIDE.md](docs/ORM-GUIDE.md)

## Build and Test

```bash
# Restore and build
dotnet restore GoodHamburger.sln
dotnet build GoodHamburger.sln --configuration Release

# Unit tests
dotnet test tests/UnitTests/UnitTests.csproj --configuration Release

# Integration tests
dotnet test tests/Integration/Integration.csproj --configuration Release

# All tests with coverage
dotnet test GoodHamburger.sln --collect:"XPlat Code Coverage" --results-directory ../coverage

# Docker
docker-compose up -d
```

## Conventions

- New entities must inherit `EntityBase` and be placed in `Domain/Entities/`
- New interfaces go in `Domain/Interfaces/` — never reference infrastructure from Domain
- EF Core mappings go in `Data/Mappings/` using `IEntityTypeConfiguration<T>`
- Database seeders go in `Data/Seeders/` and are called from `Program.cs` in Development
- All controllers use API versioning via `[ApiVersion]` attribute
- Configuration is typed via `AppSettings` class tree — never use `IConfiguration` directly in services
- When creating or updating any markdown documentation, validate the result with the checks defined in `.github/workflows/docs-check.yml` before finishing the task
- Respond in **Portuguese (Brazil)** when communicating with users
