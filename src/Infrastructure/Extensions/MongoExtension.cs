using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using GoodHamburger.Data.Repository.Mongo;
using GoodHamburger.Domain;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Infrastructure.Extensions;

/// <summary>
/// Extension methods for MongoDB configuration
/// Provides MongoDB client and database setup with proper error handling
/// </summary>
public static class MongoExtension
{
    /// <summary>
    /// Adds MongoDB services to the DI container
    /// Configures MongoClient and default database
    /// </summary>
    public static IServiceCollection AddMongo<TProgram>(this IServiceCollection services)
    {
        services.AddSingleton<IMongoClient>(sp => CreateMongoClient<TProgram>(
            sp.GetRequiredService<IOptions<AppSettings>>(),
            sp.GetRequiredService<ILogger<TProgram>>()));

        // Register default IMongoDatabase resolved from connection string
        services.AddSingleton(sp =>
        {
            var appSettings = sp.GetRequiredService<IOptions<AppSettings>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            var connectionString = appSettings.Infrastructure?.MongoDB?.ConnectionString;

            var mongoUrl = new MongoUrl(connectionString ?? string.Empty);

            var databaseName = string.IsNullOrWhiteSpace(mongoUrl.DatabaseName)
                ? "GoodHamburger"
                : mongoUrl.DatabaseName;

            return client.GetDatabase(databaseName);
        });

        // Register MongoDB repositories and services via Scrutor
        // Scans for all IMongoRepository<T> implementations and registers them
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(MongoRepository<>))
            .AddClasses(classes => classes.AssignableTo(typeof(IMongoRepository<>)))
            .AsMatchingInterface()
            .WithScopedLifetime()
        );

        // Register generic fallback for IMongoRepository<T>
        services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));

        return services;
    }

    private static IMongoClient CreateMongoClient<TProgram>(
        IOptions<AppSettings> settings,
        ILogger<TProgram> logger)
    {
        var appSettings = settings.Value;
        var isProduction = appSettings.Infrastructure?.Environment == "Production";

        try
        {
            var connectionString = appSettings.Infrastructure?.MongoDB?.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                if (isProduction)
                {
                    throw new InvalidOperationException("MongoDB connection string cannot be empty in production.");
                }

                logger.LogWarning("MongoDB connection string is empty; using development fallback client.");
                return new MongoClient();
            }

            var mongoUrl = new MongoUrl(connectionString);
            var commandTimeoutSeconds = appSettings.Infrastructure?.Database?.CommandTimeoutSeconds ?? 300;

            var mongoSettings = MongoClientSettings.FromUrl(mongoUrl);
            mongoSettings.RetryWrites = true;
            mongoSettings.SocketTimeout = TimeSpan.FromSeconds(commandTimeoutSeconds);
            mongoSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(Math.Min(30, Math.Max(2, commandTimeoutSeconds / 10)));
            mongoSettings.ConnectTimeout = TimeSpan.FromSeconds(Math.Min(10, Math.Max(2, commandTimeoutSeconds / 30)));

            logger.LogInformation("Creating MongoClient for servers: {Servers}", string.Join(',', mongoSettings.Servers));

            return new MongoClient(mongoSettings);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating MongoClient.");

            if (isProduction)
            {
                throw;
            }

            logger.LogWarning("Returning fallback MongoClient for development.");
            return new MongoClient();
        }
    }
}
