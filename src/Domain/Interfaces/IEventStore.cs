using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Domain.Interfaces;

/// <summary>
/// Event Store interface for storing and retrieving domain events
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Append a new event to the event stream
    /// </summary>
    Task AppendEventAsync<TEvent>(
        string aggregateType,
        string aggregateId,
        TEvent eventData,
        string? userId = null,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default) where TEvent : class;

    /// <summary>
    /// Get all events for a specific aggregate
    /// </summary>
    Task<List<DomainEvent>> GetEventsAsync(
        string aggregateType,
        string aggregateId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get events for a specific aggregate (paginated)
    /// </summary>
    Task<(List<DomainEvent> Items, long TotalCount)> GetEventsPagedAsync(
        string aggregateType,
        string aggregateId,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get events for an aggregate up to a specific timestamp (time travel)
    /// </summary>
    Task<List<DomainEvent>> GetEventsAsync(
        string aggregateType,
        string aggregateId,
        DateTime until,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get events by version range
    /// </summary>
    Task<List<DomainEvent>> GetEventsByVersionAsync(
        string aggregateType,
        string aggregateId,
        int fromVersion,
        int toVersion,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all events for a specific entity type (paginated)
    /// </summary>
    Task<(List<DomainEvent> Items, long TotalCount)> GetEventsByTypeAsync(
        string aggregateType,
        DateTime? from = null,
        DateTime? toDate = null,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get events by user who triggered them
    /// </summary>
    Task<(List<DomainEvent> Items, long TotalCount)> GetEventsByUserAsync(
        string userId,
        DateTime? from = null,
        DateTime? toDate = null,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get latest version number for an aggregate
    /// </summary>
    Task<int> GetLatestVersionAsync(
        string aggregateType,
        string aggregateId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Save snapshot of aggregate state for performance optimization
    /// </summary>
    Task SaveSnapshotAsync<TSnapshot>(
        string aggregateType,
        string aggregateId,
        TSnapshot snapshot,
        int version,
        CancellationToken cancellationToken = default) where TSnapshot : class;

    /// <summary>
    /// Get latest snapshot for an aggregate
    /// </summary>
    Task<(TSnapshot? Snapshot, int Version)> GetSnapshotAsync<TSnapshot>(
        string aggregateType,
        string aggregateId,
        CancellationToken cancellationToken = default) where TSnapshot : class;

    /// <summary>
    /// Get event statistics for auditing and monitoring
    /// </summary>
    Task<EventStatistics> GetStatisticsAsync(
        DateTime? from = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Event statistics for monitoring
/// </summary>
public class EventStatistics
{
    public long TotalEvents { get; set; }
    public Dictionary<string, long> EventsByType { get; set; } = new();
    public Dictionary<string, long> EventsByAggregateType { get; set; } = new();
    public DateTime OldestEvent { get; set; }
    public DateTime LatestEvent { get; set; }
}
