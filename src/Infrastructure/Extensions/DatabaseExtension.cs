using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using GoodHamburger.Data.Context;
using GoodHamburger.Domain;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;
using GoodHamburger.Infrastructure.Services;
// using LinqToDB.AspNet; // Uncomment when using Linq2Db

namespace GoodHamburger.Infrastructure.Extensions;

/// <summary>
/// Database configuration extension supporting multiple ORMs
///
/// IMPORTANT: Entity Framework Core is the DEFAULT ORM and is enabled by default.
///
/// ==================================================================================
/// QUICK START - How to switch ORMs:
/// ==================================================================================
/// 1. DAPPER (Ready to use immediately):
///    - Comment line 41: services.AddEntityFramework(...)
///    - Uncomment line 47: services.AddDapper(...)
///    - Run the project!
///
/// 2. NHIBERNATE (Requires package installation):
///    - Uncomment NHibernate packages in src/Data/Data.csproj (lines ~31-32)
///    - Remove <Compile Remove> for NHibernate in Data.csproj (~line 46)
///    - Uncomment implementation in AddNHibernate method below
///    - Comment line 41, uncomment line 52
///    - Run: dotnet restore && dotnet run --project src/Api
///
/// 3. LINQ2DB (Requires package installation):
///    - Uncomment linq2db packages in Data.csproj and Infrastructure.csproj
///    - Uncomment using LinqToDB.AspNet at top of this file
///    - Remove <Compile Remove> for Linq2Db in Data.csproj (~line 51)
///    - Uncomment implementation in AddLinq2Db method below
///    - Comment line 41, uncomment line 57
///    - Run: dotnet restore && dotnet run --project src/Api
///
/// ==================================================================================
/// ALL ORMs HAVE COMPLETE IMPLEMENTATIONS FOR PRODUCT AND ORDER!
/// See: src/Data/Repository/README.md for detailed instructions
/// ==================================================================================
///
/// NO CONFIGURATION IN appsettings.json IS NEEDED!
/// This is intentional to keep configuration simple and avoid errors.
/// </summary>
public static class DatabaseExtension
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IOptions<AppSettings> appSettings)
    {
        var dbSettings = appSettings.Value.Infrastructure.Database;
        var connectionString = dbSettings.ConnectionString
            ?? throw new InvalidOperationException("Database connection string is required");

        // ==============================================================================
        // ORM CONFIGURATION - Multiple ORMs enabled simultaneously!
        // ==============================================================================
        // Entity Framework Core, Dapper, and ADO.NET are all enabled by default.
        // Each ORM has its own specific interface to avoid conflicts:
        // - IRepository<Product>          → Entity Framework Core (default, InMemory for tests)
        // - IProductDapperRepository      → Dapper (high performance, SQL Server)
        // - IProductAdoRepository         → ADO.NET (maximum control, SQL Server)
        //
        // By default, controllers use IRepository<T> which resolves to EF Core (InMemory).
        // You can inject specific interfaces when you need Dapper or ADO.NET features.
        // See docs/ORM-GUIDE.md for detailed usage examples.
        // ==============================================================================

        // PRIMARY: Entity Framework Core (Change Tracking, Migrations, CRUD)
        services.AddEntityFramework(connectionString, dbSettings);

        // ENABLED: Dapper (High Performance Queries, Complex Reports)
        services.AddDapper(connectionString);

        // ENABLED: ADO.NET (Maximum Control, Bulk Operations, Stored Procedures)
        services.AddAdo(connectionString);

        // OPTIONAL: NHibernate (Enterprise Features) - Requires package installation
        // Uncomment to enable NHibernate alongside other ORMs
        // See docs/ORM-GUIDE.md - "NHibernate Configuration" section
        // services.AddNHibernate(connectionString, dbSettings);

        // OPTIONAL: Linq2Db (LINQ + Performance) - Requires package installation
        // Uncomment to enable Linq2Db alongside other ORMs
        // See docs/ORM-GUIDE.md - "Linq2Db Configuration" section
        // services.AddLinq2Db(connectionString, dbSettings);

        return services;
    }

    private static IServiceCollection AddEntityFramework(
        this IServiceCollection services,
        string connectionString,
        DatabaseSettings settings)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            // Configure database provider based on DatabaseType
            // For production, install and uncomment the appropriate provider:
            // - Microsoft.EntityFrameworkCore.SqlServer for SQL Server
            // - Oracle.EntityFrameworkCore for Oracle
            // - Npgsql.EntityFrameworkCore.PostgreSQL for PostgreSQL
            // - Pomelo.EntityFrameworkCore.MySql for MySQL

            switch (settings.DatabaseType.ToLower())
            {
                case "memory":
                case "inmemory":
                    options.UseInMemoryDatabase("GoodHamburgerDb");
                    break;

                case "sqlserver":
                    options.UseSqlServer(connectionString, sqlOptions =>
                    {
                        sqlOptions.CommandTimeout(settings.CommandTimeoutSeconds);
                        sqlOptions.EnableRetryOnFailure(3);
                    });
                    break;

                case "oracle":
                    options.UseOracle(connectionString, oracleOptions =>
                    {
                        oracleOptions.CommandTimeout(settings.CommandTimeoutSeconds);
                        // Oracle doesn't support EnableRetryOnFailure like SQL Server/PostgreSQL
                    });
                    break;

                case "postgresql":
                    options.UseNpgsql(connectionString, npgsqlOptions =>
                    {
                        npgsqlOptions.CommandTimeout(settings.CommandTimeoutSeconds);
                        npgsqlOptions.EnableRetryOnFailure(3);
                    });
                    break;

                case "mysql":
                    var serverVersion = ServerVersion.AutoDetect(connectionString);
                    options.UseMySql(connectionString, serverVersion, mySqlOptions =>
                    {
                        mySqlOptions.CommandTimeout(settings.CommandTimeoutSeconds);
                        mySqlOptions.EnableRetryOnFailure(3);
                    });
                    break;

                default:
                    // Default to in-memory for testing/development
                    options.UseInMemoryDatabase("GoodHamburgerDb");
                    break;
            }

            // Common EF Core configurations
            if (settings.EnableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }

            options.EnableDetailedErrors();
        });

        // Register DbContext as generic DbContext for Repository<T> to resolve
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    private static IServiceCollection AddDapper(
        this IServiceCollection services,
        string connectionString)
    {
        // Register DbConnectionFactory for dependency injection
        // BEST PRACTICE: Instead of creating connections inside repositories,
        // we inject a factory that creates connections. This provides:
        // - Better testability (can mock IDbConnectionFactory)
        // - Proper dependency injection
        // - Centralized connection configuration
        // - Easier to switch database providers
        services.AddScoped<IDbConnectionFactory>(provider =>
            new Infrastructure.Services.SqlConnectionFactory(connectionString));

        // Register Dapper repositories with SPECIFIC interfaces
        // This allows using Dapper alongside EF Core without conflicts
        // Use IProductDapperRepository when you need Dapper's high-performance queries
        services.AddScoped<IProductDapperRepository, Data.Repository.Dapper.ProductDapperRepository>();
        services.AddScoped<IOrderDapperRepository, Data.Repository.Dapper.OrderDapperRepository>();

        return services;
    }

    /// <summary>
    /// Configures ADO.NET as the data access layer
    /// Provides maximum control and performance with raw database operations
    /// </summary>
    private static IServiceCollection AddAdo(
        this IServiceCollection services,
        string connectionString)
    {
        // Register connection factory (if not already registered)
        services.AddSingleton<IDbConnectionFactory>(sp =>
            new SqlConnectionFactory(connectionString));

        // Register ADO.NET repositories with SPECIFIC interfaces
        // This allows using ADO.NET alongside EF Core without conflicts
        // Use IProductAdoRepository when you need maximum control and performance
        services.AddScoped<IProductAdoRepository, Data.Repository.Ado.ProductAdoRepository>();
        services.AddScoped<IOrderAdoRepository, Data.Repository.Ado.OrderAdoRepository>();

        return services;
    }
}
