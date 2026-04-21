using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GoodHamburger.Domain.Dtos;
using GoodHamburger.Web.Services;

namespace GoodHamburger.Web.Interfaces;

public interface IAuditClient
{
    Task<ApiResult<PagedResponse<JsonElement>>> GetAuditHistoryAsync(
        string entityType,
        string? entityId = null,
        int? limit = 100,
        int? offset = 0,
        CancellationToken cancellationToken = default);

    Task<ApiResult<JsonElement>> ReplayAuditAsync(string entityType, string entityId, CancellationToken cancellationToken = default);

    Task<ApiResult<JsonElement>> GetAuditStatisticsAsync(CancellationToken cancellationToken = default);
}
