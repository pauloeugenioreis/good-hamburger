using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace GoodHamburger.Infrastructure.Extensions;

/// <summary>
/// Swagger extension methods with JWT authentication support
/// </summary>
public static class SwaggerExtension
{
    /// <summary>
    /// Add Swagger with JWT Bearer authentication
    /// </summary>
    public static IServiceCollection AddSwaggerWithAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "GoodHamburger API",
                Version = "v1",
                Description = "Enterprise .NET API",
                Contact = new OpenApiContact
                {
                    Name = "Paulo Eugênio da Silva dos Reis",
                    Email = "pauloeugenioreis@msn.com"
                }
            });

            // Add JWT Bearer authentication
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            // TODO: AddSecurityRequirement API mudou no Microsoft.OpenApi 2.x
            // Temporariamente comentado até ajustar para a nova API
            /*
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            */

            // Include XML comments if available
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    /// <summary>
    /// Use Swagger UI
    /// </summary>
    public static IApplicationBuilder UseSwaggerWithAuth(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "GoodHamburger API v1");
            c.RoutePrefix = "swagger"; // Serve Swagger UI at /swagger
        });

        return app;
    }
}
