using System.ComponentModel.DataAnnotations;
using GoodHamburger.Domain.Validators;

namespace GoodHamburger.Domain;

/// <summary>
/// Application settings configuration
/// </summary>
public class AppSettings
{
    [Required(ErrorMessage = "EnvironmentName is required")]
    [AllowedValues("Development", "Testing", "Staging", "Production",
        ErrorMessage = "EnvironmentName must be Development, Testing, Staging, or Production")]
    public string EnvironmentName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Infrastructure section is required")]
    public InfrastructureSettings Infrastructure { get; set; } = new();

    [Required(ErrorMessage = "Authentication section is required")]
    public AuthenticationSettings Authentication { get; set; } = new();

    public bool IsDevelopment() => EnvironmentName == "Development";
    public bool IsTesting() => EnvironmentName == "Testing";
    public bool IsStaging() => EnvironmentName == "Staging";
    public bool IsProduction() => EnvironmentName == "Production";
}

public class InfrastructureSettings
{
    public string Environment { get; set; } = "Development";
    public CacheSettings Cache { get; set; } = new();
    public DatabaseSettings Database { get; set; } = new();
    public MongoDbSettings MongoDB { get; set; } = new();
    public RabbitMqSettings RabbitMQ { get; set; } = new();
    public StorageSettings Storage { get; set; } = new();
    public QuartzSettings Quartz { get; set; } = new();
    public TelemetrySettings Telemetry { get; set; } = new();
    public RateLimitingSettings RateLimiting { get; set; } = new();
    public EventSourcingSettings EventSourcing { get; set; } = new();
    public OrderSettings Order { get; set; } = new();
}

public class OrderSettings
{
    [Range(0, double.MaxValue, ErrorMessage = "FreeShippingThreshold must be non-negative")]
    public decimal FreeShippingThreshold { get; set; } = 100m;
}

public class CacheSettings
{
    public bool Enabled { get; set; } = true;

    [Required(ErrorMessage = "Cache Provider is required")]
    [AllowedValues("Memory", "Redis", "SqlServer",
        ErrorMessage = "Provider must be Memory, Redis, or SqlServer")]
    public string Provider { get; set; } = "Memory"; // Memory, Redis, SqlServer

    public RedisSettings? Redis { get; set; }

    [Range(1, 1440, ErrorMessage = "DefaultExpirationMinutes must be between 1 and 1440 (24 hours)")]
    public int DefaultExpirationMinutes { get; set; } = 60;
}

public class RedisSettings
{
    [RequiredIf(nameof(CacheSettings.Provider), "Redis",
        ErrorMessage = "Redis ConnectionString is required when Provider is Redis")]
    [RedisConnectionString(ErrorMessage = "Invalid Redis connection string format")]
    public string ConnectionString { get; set; } = string.Empty;
}

public class DatabaseSettings
{
    [Required(ErrorMessage = "DatabaseType is required")]
    [AllowedValues("InMemory", "SqlServer", "Oracle", "PostgreSQL", "MySQL",
        ErrorMessage = "DatabaseType must be InMemory, SqlServer, Oracle, PostgreSQL, or MySQL")]
    public string DatabaseType { get; set; } = "InMemory"; // InMemory, SqlServer, Oracle, PostgreSQL, MySQL

    [Required(ErrorMessage = "ConnectionString is required")]
    [MinLength(10, ErrorMessage = "ConnectionString must be at least 10 characters")]
    public string ConnectionString { get; set; } = string.Empty;

    public string ReadOnlyConnectionString { get; set; } = string.Empty;

    [Range(1, 300, ErrorMessage = "CommandTimeoutSeconds must be between 1 and 300 seconds")]
    public int CommandTimeoutSeconds { get; set; } = 30;

    public bool EnableSensitiveDataLogging { get; set; }
    public bool EnableDetailedErrors { get; set; }
}

public class MongoDbSettings
{
    public string? ConnectionString { get; set; }
}

public class RabbitMqSettings
{
    public string? ConnectionString { get; set; }
}

public class QuartzSettings
{
    public int MaxConcurrency { get; set; } = 10;
}

public class StorageSettings
{
    [AllowedValues("Google", "Azure", "Aws",
        ErrorMessage = "Storage Provider must be Google, Azure, or Aws")]
    public string Provider { get; set; } = "Google";

    public GoogleStorageSettings Google { get; set; } = new();
    public AzureStorageSettings Azure { get; set; } = new();
    public AwsStorageSettings Aws { get; set; } = new();

    public string DefaultBucket { get; set; } = string.Empty;
}

public class GoogleStorageSettings
{
    /// <summary>
    /// Google Cloud service account JSON payload
    /// </summary>
    public string? ServiceAccount { get; set; }

    /// <summary>
    /// Optional Google Cloud project identifier
    /// </summary>
    public string? ProjectId { get; set; }
}

