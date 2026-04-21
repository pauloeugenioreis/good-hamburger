namespace GoodHamburger.Domain.Dtos;

/// <summary>
/// Exception context information for notification purposes
/// Decouples Domain layer from HTTP infrastructure
/// </summary>
public class ExceptionContext
{
    /// <summary>
    /// User identifier (username, email, or "Anonymous")
    /// </summary>
    public string User { get; set; } = "Anonymous";

    /// <summary>
    /// Request path (e.g., /api/products/123)
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// HTTP method (GET, POST, PUT, DELETE, etc.)
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// User IP address
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent (browser/client information)
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Additional context information (claims, headers, etc.)
    /// </summary>
    public Dictionary<string, string>? AdditionalInfo { get; set; }

    /// <summary>
    /// Timestamp when exception occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
