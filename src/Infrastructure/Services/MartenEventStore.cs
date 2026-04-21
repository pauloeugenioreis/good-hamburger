using System.Text.Json;
using Marten;
using Marten.Events;
using Marten.Events.Projections;
using Microsoft.Extensions.Logging;
using GoodHamburger.Domain;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Infrastructure.Services;

/// <summary>
/// Marten-based Event Store implementation
/// </summary>
public class MartenEventStore : IEventStore
{
    private readonly IDocumentStore _documentStore;
    private readonly EventSourcingSettings _settings;
    private readonly ILogger<MartenEventStore> _logger;

    public MartenEventStore(IDocumentStore documentStore, EventSourcingSettings settings, ILogger<MartenEventStore> logger)
    {
        _documentStore = documentStore;
        _settings = settings;
        _logger = logger;
    }

    public async Task AppendEventAsync<TEvent>(
        string aggregateType,
        string aggregateId,
        TEvent eventData,
        string? userId = null,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default) where TEvent : class
    {
        await using var session = _documentStore.LightweightSession();

        // Create stream ID combining type and ID
        var streamId = $"{aggregateType}-{aggregateId}";

        // Append to Marten event stream
        session.Events.Append(streamId, new object[] { eventData });

        await session.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<DomainEvent>> GetEventsAsync(
        string aggregateType,
        string aggregateId,
        CancellationToken cancellationToken = default)
    {
        await using var session = _documentStore.QuerySession();
        var streamId = $"{aggregateType}-{aggregateId}";

        var events = await session.Events.FetchStreamAsync(streamId, token: cancellationToken);

        return events
            .Select(e => ConvertToTypedEvent(e.Data, e))
            .OfType<DomainEvent>()
            .ToList();
    }

    public async Task<(List<DomainEvent> Items, long TotalCount)> GetEventsPagedAsync(
        string aggregateType,
        string aggregateId,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default)
    {
        await using var session = _documentStore.QuerySession();
        var streamId = $"{aggregateType}-{aggregateId}";

        // Marten's FetchStreamAsync doesn't support offset/limit directly in a built-in way 
        // to return a paged list with total count easily on the stream itself, 
        // but we can query the events table directly or fetch and slice.
        // For aggregate streams (usually small), we fetch all and slice.
        var allEvents = await session.Events.FetchStreamAsync(streamId, token: cancellationToken);
        var totalCount = allEvents.Count;

        var items = allEvents
            .Skip(offset ?? 0)
            .Take(limit ?? 100)
            .Select(e => ConvertToTypedEvent(e.Data, e))
            .OfType<DomainEvent>()
            .ToList();

        return (items, totalCount);
    }

    public async Task<List<DomainEvent>> GetEventsAsync(
        string aggregateType,
        string aggregateId,
        DateTime until,
        CancellationToken cancellationToken = default)
    {
        await using var session = _documentStore.QuerySession();
        var streamId = $"{aggregateType}-{aggregateId}";

        var events = await session.Events.FetchStreamAsync(streamId, token: cancellationToken);

        return events
            .Where(e => e.Timestamp <= until)
            .Select(e => ConvertToTypedEvent(e.Data, e))
            .OfType<DomainEvent>()
            .ToList();
    }

    public async Task<List<DomainEvent>> GetEventsByVersionAsync(
        string aggregateType,
        string aggregateId,
        int fromVersion,
        int toVersion,
        CancellationToken cancellationToken = default)
    {
        await using var session = _documentStore.QuerySession();
        var streamId = $"{aggregateType}-{aggregateId}";

        var events = await session.Events.FetchStreamAsync(streamId, token: cancellationToken);

        return events
            .Where(e => e.Version >= fromVersion && e.Version <= toVersion)
            .Select(e => ConvertToTypedEvent(e.Data, e))
            .OfType<DomainEvent>()
            .ToList();
    }

    public async Task<(List<DomainEvent> Items, long TotalCount)> GetEventsByTypeAsync(
        string aggregateType,
        DateTime? from = null,
        DateTime? toDate = null,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default)
    {
        await using var session = _documentStore.QuerySession();

        var query = session.Events.QueryAllRawEvents()
            .Where(e => e.StreamKey != null && e.StreamKey.StartsWith(aggregateType + "-"))
            .AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(e => e.Timestamp >= from.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.Timestamp <= toDate.Value);
        }

        query = query.OrderByDescending(e => e.Timestamp);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        if (offset.HasValue && offset.Value > 0)
        {
            query = query.Skip(offset.Value);
        }
        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        var events = await query.ToListAsync(cancellationToken);

        var items = events
            .Select(e => ConvertToTypedEvent(e.Data, e))
            .OfType<DomainEvent>()
            .ToList();

        return (items, totalCount);
    }

    public async Task<(List<DomainEvent> Items, long TotalCount)> GetEventsByUserAsync(
        string userId,
        DateTime? from = null,
        DateTime? toDate = null,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default)
    {
        await using var session = _documentStore.QuerySession();

        var query = session.Events.QueryAllRawEvents()
            .Where(e => ((DomainEvent)e.Data).UserId == userId)
            .AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(e => e.Timestamp >= from.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.Timestamp <= toDate.Value);
        }

