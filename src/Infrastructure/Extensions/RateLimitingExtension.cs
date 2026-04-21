using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using GoodHamburger.Domain;

namespace GoodHamburger.Infrastructure.Extensions;

/// <summary>
/// Rate limiting configuration to protect against abuse and ensure fair usage
/// Supports Fixed Window, Sliding Window, Token Bucket, and Concurrency limiters
/// </summary>
public static class RateLimitingExtension
{
    public static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services, IOptions<AppSettings> appSettings)
    {
        var settings = appSettings.Value.Infrastructure.RateLimiting;

        if (!settings.Enabled)
        {
            Console.WriteLine("⚠️  Rate Limiting is disabled");
            return services;
        }

        services.AddRateLimiter(options =>
        {
            // Global rejection behavior
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
                    ? retryAfterValue.TotalSeconds
                    : settings.DefaultWindow.TotalSeconds;

                context.HttpContext.Response.Headers["Retry-After"] = retryAfter.ToString(CultureInfo.InvariantCulture);
                context.HttpContext.Response.Headers["X-RateLimit-Limit"] = GetLimitForPolicy(settings.DefaultPolicy, settings).ToString();
                context.HttpContext.Response.Headers["X-RateLimit-Remaining"] = "0";
                context.HttpContext.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.Add(TimeSpan.FromSeconds(retryAfter)).ToUnixTimeSeconds().ToString();

                var response = new
                {
                    error = "Rate limit exceeded",
                    message = $"Too many requests. Please retry after {retryAfter} seconds.",
                    retryAfter = (int)retryAfter,
                    policy = settings.DefaultPolicy
                };

                await context.HttpContext.Response.WriteAsync(
                    JsonSerializer.Serialize(response),
                    cancellationToken);
            };

            // Fixed Window Limiter (janela fixa)
            if (settings.Policies.FixedWindow.Enabled)
            {
                options.AddFixedWindowLimiter("fixed", opt =>
                {
                    opt.PermitLimit = settings.Policies.FixedWindow.PermitLimit;
                    opt.Window = settings.Policies.FixedWindow.Window;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = settings.Policies.FixedWindow.QueueLimit;
                });
                Console.WriteLine($"  ✅ Fixed Window Rate Limiter: {settings.Policies.FixedWindow.PermitLimit} requests per {settings.Policies.FixedWindow.Window.TotalSeconds}s");
            }

            // Sliding Window Limiter (janela deslizante - mais justo)
            if (settings.Policies.SlidingWindow.Enabled)
            {
                options.AddSlidingWindowLimiter("sliding", opt =>
                {
                    opt.PermitLimit = settings.Policies.SlidingWindow.PermitLimit;
                    opt.Window = settings.Policies.SlidingWindow.Window;
                    opt.SegmentsPerWindow = settings.Policies.SlidingWindow.SegmentsPerWindow;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = settings.Policies.SlidingWindow.QueueLimit;
                });
                Console.WriteLine($"  ✅ Sliding Window Rate Limiter: {settings.Policies.SlidingWindow.PermitLimit} requests per {settings.Policies.SlidingWindow.Window.TotalSeconds}s");
            }

            // Token Bucket Limiter (permite bursts controlados)
            if (settings.Policies.TokenBucket.Enabled)
            {
                options.AddTokenBucketLimiter("token", opt =>
                {
                    opt.TokenLimit = settings.Policies.TokenBucket.TokenLimit;
                    opt.ReplenishmentPeriod = settings.Policies.TokenBucket.ReplenishmentPeriod;
                    opt.TokensPerPeriod = settings.Policies.TokenBucket.TokensPerPeriod;
                    opt.AutoReplenishment = settings.Policies.TokenBucket.AutoReplenishment;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = settings.Policies.TokenBucket.QueueLimit;
                });
                Console.WriteLine($"  ✅ Token Bucket Rate Limiter: {settings.Policies.TokenBucket.TokenLimit} tokens, +{settings.Policies.TokenBucket.TokensPerPeriod} every {settings.Policies.TokenBucket.ReplenishmentPeriod.TotalSeconds}s");
            }

            // Concurrency Limiter (limita requests simultâneas)
            if (settings.Policies.Concurrency.Enabled)
            {
                options.AddConcurrencyLimiter("concurrent", opt =>
                {
                    opt.PermitLimit = settings.Policies.Concurrency.PermitLimit;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = settings.Policies.Concurrency.QueueLimit;
                });
                Console.WriteLine($"  ✅ Concurrency Limiter: {settings.Policies.Concurrency.PermitLimit} concurrent requests");
            }

            // Global policy (aplica-se a todos os endpoints por padrão)
            if (!string.IsNullOrEmpty(settings.DefaultPolicy))
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    // Whitelist - IPs que não sofrem rate limiting
                    var ipAddress = GetClientIpAddress(context);
                    if (settings.WhitelistedIps.Contains(ipAddress))
                    {
                        return RateLimitPartition.GetNoLimiter<string>("whitelist");
                    }

                    // Aplicar política padrão baseada no IP do cliente
                    return settings.DefaultPolicy switch
                    {
                        "fixed" => RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = settings.Policies.FixedWindow.PermitLimit,
                            Window = settings.Policies.FixedWindow.Window,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = settings.Policies.FixedWindow.QueueLimit
                        }),
                        "sliding" => RateLimitPartition.GetSlidingWindowLimiter(ipAddress, _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = settings.Policies.SlidingWindow.PermitLimit,
                            Window = settings.Policies.SlidingWindow.Window,
                            SegmentsPerWindow = settings.Policies.SlidingWindow.SegmentsPerWindow,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = settings.Policies.SlidingWindow.QueueLimit
                        }),
                        "token" => RateLimitPartition.GetTokenBucketLimiter(ipAddress, _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = settings.Policies.TokenBucket.TokenLimit,
                            ReplenishmentPeriod = settings.Policies.TokenBucket.ReplenishmentPeriod,
                            TokensPerPeriod = settings.Policies.TokenBucket.TokensPerPeriod,
                            AutoReplenishment = settings.Policies.TokenBucket.AutoReplenishment,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = settings.Policies.TokenBucket.QueueLimit
                        }),
                        "concurrent" => RateLimitPartition.GetConcurrencyLimiter(ipAddress, _ => new ConcurrencyLimiterOptions
                        {
                            PermitLimit = settings.Policies.Concurrency.PermitLimit,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = settings.Policies.Concurrency.QueueLimit
                        }),
                        _ => RateLimitPartition.GetNoLimiter<string>("none")
                    };
                });

                Console.WriteLine($"  🌐 Global Rate Limiter enabled with policy: {settings.DefaultPolicy}");
            }
        });

        Console.WriteLine("✅ Rate Limiting enabled");
        return services;
    }

    /// <summary>
    /// Middleware to add rate limit headers to all responses
    /// </summary>
    public static IApplicationBuilder UseRateLimitingHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            await next();

            // Add rate limit headers if not already present
            if (!context.Response.Headers.ContainsKey("X-RateLimit-Limit"))
            {
                // These headers will be set by OnRejected callback when limit is exceeded
                // For successful requests, we could add them here if needed
            }
        });
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Try to get IP from X-Forwarded-For header (if behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        // Try X-Real-IP header
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fallback to connection remote IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static int GetLimitForPolicy(string policy, RateLimitingSettings settings)
    {
        return policy switch
        {
            "fixed" => settings.Policies.FixedWindow.PermitLimit,
            "sliding" => settings.Policies.SlidingWindow.PermitLimit,
            "token" => settings.Policies.TokenBucket.TokenLimit,
            "concurrent" => settings.Policies.Concurrency.PermitLimit,
            _ => 100
        };
    }
}
