using System;
using System.Threading.Tasks;
using GoodHamburger.Domain.Dtos;

namespace GoodHamburger.Domain.Interfaces;

/// <summary>
/// Service for notifying about exceptions
/// Decoupled from HTTP infrastructure to maintain clean architecture
/// </summary>
public interface IExceptionNotificationService
{
    /// <summary>
    /// Notifies about an exception that occurred during request processing
    /// </summary>
    /// <param name="context">Exception context information (user, path, method, etc.)</param>
    /// <param name="exception">The exception that occurred</param>
    Task NotifyAsync(ExceptionContext context, Exception exception);
}
