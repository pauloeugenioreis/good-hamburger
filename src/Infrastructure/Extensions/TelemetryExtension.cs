using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using GoodHamburger.Domain;

namespace GoodHamburger.Infrastructure.Extensions;

/// <summary>
/// Telemetry and observability configuration using OpenTelemetry
/// Supports multiple backends: OTLP (Jaeger, Grafana, Tempo), Prometheus, Datadog, Dynatrace, Application Insights
/// Note: Jaeger now uses OTLP protocol instead of deprecated native exporter
/// </summary>
public static class TelemetryExtension
{
    public static IServiceCollection AddTelemetry(
        this IServiceCollection services,
        IOptions<AppSettings> appSettings)
    {
        var settings = appSettings.Value;

        if (!appSettings.Value.Infrastructure.Telemetry.Enabled)
        {
            Console.WriteLine("⚠️  Telemetry is disabled");
            return services;
        }

        var serviceName = "GoodHamburger.Api";
        var serviceVersion = "1.0.0";

        // Resource attributes (identificação do serviço)
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: serviceName,
                serviceVersion: serviceVersion,
                serviceInstanceId: Environment.MachineName)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = settings.EnvironmentName,
                ["host.name"] = Environment.MachineName,
                ["telemetry.sdk.name"] = "opentelemetry",
                ["telemetry.sdk.language"] = "dotnet",
                ["telemetry.sdk.version"] = "1.7.0"
            });

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName, serviceVersion))
            .WithTracing(tracerProvider =>
            {
                // Instrumentação automática
                tracerProvider
                    .SetResourceBuilder(resourceBuilder)
                    .AddSource(serviceName)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequest = (activity, httpRequest) =>
                        {
                            activity.SetTag("http.request.method", httpRequest.Method);
                            activity.SetTag("http.request.path", httpRequest.Path);
                            activity.SetTag("http.request.query", httpRequest.QueryString.ToString());
                            activity.SetTag("http.request.user_agent", httpRequest.Headers.UserAgent.ToString());
                        };
                        options.EnrichWithHttpResponse = (activity, httpResponse) =>
                        {
                            activity.SetTag("http.response.status_code", httpResponse.StatusCode);
                        };
                        options.Filter = (httpContext) =>
                        {
                            // Não rastrear health checks para reduzir ruído
                            return !httpContext.Request.Path.StartsWithSegments("/health");
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) =>
                        {
                            activity.SetTag("http.client.method", httpRequestMessage.Method.ToString());
                            activity.SetTag("http.client.url", httpRequestMessage.RequestUri?.ToString());
                        };
                    });

                if (appSettings.Value.Infrastructure.Telemetry.EnableSqlInstrumentation)
                {
                    tracerProvider.AddSqlClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    });
                }

                tracerProvider
                    .AddEntityFrameworkCoreInstrumentation();

                // Sampling (reduz volume em produção)
                if (appSettings.Value.Infrastructure.Telemetry.SamplingRatio > 0 && appSettings.Value.Infrastructure.Telemetry.SamplingRatio < 1.0)
                {
                    tracerProvider.SetSampler(new TraceIdRatioBasedSampler(appSettings.Value.Infrastructure.Telemetry.SamplingRatio));
                }

                // Configurar exportadores baseado no provider
                ConfigureTraceExporters(tracerProvider, appSettings);
            })
            .WithMetrics(meterProvider =>
            {
                meterProvider
                    .SetResourceBuilder(resourceBuilder)
                    .AddMeter(serviceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                // Configurar exportadores de métricas
                ConfigureMetricExporters(meterProvider, settings.Infrastructure.Telemetry);
            });

        Console.WriteLine($"✅ Telemetry enabled: {string.Join(", ", settings.Infrastructure.Telemetry.Providers)}");
        return services;
    }

    private static void ConfigureTraceExporters(
        TracerProviderBuilder builder,
        IOptions<AppSettings> appSettings)
    {
        var settings = appSettings.Value;

        foreach (var provider in settings.Infrastructure.Telemetry.Providers)
        {
            switch (provider.ToLowerInvariant())
            {
                case "console":
                    builder.AddConsoleExporter();
                    Console.WriteLine("  📊 Console exporter enabled");
                    break;

                case "jaeger":
                    // Jaeger now uses OTLP protocol (native exporter is deprecated)
                    // Configure OTLP exporter to send to Jaeger's OTLP endpoints
                    var jaegerOtlpEndpoint = settings.Infrastructure.Telemetry.Jaeger.UseGrpc
                        ? $"http://{settings.Infrastructure.Telemetry.Jaeger.Host}:4317"
                        : $"http://{settings.Infrastructure.Telemetry.Jaeger.Host}:4318";

                    builder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(jaegerOtlpEndpoint);
                        options.Protocol = settings.Infrastructure.Telemetry.Jaeger.UseGrpc
                            ? OtlpExportProtocol.Grpc
                            : OtlpExportProtocol.HttpProtobuf;
                    });
                    Console.WriteLine($"  📊 Jaeger (via OTLP) exporter enabled: {jaegerOtlpEndpoint}");
                    break;

                case "zipkin":
                    builder.AddZipkinExporter(options =>
                    {
                        options.Endpoint = new Uri(settings.Infrastructure.Telemetry.Zipkin.Endpoint);
                    });
                    Console.WriteLine($"  📊 Zipkin exporter enabled: {settings.Infrastructure.Telemetry.Zipkin.Endpoint}");
                    break;

                case "otlp":
                case "grafana":
                    builder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(settings.Infrastructure.Telemetry.Otlp.Endpoint);
                        options.Protocol = settings.Infrastructure.Telemetry.Otlp.Protocol == "grpc"
                            ? OtlpExportProtocol.Grpc
                            : OtlpExportProtocol.HttpProtobuf;

                        if (!string.IsNullOrEmpty(settings.Infrastructure.Telemetry.Otlp.Headers))
                        {
                            options.Headers = settings.Infrastructure.Telemetry.Otlp.Headers;
                        }
                    });
                    Console.WriteLine($"  📊 OTLP/Grafana exporter enabled: {settings.Infrastructure.Telemetry.Otlp.Endpoint}");
                    break;

                case "applicationinsights":
                case "azure":
                    // TODO: Application Insights requires Azure.Monitor.OpenTelemetry.AspNetCore package
                    // builder.AddAzureMonitorTraceExporter(options =>
                    // {
                    //     options.ConnectionString = settings.Infrastructure.Telemetry.ApplicationInsights.ConnectionString;
                    // });
                    Console.WriteLine($"  ⚠️  Application Insights not yet configured (requires Azure.Monitor.OpenTelemetry.AspNetCore)");
                    break;

                case "datadog":
                    // Datadog usa OTLP protocol
                    builder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(settings.Infrastructure.Telemetry.Datadog.Endpoint);
                        options.Protocol = OtlpExportProtocol.Grpc;
                        options.Headers = $"DD-API-KEY={settings.Infrastructure.Telemetry.Datadog.ApiKey}";
                    });
                    Console.WriteLine($"  📊 Datadog exporter enabled: {settings.Infrastructure.Telemetry.Datadog.Endpoint}");
                    break;

                case "dynatrace":
                    // Dynatrace usa OTLP protocol
                    builder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(settings.Infrastructure.Telemetry.Dynatrace.Endpoint);
                        options.Protocol = OtlpExportProtocol.HttpProtobuf;
                        options.Headers = $"Authorization=Api-Token {settings.Infrastructure.Telemetry.Dynatrace.ApiToken}";
                    });
                    Console.WriteLine($"  📊 Dynatrace exporter enabled: {settings.Infrastructure.Telemetry.Dynatrace.Endpoint}");
                    break;

                default:
                    Console.WriteLine($"  ⚠️  Unknown telemetry provider: {provider}");
                    break;
            }
        }

        // Console exporter sempre ativo em Development (para debug)
        if (settings.IsDevelopment() && !settings.Infrastructure.Telemetry.Providers.Contains("console", StringComparer.OrdinalIgnoreCase))
        {
            builder.AddConsoleExporter();
            Console.WriteLine("  📊 Console exporter enabled (Development)");
        }
    }

    private static void ConfigureMetricExporters(
        MeterProviderBuilder builder,
        TelemetrySettings settings
        )
    {
        foreach (var provider in settings.Providers)
        {
            switch (provider.ToLowerInvariant())
            {
                case "console":
                    builder.AddConsoleExporter();
                    break;

                case "prometheus":
                    // Prometheus usa scraping, não push
                    // Endpoint será exposto em /metrics
                    builder.AddPrometheusExporter();
                    Console.WriteLine("  📈 Prometheus exporter enabled (endpoint: /metrics)");
                    break;

                case "otlp":
                case "grafana":
                    builder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(settings.Otlp.Endpoint);
                        options.Protocol = settings.Otlp.Protocol == "grpc"
                            ? OtlpExportProtocol.Grpc
                            : OtlpExportProtocol.HttpProtobuf;

                        if (!string.IsNullOrEmpty(settings.Otlp.Headers))
                        {
                            options.Headers = settings.Otlp.Headers;
                        }
                    });
                    break;

                case "applicationinsights":
                case "azure":
                    // TODO: Application Insights requires Azure.Monitor.OpenTelemetry.AspNetCore package
                    // builder.AddAzureMonitorMetricExporter(options =>
                    // {
                    //     options.ConnectionString = settings.ApplicationInsights.ConnectionString;
                    // });
                    break;

                case "datadog":
                    builder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(settings.Datadog.Endpoint);
                        options.Protocol = OtlpExportProtocol.Grpc;
                        options.Headers = $"DD-API-KEY={settings.Datadog.ApiKey}";
                    });
                    break;

                case "dynatrace":
                    builder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(settings.Dynatrace.Endpoint);
                        options.Protocol = OtlpExportProtocol.HttpProtobuf;
                        options.Headers = $"Authorization=Api-Token {settings.Dynatrace.ApiToken}";
                    });
                    break;
            }
        }
    }
}
