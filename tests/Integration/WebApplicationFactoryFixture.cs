using System.Collections.Generic;
using System.Linq;
using Asp.Versioning;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using GoodHamburger.Data.Context;
using GoodHamburger.Domain;
using GoodHamburger.Domain.Interfaces;
using GoodHamburger.Integration.Tests.Support;

namespace GoodHamburger.Integration.Tests;

/// <summary>
/// Custom WebApplicationFactory for integration tests with in-memory database
/// </summary>
public class WebApplicationFactoryFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to Testing to disable Swagger in Program.cs
        builder.UseEnvironment("Testing");

        // Add test configuration with valid JWT secret
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Authentication:Enabled"] = "false", // Backward compatibility with legacy key
                ["AppSettings:Authentication:Enabled"] = "false",
                ["AppSettings:Infrastructure:Database:DatabaseType"] = "InMemory",
                ["AppSettings:Infrastructure:EventSourcing:Enabled"] = "true",
                ["AppSettings:Infrastructure:EventSourcing:Mode"] = "Hybrid",
                ["AppSettings:Infrastructure:EventSourcing:Provider"] = "Custom",
                ["AppSettings:Infrastructure:EventSourcing:EnableAuditApi"] = "true",
                ["AppSettings:Infrastructure:EventSourcing:StoreMetadata"] = "true",
                ["AppSettings:Infrastructure:EventSourcing:AuditEntities:0"] = "Order",
                ["AppSettings:Infrastructure:EventSourcing:AuditEntities:1"] = "Product"
            }!);
        });

        builder.ConfigureServices(services =>
        {
            // Remove ALL Swagger-related services to avoid OpenAPI version conflicts
            var swaggerDescriptors = services
                .Where(d => d.ServiceType.Namespace != null &&
                           (d.ServiceType.Namespace.StartsWith("Swashbuckle") ||
                            d.ServiceType.Namespace.StartsWith("Microsoft.OpenApi")))
                .ToList();

            foreach (var descriptor in swaggerDescriptors)
            {
                services.Remove(descriptor);
            }

            // Remove ALL Authentication services to avoid JWT configuration issues
            var authDescriptors = services
                .Where(d => d.ServiceType.Namespace != null &&
                           (d.ServiceType.Namespace.StartsWith("Microsoft.AspNetCore.Authentication") ||
                            d.ServiceType.FullName?.Contains("Authentication") == true))
                .ToList();

            foreach (var descriptor in authDescriptors)
            {
                services.Remove(descriptor);
            }

            // Remove the existing DbContext registration
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.RemoveAll(typeof(DbContextOptions));
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<ApplicationDbContext>));
            services.RemoveAll(typeof(IConfigureOptions<DbContextOptions<ApplicationDbContext>>));
            services.RemoveAll(typeof(IPostConfigureOptions<DbContextOptions<ApplicationDbContext>>));
            services.RemoveAll(typeof(ApplicationDbContext));
            services.RemoveAll(typeof(DbContext));

            // Add in-memory database for testing (unique per fixture instance for isolation)
            var databaseName = $"TestDb_{Guid.NewGuid()}";
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
            });

            // Register DbContext as ApplicationDbContext for Repository<T> that expects DbContext
            services.AddScoped<DbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

            // Replace Marten-based event store with an in-memory implementation for tests
            services.RemoveAll(typeof(IEventStore));
            services.AddSingleton<IEventStore, InMemoryEventStore>();

            // Ensure EventSourcing settings are available for controllers
            services.RemoveAll<EventSourcingSettings>();
            services.AddSingleton(new EventSourcingSettings
            {
                Enabled = true,
                Mode = EventSourcingMode.Hybrid,
                Provider = "Custom",
                EnableAuditApi = true,
                StoreMetadata = true,
                AuditEntities = new List<string> { "Order", "Product" }
            });

            // Configure API Versioning for tests
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("X-Api-Version"),
                    new QueryStringApiVersionReader("api-version")
                );
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure the database is created
            db.Database.EnsureCreated();

            // Seed test data if needed
            SeedTestData(db);
        });
    }

    private static void SeedTestData(ApplicationDbContext context)
    {
        // Intentionally empty: each integration test seeds only the data it needs.
    }
}
