using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using GoodHamburger.Domain;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;
using GoodHamburger.Domain.Dtos;

namespace GoodHamburger.Api.Controllers;

/// <summary>
/// Audit API - Query event history and audit trails
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuditController : ApiControllerBase
{
    private const string EventSourcingDisabledMessage = "Event Sourcing or Audit API is disabled";
    private readonly IEventStore _eventStore;
    private readonly EventSourcingSettings _settings;

    public AuditController(IEventStore eventStore, EventSourcingSettings settings)
    {
        _eventStore = eventStore;
        _settings = settings;
    }

    /// <summary>
    /// Get history of events for an entity type (paginated)
    /// </summary>
    [HttpGet("{entityType}")]
    [ProducesResponseType(typeof(PagedResponse<DomainEvent>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEntitiesHistory(
        string entityType,
        [FromQuery] int? limit = 100,
        [FromQuery] int? offset = 0,
        CancellationToken cancellationToken = default)
    {
        return await GetHistoryInternal(entityType, null, limit, offset, cancellationToken);
    }

    /// <summary>
    /// Get history of events for a specific entity instance (paginated)
    /// </summary>
    [HttpGet("{entityType}/{entityId}")]
    [ProducesResponseType(typeof(PagedResponse<DomainEvent>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEntityHistory(
        string entityType,
        string entityId,
        [FromQuery] int? limit = 100,
        [FromQuery] int? offset = 0,
        CancellationToken cancellationToken = default)
    {
        return await GetHistoryInternal(entityType, entityId, limit, offset, cancellationToken);
    }

    private async Task<IActionResult> GetHistoryInternal(
        string entityType,
        string? entityId,
        int? limit,
        int? offset,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(entityType))
        {
            return BadRequest("entityType is required");
        }

        if (!_settings.Enabled || !_settings.EnableAuditApi)
        {
            return BadRequest(EventSourcingDisabledMessage);
        }

        int pageSize = limit ?? 100;
        int currentOffset = offset ?? 0;
        int page = (currentOffset / pageSize) + 1;

        List<DomainEvent> items;
        long total;

        if (string.IsNullOrWhiteSpace(entityId))
        {
            (items, total) = await _eventStore.GetEventsByTypeAsync(
                entityType, null, null, pageSize, currentOffset, cancellationToken);
        }
        else
        {
            (items, total) = await _eventStore.GetEventsPagedAsync(
                entityType, entityId, pageSize, currentOffset, cancellationToken);
        }

        return HandlePagedResult(items, total, page, pageSize);
    }

    /// <summary>
    /// Get entity state at a specific point in time (time travel)
    /// </summary>
    [HttpGet("{entityType}/{entityId}/at/{timestamp}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetStateAt(
        string entityType,
        string entityId,
        DateTime timestamp,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            return BadRequest(EventSourcingDisabledMessage);
        }

        if (!_settings.EnableAuditApi)
        {
            return StatusCode(403, EventSourcingDisabledMessage);
        }

        var events = await _eventStore.GetEventsAsync(entityType, entityId, timestamp, cancellationToken);

        if (events.Count == 0)
        {
            return NotFound($"No events found for {entityType} with ID {entityId}");
        }

        return Ok(new
        {
            EntityType = entityType,
            EntityId = entityId,
            Timestamp = timestamp,
            EventCount = events.Count,
            Events = events.OrderBy(e => e.Version).Select(e => new
            {
                e.EventType,
                e.Version,
                e.OccurredOn,
                e.UserId
            })
        });
    }

    /// <summary>
    /// Get events by version range
    /// </summary>
    [HttpGet("{entityType}/{entityId}/versions/{fromVersion}/{toVersion}")]
    [ProducesResponseType(typeof(List<DomainEvent>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<DomainEvent>>> GetEventsByVersion(
        string entityType,
        string entityId,
        int fromVersion,
        int toVersion,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled || !_settings.EnableAuditApi)
        {
            return BadRequest(EventSourcingDisabledMessage);
        }

        var events = await _eventStore.GetEventsByVersionAsync(
            entityType, entityId, fromVersion, toVersion, cancellationToken);

        return Ok(events);
    }


    /// <summary>
    /// Get events by user
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetEventsByUser(
        string userId,
        [FromQuery] DateTime? from = null,
        [FromQuery(Name = "to")] DateTime? toDate = null,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled || !_settings.EnableAuditApi)
        {
            return BadRequest(EventSourcingDisabledMessage);
        }

        var (items, total) = await _eventStore.GetEventsByUserAsync(userId, from, toDate, limit, 0, cancellationToken);

        return Ok(new
        {
            UserId = userId,
            EventCount = total,
            Events = items.OrderByDescending(e => e.OccurredOn).Select(e => new
            {
                e.EventId,
                e.EventType,
                e.AggregateType,
                e.AggregateId,
                e.OccurredOn,
                e.Version,
                e.Metadata
            })
        });
    }

    /// <summary>
    /// Get event statistics
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetStatistics(
        [FromQuery] DateTime? from = null,
        [FromQuery(Name = "to")] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled || !_settings.EnableAuditApi)
        {
            return BadRequest(EventSourcingDisabledMessage);
        }

        var stats = await _eventStore.GetStatisticsAsync(from, toDate, cancellationToken);

        return Ok(stats);
    }

    /// <summary>
    /// Replay events to rebuild state
    /// </summary>
    [HttpPost("{entityType}/{entityId}/replay")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ReplayEvents(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled || !_settings.EnableAuditApi)
        {
            return BadRequest(EventSourcingDisabledMessage);
        }

        var events = await _eventStore.GetEventsAsync(entityType, entityId, cancellationToken);

        if (!events.Any())
        {
            return NotFound($"No events found for {entityType} with ID {entityId}");
        }

        return Ok(new
        {
            EntityType = entityType,
            EntityId = entityId,
            EventCount = events.Count,
            ReconstructedState = "Event replay completed successfully",
            Events = events.OrderBy(e => e.Version).Select(e => new
            {
                e.EventType,
                e.Version,
                e.OccurredOn
            })
        });
    }
}
