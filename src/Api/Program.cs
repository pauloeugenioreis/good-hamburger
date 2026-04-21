using Microsoft.EntityFrameworkCore;
using GoodHamburger.Data.Context;
using GoodHamburger.Data.Seeders;
using GoodHamburger.Infrastructure.Extensions;
using GoodHamburger.Infrastructure.Filters;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});
builder.Services.AddProblemDetails();


// Add Swagger with JWT authentication (skip in Testing environment to avoid version conflicts)
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddSwaggerWithAuth();
}

// Add infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);

// ============================================================
// OPTIONAL FEATURES — Uncomment to enable
// See docs/FEATURES.md for configuration details
// ============================================================

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var eventStore = scope.ServiceProvider.GetRequiredService<GoodHamburger.Domain.Interfaces.IEventStore>();

    await context.Database.MigrateAsync();

    var seeder = new GoodHamburger.Data.Seeders.DbSeeder(context, eventStore);
    await seeder.SeedAsync();
}

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithAuth();
}
else if (app.Environment.IsEnvironment("Testing"))
{
    // Skip Swagger in Testing environment
}

app.UseGlobalExceptionHandler();

app.UseHttpsRedirection();

// Security Headers
app.UseHsts();
app.UseXContentTypeOptions();
app.UseReferrerPolicy(opts => opts.NoReferrer());
app.UseXXssProtection(opts => opts.EnabledWithBlockMode());
app.UseXfo(opts => opts.Deny());


// Use infrastructure middleware
app.UseInfrastructureMiddleware();

// Prometheus metrics endpoint (if telemetry is enabled)
var telemetryEnabled = app.Configuration.GetValue<bool>("AppSettings:Infrastructure:Telemetry:Enabled");
var telemetryProviders = app.Configuration.GetSection("AppSettings:Infrastructure:Telemetry:Providers").Get<string[]>() ?? Array.Empty<string>();
if (telemetryEnabled && telemetryProviders.Contains("prometheus", StringComparer.OrdinalIgnoreCase))
{
    app.MapPrometheusScrapingEndpoint();
}

app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

await app.RunAsync().ConfigureAwait(false);

// Make Program class accessible for testing
public partial class Program
{
    protected Program()
    {
    }
}
