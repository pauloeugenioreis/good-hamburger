using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GoodHamburger.Domain.Dtos;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Infrastructure.Services;

/// <summary>
/// Default implementation that logs exceptions to console
/// Override this service to implement custom notification logic (email, Slack, etc.)
/// </summary>
public class ExceptionNotificationService : IExceptionNotificationService
{
    private readonly ILogger<ExceptionNotificationService> _logger;

    public ExceptionNotificationService(ILogger<ExceptionNotificationService> logger)
    {
        _logger = logger;
    }

    public Task NotifyAsync(ExceptionContext context, Exception exception)
    {
        try
        {
            // Log exception details
            _logger.LogError(exception,
                "Exception occurred for user {User} on {Method} {Path} at {Timestamp}",
                context.User, context.Method, context.Path, context.Timestamp);

            // Log additional context if available
            if (context.AdditionalInfo != null && context.AdditionalInfo.Any())
            {
                _logger.LogDebug("Additional context: {Context}",
                    string.Join(", ", context.AdditionalInfo.Select(kvp => $"{kvp.Key}={kvp.Value}")));
            }

            if (!string.IsNullOrEmpty(context.IpAddress))
            {
                _logger.LogDebug("Client IP: {IpAddress}", context.IpAddress);
            }

            // TODO: Implement custom notification logic here
            // Examples:
            // - Send email to admin
            // - Post to Slack channel
            // - Create issue in issue tracker
            // - Store in error database
        }
        catch (Exception ex)
        {
            // Don't throw exceptions in error handler
            _logger.LogError(ex, "Failed to send exception notification");
        }

        return Task.CompletedTask;
    }
}
