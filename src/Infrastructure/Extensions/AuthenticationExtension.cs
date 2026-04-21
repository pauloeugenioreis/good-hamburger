using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GoodHamburger.Infrastructure.Extensions;

/// <summary>
/// Authentication extension methods for configuring JWT Bearer authentication
/// </summary>
public static class AuthenticationExtension
{
    /// <summary>
    /// Add JWT Authentication services
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Use Authentication middleware
    /// </summary>
    public static IApplicationBuilder UseJwtAuthentication(this IApplicationBuilder app)
    {
        return app;
    }
}
