---
description: "Use when writing or editing C# code. Covers naming conventions, nullable types, async patterns, and code style for this .NET 10 Clean Architecture project."
applyTo: "**/*.cs"
---

# C# Code Standards

## Language Features
- Target .NET 10.0 with `LangVersion: latest`
- Nullable reference types enabled — always handle nullability explicitly
- Use file-scoped namespaces (`namespace X;`)
- Prefer primary constructors where appropriate

## Naming
- `PascalCase` for public members, types, methods, and properties
- `_camelCase` for private fields (underscore prefix)
- `Async` suffix on all async methods (e.g., `GetByIdAsync`)
- `I` prefix for interfaces (e.g., `IRepository<T>`)

## Async
- All I/O-bound methods must be async returning `Task` or `Task<T>`
- Always pass and honor `CancellationToken`
- Never use `.Result` or `.Wait()` — always `await`

## Dependencies
- Use constructor injection — never `new` up services
- Register via Scrutor convention in `DependencyInjectionExtensions.cs`
- Never reference Infrastructure or Api from Domain layer
- Services inject specialized interfaces (`I{Name}Repository`), never generic `IRepository<T>`
- Controllers inject specialized interfaces (`I{Name}Service`), never generic `IService<T>`
- Generic interfaces exist as base contracts for developers to use manually if needed, but scaffolding always creates concrete specialized artifacts

## Error Handling
- Throw domain-specific exceptions from `Domain/Exceptions/`
- Never catch and swallow exceptions silently
- `GlobalExceptionHandler` middleware handles unhandled exceptions returning RFC 7807 `ProblemDetails`
