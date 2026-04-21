using Microsoft.AspNetCore.Builder;

namespace GoodHamburger.Infrastructure.Extensions;

/// <summary>
/// Extension methods for exception handling middleware
/// </summary>
public static class ExceptionHandlerExtension
{
    /// <summary>
    /// Adds global exception handler middleware to the pipeline
    /// Should be added early in the pipeline to catch all exceptions
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseMiddleware<Middleware.GlobalExceptionHandler>();
        return app;
    }
}
