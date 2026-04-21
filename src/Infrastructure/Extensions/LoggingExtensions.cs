using System;
using Google.Cloud.Logging.Console;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using GoodHamburger.Domain;

namespace GoodHamburger.Infrastructure.Extensions;

/// <summary>
/// Extension methods for structured logging configuration
/// Supports console, debug, and Google Cloud Logging
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Adds customized logging with structured output
    /// Uses JSON console formatter in dev, Google Cloud Console in production
    /// </summary>
    public static WebApplicationBuilder AddCustomLogging(this WebApplicationBuilder builder)
    {
        var tempProvider = builder.Services.BuildServiceProvider();
        var appSettings = tempProvider.GetRequiredService<IOptions<AppSettings>>().Value;

        // Configure structured and detailed logging
        builder.Logging.ClearProviders();

        if (!appSettings.IsProduction())
        {
            // Development: use JSON console formatter
            builder.Logging.AddConsole(options => options.FormatterName = ConsoleFormatterNames.Json);
        }
        else if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT")))
        {
            // Production with GCP: use Google Cloud Logging
            builder.Logging.AddGoogleCloudConsole();
        }
        else
        {
            // Production without GCP: use simple console
            builder.Logging.AddConsole();
        }

        builder.Logging.AddDebug();
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        // Configure JSON console formatter
        builder.Services.Configure<JsonConsoleFormatterOptions>(options =>
        {
            options.IncludeScopes = true;
            options.TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss.fffK";
            options.UseUtcTimestamp = true;
        });

        return builder;
    }
}
