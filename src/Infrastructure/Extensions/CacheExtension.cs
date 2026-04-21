using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using GoodHamburger.Domain;

namespace GoodHamburger.Infrastructure.Extensions;

/// <summary>
/// Cache configuration supporting multiple providers
/// </summary>
public static class CacheExtension
{
    public static IServiceCollection AddCacheConfiguration(this IServiceCollection services, IOptions<AppSettings> appSettings)
    {
        var cacheSettings = appSettings.Value.Infrastructure.Cache;

        if (!cacheSettings.Enabled)
        {
            // Use memory cache as fallback
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            return services;
        }

        switch (cacheSettings.Provider.ToLower())
        {
            case "redis":
                var redisConnectionString = cacheSettings.Redis?.ConnectionString;
                if (string.IsNullOrEmpty(redisConnectionString))
                {
                    throw new InvalidOperationException("Redis connection string is required when Redis provider is selected");
                }

                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = "GoodHamburger_";
                });
                break;

            case "sqlserver":
                // SQL Server distributed cache requires:
                // Install-Package Microsoft.Extensions.Caching.SqlServer
                // dotnet sql-cache create "connection-string" dbo Cache
                services.AddMemoryCache();
                services.AddDistributedMemoryCache();
                break;
            default:
                services.AddMemoryCache();
                services.AddDistributedMemoryCache();
                break;
        }

        return services;
    }
}