public class AzureStorageSettings
{
    /// <summary>
    /// Azure Storage connection string (preferred for development)
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Service URI when using SAS or Managed Identity authentication
    /// </summary>
    public string? BlobServiceUri { get; set; }

    /// <summary>
    /// Optional Managed Identity client id when authenticating via DefaultAzureCredential
    /// </summary>
    public string? ManagedIdentityClientId { get; set; }
}

public class AwsStorageSettings
{
    /// <summary>
    /// AWS access key id. Leave empty to rely on default credential chain
    /// </summary>
    public string? AccessKeyId { get; set; }

    /// <summary>
    /// AWS secret access key. Leave empty to rely on default credential chain
    /// </summary>
    public string? SecretAccessKey { get; set; }

    /// <summary>
    /// AWS region (e.g. us-east-1)
    /// </summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>
    /// Optional profile name when using shared credentials file
    /// </summary>
    public string? Profile { get; set; }

    /// <summary>
    /// Optional custom endpoint (e.g. http://localhost:4566 for LocalStack)
    /// </summary>
    public string? ServiceUrl { get; set; }
}

public class AuthenticationSettings
{
    /// <summary>
    /// Enable/disable authentication globally
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// JWT Bearer token settings
    /// </summary>
    public JwtSettings Jwt { get; set; } = new();

    /// <summary>
    /// OAuth2 external providers settings
    /// </summary>
    public OAuth2Settings OAuth2 { get; set; } = new();

    /// <summary>
    /// Password policy settings
    /// </summary>
    public PasswordPolicySettings PasswordPolicy { get; set; } = new();
}

public class JwtSettings
{
    [Required(ErrorMessage = "JWT Secret is required")]
    [MinLength(32, ErrorMessage = "JWT Secret must be at least 32 characters for security")]
    public string Secret { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT Issuer is required")]
    [Url(ErrorMessage = "Issuer must be a valid URL")]
    public string Issuer { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT Audience is required")]
    [Url(ErrorMessage = "Audience must be a valid URL")]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 1440, ErrorMessage = "ExpirationMinutes must be between 1 and 1440 (24 hours)")]
    public int ExpirationMinutes { get; set; } = 60;

    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
    public bool ValidateIssuerSigningKey { get; set; } = true;
}

public class OAuth2Settings
{
    public bool Enabled { get; set; }
    public GoogleOAuthSettings Google { get; set; } = new();
    public MicrosoftOAuthSettings Microsoft { get; set; } = new();
    public GitHubOAuthSettings GitHub { get; set; } = new();
}

