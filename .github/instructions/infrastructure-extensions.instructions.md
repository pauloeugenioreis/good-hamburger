---
description: "Use when creating or modifying infrastructure extensions, middleware, or service registrations. Covers the modular extension pattern used in this project."
applyTo: "src/Infrastructure/Extensions/**"
---

# Infrastructure Extensions

## Pattern
Each infrastructure concern is a separate static extension method class in `Infrastructure/Extensions/`. The orchestrator `InfrastructureExtensions.cs` calls all individual extensions.

## Creating a New Extension
```csharp
public static class MyFeatureExtension
{
    public static IServiceCollection AddMyFeature(
        this IServiceCollection services,
        AppSettings settings)
    {
        if (!settings.Infrastructure.MyFeature.Enabled)
            return services;

        // Register services
        return services;
    }

    public static IApplicationBuilder UseMyFeature(
        this IApplicationBuilder app,
        AppSettings settings)
    {
        if (!settings.Infrastructure.MyFeature.Enabled)
            return app;

        // Configure middleware
        return app;
    }
}
```

## Rules
- Check `AppSettings` feature flag before registering services
- Return `services`/`app` for fluent chaining
- Register the extension call in `InfrastructureExtensions.cs`
- Add corresponding configuration section in `AppSettings.cs` under `Infrastructure`
- Feature must be opt-in (disabled by default unless core functionality)

## Existing Extensions (17)
AppSettings, Database, Cache, HealthChecks, Swagger, DependencyInjection, Authentication, ApiVersioning, Telemetry, RateLimiting, EventSourcing, Quartz, RabbitMQ, MongoDB, Storage, Logging
