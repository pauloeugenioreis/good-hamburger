using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using GoodHamburger.Domain;

namespace GoodHamburger.Infrastructure.Extensions;

/// <summary>
/// Main infrastructure extension that orchestrates all infrastructure components
/// </summary>
public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Core settings - MUST be registered first
        services.AddAppSettingsConfiguration(configuration);

        // Build a service provider to resolve IOptions<AppSettings> for other extensions
        // This is done once here to avoid multiple BuildServiceProvider calls
        using var tempProvider = services.BuildServiceProvider();
        var appSettings = tempProvider.GetRequiredService<IOptions<AppSettings>>();

        // Database
        services.AddDatabaseConfiguration(appSettings);

        // Cache
        services.AddCacheConfiguration(appSettings);

        // Health checks
        services.AddHealthChecksConfiguration(appSettings);

        // Telemetry (OpenTelemetry)
        services.AddTelemetry(appSettings);

        // Rate Limiting
        services.AddRateLimitingConfiguration(appSettings);

        // Event Sourcing
        services.AddEventSourcing(appSettings);

        // Authentication (JWT + OAuth2)
        services.AddJwtAuthentication();

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCorsPolicy", policy =>
            {
                if (environment.IsDevelopment())
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                }
                else
                {
                    // Configure production CORS policy
                    policy.WithOrigins("https://yourdomain.com")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                }
            });
        });

        // Response compression
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });

        // API Versioning
        services.AddCustomizedApiVersioning();

        // Application dependencies
        services.AddApplicationDependencies(appSettings.Value);

        return services;
    }

    public static IApplicationBuilder UseInfrastructureMiddleware(this IApplicationBuilder app)
    {
        // CORS
        app.UseCors("DefaultCorsPolicy");

        // Response compression
        app.UseResponseCompression();

        // Authentication & Authorization
        app.UseJwtAuthentication();

        // Rate Limiting - Only if enabled in configuration
        var serviceProvider = app.ApplicationServices;
        var appSettings = serviceProvider.GetRequiredService<AppSettings>();
        if (appSettings.Infrastructure.RateLimiting.Enabled)
        {
            app.UseRateLimiter();
        }

        // Health checks
        app.UseHealthChecks("/health");
        app.UseHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        return app;
    }
}
