namespace GoodHamburger.Domain.Entities;

/// <summary>
/// Domain event base class for event sourcing
/// </summary>
public class DomainEvent
{
    public long Id { get; set; }
    public Guid EventId { get; set; } // Unique event identifier
    public string AggregateType { get; set; } = string.Empty;
    public string AggregateId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public object? EventData { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime OccurredOn { get; set; } // Alias for Timestamp
    public int Version { get; set; } // Event version for aggregate
    public string? UserId { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
