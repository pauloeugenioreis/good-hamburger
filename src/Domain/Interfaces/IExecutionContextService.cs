namespace GoodHamburger.Domain.Interfaces;

/// <summary>
/// Service to provide execution context information (user, IP, etc.)
/// This abstraction keeps Domain/Data layers clean from HTTP dependencies
/// </summary>
public interface IExecutionContextService
{
    /// <summary>
    /// Gets the current user identifier (email, username, or "system" if not authenticated)
    /// </summary>
    string GetCurrentUserId();

    /// <summary>
    /// Gets contextual metadata about the current execution
    /// </summary>
    Dictionary<string, string> GetMetadata();
}
