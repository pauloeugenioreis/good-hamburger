using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GoodHamburger.Domain;

namespace GoodHamburger.Infrastructure.Extensions;

/// <summary>
/// Application settings configuration and validation
/// </summary>
public static class AppSettingsExtension
{
    public static IServiceCollection AddAppSettingsConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind and validate settings
        services.AddOptions<AppSettings>()
            .Bind(configuration.GetSection("AppSettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register settings as singleton for easy injection
        services.AddSingleton(sp =>
            sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AppSettings>>().Value);

        return services;
    }
}
