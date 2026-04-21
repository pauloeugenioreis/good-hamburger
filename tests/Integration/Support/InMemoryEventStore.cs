using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json;
using System.Threading;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Integration.Tests.Support;

/// <summary>
/// Lightweight in-memory implementation of <see cref="IEventStore"/> used by integration tests.
/// It preserves events between HTTP requests so we can exercise the AuditController without
/// provisioning Marten/PostgreSQL.
/// </summary>
internal sealed class InMemoryEventStore : IEventStore
{
    private readonly List<DomainEvent> _events = new();
    private readonly ConcurrentDictionary<string, (object Snapshot, int Version)> _snapshots = new();
    private readonly object _lock = new();
    private long _eventSequence;

    public Task AppendEventAsync<TEvent>(
        string aggregateType,
        string aggregateId,
        TEvent eventData,
        string? userId = null,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default) where TEvent : class
    {
        var timestamp = DateTime.UtcNow;
        DomainEvent domainEvent;

        lock (_lock)
        {
            var nextVersion = _events
                .Where(e => e.AggregateType == aggregateType && e.AggregateId == aggregateId)
                .Select(e => e.Version)
                .DefaultIfEmpty(0)
                .Max() + 1;

            domainEvent = new DomainEvent
            {
                Id = Interlocked.Increment(ref _eventSequence),
                EventId = Guid.NewGuid(),
                AggregateType = aggregateType,
                AggregateId = aggregateId,
                EventType = typeof(TEvent).Name,
                EventData = eventData,
                Timestamp = timestamp,
                OccurredOn = timestamp,
                Version = nextVersion,
                UserId = userId,
                Metadata = metadata ?? new Dictionary<string, string>()
            };

            _events.Add(domainEvent);
        }

        return Task.CompletedTask;
    }

    public Task<List<DomainEvent>> GetEventsAsync(
        string aggregateType,
        string aggregateId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(FilterByAggregate(aggregateType, aggregateId));
    }

    public Task<(List<DomainEvent> Items, long TotalCount)> GetEventsPagedAsync(
        string aggregateType,
        string aggregateId,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default)
    {
        var allEvents = FilterByAggregate(aggregateType, aggregateId);
        long totalCount = allEvents.Count;

        IEnumerable<DomainEvent> query = allEvents;
        if (offset.HasValue) query = query.Skip(offset.Value);
        if (limit.HasValue) query = query.Take(limit.Value);

        return Task.FromResult((query.ToList(), totalCount));
    }

    public Task<List<DomainEvent>> GetEventsAsync(
        string aggregateType,
        string aggregateId,
        DateTime until,
        CancellationToken cancellationToken = default)
    {
        var events = FilterByAggregate(aggregateType, aggregateId)
            .Where(e => e.OccurredOn <= until)
            .ToList();

        return Task.FromResult(events);
    }

    public Task<List<DomainEvent>> GetEventsByVersionAsync(
        string aggregateType,
        string aggregateId,
        int fromVersion,
        int toVersion,
        CancellationToken cancellationToken = default)
    {
        var events = FilterByAggregate(aggregateType, aggregateId)
            .Where(e => e.Version >= fromVersion && e.Version <= toVersion)
            .ToList();

        return Task.FromResult(events);
    }

    public Task<(List<DomainEvent> Items, long TotalCount)> GetEventsByTypeAsync(
        string aggregateType,
        DateTime? from = null,
        DateTime? toDate = null,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default)
    {
        var allEvents = SnapshotEvents()
            .Where(e => e.AggregateType == aggregateType)
            .Where(e => !from.HasValue || e.OccurredOn >= from.Value)
            .Where(e => !toDate.HasValue || e.OccurredOn <= toDate.Value)
            .OrderByDescending(e => e.OccurredOn)
            .ToList();

        long totalCount = allEvents.Count;
        IEnumerable<DomainEvent> query = allEvents;

        if (offset.HasValue)
        {
            query = query.Skip(offset.Value);
        }

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return Task.FromResult((query.ToList(), totalCount));
    }

