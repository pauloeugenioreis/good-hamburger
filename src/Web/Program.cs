using GoodHamburger.Web.Components;
using GoodHamburger.Web.Services;
using GoodHamburger.Web.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection(ApiOptions.SectionName));

builder.Services.AddScoped<LoadingState>();
builder.Services.AddTransient<HttpLoadingHandler>();

Action<IServiceProvider, HttpClient> configureClient = (serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ApiOptions>>().Value;
    var baseUrl = string.IsNullOrWhiteSpace(options.BaseUrl)
        ? "https://localhost:3060/"
        : options.BaseUrl;

    client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
    client.Timeout = TimeSpan.FromSeconds(options.RequestTimeoutSeconds > 0 ? options.RequestTimeoutSeconds : 30);
};

builder.Services.AddHttpClient<IProductClient, ProductClient>(configureClient)
    .AddHttpMessageHandler<HttpLoadingHandler>();
builder.Services.AddHttpClient<IOrderClient, OrderClient>(configureClient)
    .AddHttpMessageHandler<HttpLoadingHandler>();
builder.Services.AddHttpClient<IAuditClient, AuditClient>(configureClient)
    .AddHttpMessageHandler<HttpLoadingHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
