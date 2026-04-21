using Microsoft.AspNetCore.Http;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Infrastructure.Services;

/// <summary>
/// Implementation of execution context service using HTTP context
/// </summary>
public class ExecutionContextService : IExecutionContextService
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public ExecutionContextService(IHttpContextAccessor? httpContextAccessor = null)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentUserId()
    {
        if (_httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated == true)
        {
            return _httpContextAccessor.HttpContext.User.Identity.Name ?? "authenticated-user";
        }
        return "system";
    }

    public Dictionary<string, string> GetMetadata()
    {
        var metadata = new Dictionary<string, string>
        {
            ["Timestamp"] = DateTime.UtcNow.ToString("O"),
            ["MachineName"] = Environment.MachineName
        };

        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext != null)
        {
            metadata["IpAddress"] = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            metadata["UserAgent"] = httpContext.Request.Headers["User-Agent"].ToString();
            metadata["RequestPath"] = httpContext.Request.Path;
            metadata["RequestMethod"] = httpContext.Request.Method;
        }

        return metadata;
    }
}