        query = query.OrderByDescending(e => e.Timestamp);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        if (offset.HasValue && offset.Value > 0)
        {
            query = query.Skip(offset.Value);
        }
        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        var events = await query.ToListAsync(cancellationToken);

        var items = events
            .Select(e => ConvertToTypedEvent(e.Data, e))
            .OfType<DomainEvent>()
            .ToList();

        return (items, totalCount);
    }

    public async Task<int> GetLatestVersionAsync(
        string aggregateType,
        string aggregateId,
        CancellationToken cancellationToken = default)
    {
        await using var session = _documentStore.QuerySession();
        var streamId = $"{aggregateType}-{aggregateId}";

        var state = await session.Events.FetchStreamStateAsync(streamId, cancellationToken);

        return (int)(state?.Version ?? 0);
    }

    public async Task SaveSnapshotAsync<TSnapshot>(
        string aggregateType,
        string aggregateId,
        TSnapshot snapshot,
        int version,
        CancellationToken cancellationToken = default) where TSnapshot : class
    {
        if (!_settings.StoreSnapshots)
        {
            return;
        }

        await using var session = _documentStore.LightweightSession();

        var snapshotDoc = new SnapshotDocument<TSnapshot>
        {
            Id = $"{aggregateType}-{aggregateId}",
            AggregateType = aggregateType,
            AggregateId = aggregateId,
            Snapshot = snapshot,
            Version = version,
            CreatedAt = DateTime.UtcNow
        };

        session.Store(snapshotDoc);
        await session.SaveChangesAsync(cancellationToken);
    }

    public async Task<(TSnapshot? Snapshot, int Version)> GetSnapshotAsync<TSnapshot>(
        string aggregateType,
        string aggregateId,
        CancellationToken cancellationToken = default) where TSnapshot : class
    {
        if (!_settings.StoreSnapshots)
        {
            return (null, 0);
        }

        await using var session = _documentStore.QuerySession();
        var id = $"{aggregateType}-{aggregateId}";

        var snapshot = await session.LoadAsync<SnapshotDocument<TSnapshot>>(id, cancellationToken);

        if (snapshot == null)
        {
            return (null, 0);
        }

        return (snapshot.Snapshot, snapshot.Version);
    }

    public async Task<EventStatistics> GetStatisticsAsync(
        DateTime? from = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        await using var session = _documentStore.QuerySession();

        var query = session.Events.QueryAllRawEvents().AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(e => e.Timestamp >= from.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.Timestamp <= toDate.Value);
        }

        var events = await query.ToListAsync(cancellationToken);

        var stats = new EventStatistics
        {
            TotalEvents = events.Count
        };

        if (events.Any())
        {
            stats.OldestEvent = events.Min(e => e.Timestamp).DateTime;
            stats.LatestEvent = events.Max(e => e.Timestamp).DateTime;

            // Group by event type
            stats.EventsByType = events
                .Select(e => ConvertToTypedEvent(e.Data, e))
                .Where(e => e != null)
                .GroupBy(e => e!.EventType)
                .ToDictionary(g => g.Key, g => (long)g.Count());

            // Group by aggregate type
            stats.EventsByAggregateType = events
                .Select(e => ConvertToTypedEvent(e.Data, e))
                .Where(e => e != null)
                .GroupBy(e => e!.AggregateType)
                .ToDictionary(g => g.Key, g => (long)g.Count());
        }

        return stats;
    }

    private DomainEvent? ConvertToTypedEvent(object eventData, dynamic envelope)
    {
        try
        {
            // Serialize the actual event object (e.g. ProductCreatedEvent)
            var json = JsonSerializer.Serialize(eventData);
            
            // Deserialize into the API DTO (GoodHamburger.Domain.Entities.DomainEvent)
            var dto = JsonSerializer.Deserialize<DomainEvent>(json);
            
            if (dto != null)
            {
                // Hydrate DTO with metadata from Marten envelope
                dto.EventId = envelope.Id;
                dto.Version = (int)envelope.Version;
                dto.EventType = envelope.EventTypeName;
                dto.Timestamp = envelope.Timestamp.UtcDateTime;
                dto.OccurredOn = envelope.Timestamp.UtcDateTime;
                
                // Parse StreamKey to get AggregateType and AggregateId (format: "Type-Id")
                if (!string.IsNullOrEmpty(envelope.StreamKey))
                {
                    var firstDash = envelope.StreamKey.IndexOf('-');
                    if (firstDash > 0)
                    {
                        dto.AggregateType = envelope.StreamKey.Substring(0, firstDash);
                        dto.AggregateId = envelope.StreamKey.Substring(firstDash + 1);
                    }
                    else
                    {
                        dto.AggregateId = envelope.StreamKey;
                    }
                }

                // Ensure EventData contains the full JSON for the details view
                dto.EventData = JsonSerializer.Deserialize<object>(json);
            }
            
            return dto;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to convert event data to DomainEvent. Type: {Type}", eventData.GetType().Name);
            return null;
        }
    }
}

/// <summary>
/// Snapshot document for Marten storage
/// </summary>
internal class SnapshotDocument<TSnapshot> where TSnapshot : class
{
    public string Id { get; set; } = string.Empty;
    public string AggregateType { get; set; } = string.Empty;
    public string AggregateId { get; set; } = string.Empty;
    public TSnapshot Snapshot { get; set; } = default!;
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
}