public class GoogleOAuthSettings
{
    public bool Enabled { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class MicrosoftOAuthSettings
{
    public bool Enabled { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string TenantId { get; set; } = "common";
}

public class GitHubOAuthSettings
{
    public bool Enabled { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class PasswordPolicySettings
{
    [Range(6, 128, ErrorMessage = "MinimumLength must be between 6 and 128 characters")]
    public int MinimumLength { get; set; } = 8;

    public bool RequireDigit { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireNonAlphanumeric { get; set; } = true;

    [Range(1, 100, ErrorMessage = "MaxFailedAccessAttempts must be between 1 and 100")]
    public int MaxFailedAccessAttempts { get; set; } = 5;

    [Range(1, 1440, ErrorMessage = "LockoutMinutes must be between 1 and 1440 (24 hours)")]
    public int LockoutMinutes { get; set; } = 15;
}

public class TelemetrySettings
{
    public bool Enabled { get; set; }
    public string[] Providers { get; set; } = Array.Empty<string>(); // console, jaeger (via OTLP), otlp, grafana, prometheus, applicationinsights, datadog, dynatrace

    [Range(0.0, 1.0, ErrorMessage = "SamplingRatio must be between 0.0 and 1.0")]
    public double SamplingRatio { get; set; } = 1.0; // 0.0 to 1.0 (1.0 = 100%)

    public bool EnableSqlInstrumentation { get; set; } = true;
    public bool EnableHttpInstrumentation { get; set; } = true;

    public JaegerSettings Jaeger { get; set; } = new();
    public ZipkinSettings Zipkin { get; set; } = new();
    public OtlpSettings Otlp { get; set; } = new();
    public ApplicationInsightsSettings ApplicationInsights { get; set; } = new();
    public DatadogSettings Datadog { get; set; } = new();
    public DynatraceSettings Dynatrace { get; set; } = new();
}

public class JaegerSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 4317; // OTLP gRPC port (was 6831 for deprecated Jaeger native)
    public bool UseGrpc { get; set; } = true; // true = gRPC (4317), false = HTTP (4318)

    public int MaxPayloadSizeInBytes { get; set; } = 4096; // Kept for backward compatibility
}

public class ZipkinSettings
{
    public string Endpoint { get; set; } = "http://localhost:9411/api/v2/spans";
}

public class OtlpSettings
{
    public string Endpoint { get; set; } = "http://localhost:4317"; // gRPC endpoint
    public string Protocol { get; set; } = "grpc"; // grpc or http
    public string? Headers { get; set; } // Format: "key1=value1,key2=value2"
}

public class ApplicationInsightsSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public bool EnableAdaptiveSampling { get; set; } = true;
    public bool EnableLiveMetrics { get; set; } = true;
}

public class DatadogSettings
{
    public string Endpoint { get; set; } = "http://localhost:4317"; // Datadog Agent OTLP endpoint
    public string ApiKey { get; set; } = string.Empty;
    public string Site { get; set; } = "datadoghq.com"; // or datadoghq.eu
    public string Environment { get; set; } = "development";
}

public class DynatraceSettings
{
    public string Endpoint { get; set; } = string.Empty; // https://{your-environment-id}.live.dynatrace.com/api/v2/otlp
    public string ApiToken { get; set; } = string.Empty;
    public string Environment { get; set; } = "development";
}

public class RateLimitingSettings
{
    public bool Enabled { get; set; }
    public string DefaultPolicy { get; set; } = "fixed"; // fixed, sliding, token, concurrent, none
    public TimeSpan DefaultWindow { get; set; } = TimeSpan.FromMinutes(1);
    public string[] WhitelistedIps { get; set; } = Array.Empty<string>(); // IPs que não sofrem rate limiting
    public RateLimitingPolicies Policies { get; set; } = new();
}

public class RateLimitingPolicies
{
    public FixedWindowPolicy FixedWindow { get; set; } = new();
    public SlidingWindowPolicy SlidingWindow { get; set; } = new();
    public TokenBucketPolicy TokenBucket { get; set; } = new();
    public ConcurrencyPolicy Concurrency { get; set; } = new();
}

public class FixedWindowPolicy
{
    public bool Enabled { get; set; } = true;
    public int PermitLimit { get; set; } = 100; // Número de requests permitidas
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1); // Janela de tempo
    public int QueueLimit { get; set; } = 10; // Requests enfileiradas quando limite atingido
}

public class SlidingWindowPolicy
{
    public bool Enabled { get; set; } = true;
    public int PermitLimit { get; set; } = 100;
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);
    public int SegmentsPerWindow { get; set; } = 6; // Divide a janela em 6 segmentos de 10s cada
    public int QueueLimit { get; set; } = 10;
}

public class TokenBucketPolicy
{
    public bool Enabled { get; set; } = true;
    public int TokenLimit { get; set; } = 100; // Capacidade máxima do balde
    public TimeSpan ReplenishmentPeriod { get; set; } = TimeSpan.FromSeconds(10); // A cada quanto tempo reabastece
    public int TokensPerPeriod { get; set; } = 10; // Quantos tokens adiciona por período
    public bool AutoReplenishment { get; set; } = true;
    public int QueueLimit { get; set; } = 10;
}

public class ConcurrencyPolicy
{
    public bool Enabled { get; set; } = true;
    public int PermitLimit { get; set; } = 50; // Requests simultâneas permitidas
    public int QueueLimit { get; set; } = 100; // Requests enfileiradas
}

/// <summary>
/// Event Sourcing configuration settings
/// </summary>
public class EventSourcingSettings
{
    /// <summary>
    /// Enable/disable Event Sourcing globally
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Event Sourcing mode: Traditional, Hybrid, or EventStore
    /// </summary>
    public EventSourcingMode Mode { get; set; } = EventSourcingMode.Hybrid;

    /// <summary>
    /// Event Store provider: Marten, Custom, EventStoreDB
    /// </summary>
    public string Provider { get; set; } = "Marten";

    /// <summary>
    /// PostgreSQL connection string for Event Store (required for Marten)
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Entities to audit via Event Sourcing (empty = all entities)
    /// </summary>
    public List<string> AuditEntities { get; set; } = new();

    /// <summary>
    /// Enable snapshot storage for performance optimization
    /// </summary>
    public bool StoreSnapshots { get; set; } = true;

    /// <summary>
    /// Create snapshot every N events (improves read performance)
    /// </summary>
    public int SnapshotInterval { get; set; } = 10;

    /// <summary>
    /// Enable audit API endpoints
    /// </summary>
    public bool EnableAuditApi { get; set; } = true;

    /// <summary>
    /// Store additional metadata (IP address, user agent, etc.)
    /// </summary>
    public bool StoreMetadata { get; set; } = true;
}

/// <summary>
/// Event Sourcing operational mode
/// </summary>
public enum EventSourcingMode
{
    /// <summary>
    /// Traditional CRUD - No event sourcing (EF Core only)
    /// </summary>
    Traditional = 0,

    /// <summary>
    /// Hybrid mode - EF Core as source of truth + Events for audit trail
    /// Best for gradual adoption and simple audit requirements
    /// </summary>
    Hybrid = 1,

    /// <summary>
    /// Event Store as source of truth - All state derived from events
    /// Best for full event sourcing, time travel, and complex audit requirements
    /// </summary>
    EventStore = 2
}

