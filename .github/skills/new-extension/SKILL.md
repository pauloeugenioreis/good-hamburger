---
name: new-extension
description: "Create a new modular infrastructure extension. Use when: adding a new infrastructure feature, integrating a new service provider, creating middleware, adding a new cross-cutting concern."
argument-hint: "Extension name (e.g., Redis Streams)"
---

# New Infrastructure Extension

Creates a modular infrastructure extension following the project's established pattern of 17+ extensions.

## When to Use
- Adding a new infrastructure feature (cache provider, message broker, etc.)
- Integrating a new third-party service
- Adding new middleware or cross-cutting concerns

## Procedure

### 1. Configuration
1. Add configuration section to `src/Domain/AppSettings.cs` under `Infrastructure`
```csharp
public class MyFeatureSettings
{
    public bool Enabled { get; set; }
    // feature-specific settings
}
```

### 2. Extension Class
2. Create `src/Infrastructure/Extensions/MyFeatureExtension.cs`
```csharp
public static class MyFeatureExtension
{
    public static IServiceCollection AddMyFeature(
        this IServiceCollection services, AppSettings settings)
    {
        if (!settings.Infrastructure.MyFeature.Enabled) return services;
        // Register services
        return services;
    }

    public static IApplicationBuilder UseMyFeature(
        this IApplicationBuilder app, AppSettings settings)
    {
        if (!settings.Infrastructure.MyFeature.Enabled) return app;
        // Configure pipeline
        return app;
    }
}
```

### 3. Registration
3. Register in `src/Infrastructure/Extensions/InfrastructureExtensions.cs`:
   - Add `services.AddMyFeature(settings);` in `AddInfrastructure`
   - Add `app.UseMyFeature(settings);` in `UseInfrastructure` (if middleware needed)

### 4. Configuration Files
4. Add default settings to `src/Api/appsettings.json` under `AppSettings.Infrastructure`
5. Document the feature in `docs/` if complex

### 5. Tests
6. Add unit test for the extension registration

## Reference
- Check existing extension: `src/Infrastructure/Extensions/CacheExtension.cs`
- Orchestrator: `src/Infrastructure/Extensions/InfrastructureExtensions.cs`
