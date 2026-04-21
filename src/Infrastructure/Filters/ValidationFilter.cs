using System;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace GoodHamburger.Infrastructure.Filters;

/// <summary>
/// Action filter that automatically validates controller action arguments
/// using FluentValidation validators registered in DI container
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ValidationFilter> _logger;

    public ValidationFilter(IServiceProvider serviceProvider, ILogger<ValidationFilter> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Iterate through all action arguments
        foreach (var (key, argument) in context.ActionArguments)
        {
            if (argument == null)
            {
                continue;
            }

            var argumentType = argument.GetType();

            // Skip validation for primitive types, strings, and system types
            if (ShouldSkipValidation(argumentType))
            {
                continue;
            }

            // Look for generic IValidator<T> for the argument type
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
            var validator = _serviceProvider.GetService(validatorType) as IValidator;

            if (validator != null)
            {
                _logger.LogDebug("Validating argument {Key} of type {Type}", key, argumentType.Name);

                // Create validation context and execute validation
                var validationContext = new ValidationContext<object>(argument);
                var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

                // If validation fails, add errors to ModelState
                if (!result.IsValid)
                {
                    _logger.LogWarning("Validation failed for {Key} with {ErrorCount} errors",
                        key, result.Errors.Count);

                    foreach (var error in result.Errors)
                    {
                        context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    }

                    // Return BadRequest immediately
                    context.Result = new BadRequestObjectResult(context.ModelState);
                    return;
                }
            }
        }

        // If validation passed, continue to the next step in the pipeline
        await next();
    }

    private static bool ShouldSkipValidation(Type type)
    {
        // Primitive types (int, bool, char, etc.)
        if (type.IsPrimitive)
        {
            return true;
        }

        // Common system types
        if (type == typeof(string) ||
            type == typeof(decimal) ||
            type == typeof(DateTime) ||
            type == typeof(DateTimeOffset) ||
            type == typeof(TimeSpan) ||
            type == typeof(Guid))
        {
            return true;
        }

        // Nullable types
        if (Nullable.GetUnderlyingType(type) != null)
        {
            return true;
        }

        // Types from System namespace (except custom DTOs)
        if (type.Namespace?.StartsWith("System") == true)
        {
            return true;
        }

        // CancellationToken
        if (type == typeof(System.Threading.CancellationToken))
        {
            return true;
        }

        return false;
    }
}