    public Task<(List<DomainEvent> Items, long TotalCount)> GetEventsByUserAsync(
        string userId,
        DateTime? from = null,
        DateTime? toDate = null,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default)
    {
        var allEvents = SnapshotEvents()
            .Where(e => string.Equals(e.UserId, userId, StringComparison.OrdinalIgnoreCase))
            .Where(e => !from.HasValue || e.OccurredOn >= from.Value)
            .Where(e => !toDate.HasValue || e.OccurredOn <= toDate.Value)
            .OrderByDescending(e => e.OccurredOn)
            .ToList();

        long totalCount = allEvents.Count;
        IEnumerable<DomainEvent> query = allEvents;

        if (offset.HasValue)
        {
            query = query.Skip(offset.Value);
        }

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return Task.FromResult((query.ToList(), totalCount));
    }

    public Task<int> GetLatestVersionAsync(
        string aggregateType,
        string aggregateId,
        CancellationToken cancellationToken = default)
    {
        var version = FilterByAggregate(aggregateType, aggregateId)
            .Select(e => e.Version)
            .DefaultIfEmpty(0)
            .Max();

        return Task.FromResult(version);
    }

    public Task SaveSnapshotAsync<TSnapshot>(
        string aggregateType,
        string aggregateId,
        TSnapshot snapshot,
        int version,
        CancellationToken cancellationToken = default) where TSnapshot : class
    {
        var key = BuildStreamKey(aggregateType, aggregateId);
        _snapshots[key] = (snapshot, version);
        return Task.CompletedTask;
    }

    public Task<(TSnapshot? Snapshot, int Version)> GetSnapshotAsync<TSnapshot>(
        string aggregateType,
        string aggregateId,
        CancellationToken cancellationToken = default) where TSnapshot : class
    {
        var key = BuildStreamKey(aggregateType, aggregateId);
        if (_snapshots.TryGetValue(key, out var entry) && entry.Snapshot is TSnapshot typed)
        {
            return Task.FromResult<(TSnapshot?, int)>((typed, entry.Version));
        }

        return Task.FromResult<(TSnapshot?, int)>((null, 0));
    }

    public Task<EventStatistics> GetStatisticsAsync(
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var events = SnapshotEvents()
            .Where(e => !from.HasValue || e.OccurredOn >= from.Value)
            .Where(e => !to.HasValue || e.OccurredOn <= to.Value)
            .ToList();

        var stats = new EventStatistics
        {
            TotalEvents = events.Count,
            OldestEvent = events.Count > 0 ? events.Min(e => e.OccurredOn) : DateTime.MinValue,
            LatestEvent = events.Count > 0 ? events.Max(e => e.OccurredOn) : DateTime.MinValue,
            EventsByType = events
                .GroupBy(e => e.EventType)
                .ToDictionary(g => g.Key, g => (long)g.Count()),
            EventsByAggregateType = events
                .GroupBy(e => e.AggregateType)
                .ToDictionary(g => g.Key, g => (long)g.Count())
        };

        return Task.FromResult(stats);
    }

    private static string BuildStreamKey(string aggregateType, string aggregateId)
        => $"{aggregateType}:{aggregateId}";

    private List<DomainEvent> FilterByAggregate(string aggregateType, string aggregateId)
    {
        return SnapshotEvents()
            .Where(e => e.AggregateType == aggregateType && e.AggregateId == aggregateId)
            .OrderBy(e => e.Version)
            .ToList();
    }

    private IEnumerable<DomainEvent> SnapshotEvents()
    {
        lock (_lock)
        {
            return _events.Select(CloneEvent).ToList();
        }
    }

    private static DomainEvent CloneEvent(DomainEvent source) => new()
    {
        Id = source.Id,
        EventId = source.EventId,
        AggregateType = source.AggregateType,
        AggregateId = source.AggregateId,
        EventType = source.EventType,
        EventData = source.EventData,
        Timestamp = source.Timestamp,
        OccurredOn = source.OccurredOn,
        Version = source.Version,
        UserId = source.UserId,
        Metadata = source.Metadata
    };
}
