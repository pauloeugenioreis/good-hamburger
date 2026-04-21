using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GoodHamburger.Domain.Dtos;
using GoodHamburger.Web.Interfaces;

namespace GoodHamburger.Web.Services;

public sealed class AuditClient : BaseApiClient, IAuditClient
{
    public AuditClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public Task<ApiResult<PagedResponse<JsonElement>>> GetAuditHistoryAsync(
        string entityType,
        string? entityId = null,
        int? page = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var path = string.IsNullOrWhiteSpace(entityId)
            ? $"api/v1/Audit/{Uri.EscapeDataString(entityType)}"
            : $"api/v1/Audit/{Uri.EscapeDataString(entityType)}/{Uri.EscapeDataString(entityId)}";

        path = BuildPath(path, new Dictionary<string, string?>
        {
            ["page"] = page?.ToString(CultureInfo.InvariantCulture),
            ["pageSize"] = pageSize?.ToString(CultureInfo.InvariantCulture)
        });

        return SendAsync<PagedResponse<JsonElement>>(HttpMethod.Get, path, null, cancellationToken);
    }

    public Task<ApiResult<JsonElement>> ReplayAuditAsync(string entityType, string entityId, CancellationToken cancellationToken = default)
        => SendAsync<JsonElement>(HttpMethod.Post, $"api/v1/Audit/{Uri.EscapeDataString(entityType)}/{Uri.EscapeDataString(entityId)}/replay", null, cancellationToken);

    public Task<ApiResult<JsonElement>> GetAuditStatisticsAsync(CancellationToken cancellationToken = default)
        => SendAsync<JsonElement>(HttpMethod.Get, "api/v1/Audit/statistics", null, cancellationToken);
}
