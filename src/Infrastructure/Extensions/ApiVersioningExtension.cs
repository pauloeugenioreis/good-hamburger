using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace GoodHamburger.Infrastructure.Extensions;

/// <summary>
/// Extension methods for API versioning configuration
/// Enables versioning through URL, header, or query string
/// </summary>
public static class ApiVersioningExtension
{
    /// <summary>
    /// Adds API versioning services to the DI container
    /// Configures versioning with multiple readers (URL, header, query string)
    /// </summary>
    public static IServiceCollection AddCustomizedApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;

            // Configure how API version will be read
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

        return services;
    }
}
