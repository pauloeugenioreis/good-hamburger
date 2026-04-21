namespace GoodHamburger.Domain.Events;

/// <summary>
/// Base class for all domain events
/// </summary>
public abstract class DomainEvent
{
    /// <summary>
    /// Unique identifier for the event
    /// </summary>
    public Guid EventId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Type of the event (e.g., "OrderCreated", "OrderShipped")
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the aggregate (entity) this event belongs to
    /// </summary>
    public string AggregateId { get; set; } = string.Empty;

    /// <summary>
    /// Type of the aggregate (e.g., "Order", "Product")
    /// </summary>
    public string AggregateType { get; set; } = string.Empty;

    /// <summary>
    /// When the event occurred
    /// </summary>
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Version number for concurrency control
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// User who triggered the event
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Additional metadata (IP, User Agent, etc.)
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Generic domain event with typed data
/// </summary>
public class DomainEvent<TData> : DomainEvent where TData : class
{
    /// <summary>
    /// Event payload
    /// </summary>
    public TData Data { get; set; } = default!;
}
