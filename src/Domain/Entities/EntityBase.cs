namespace GoodHamburger.Domain.Entities;

/// <summary>
/// Base entity class with common properties for all entities
/// </summary>
public abstract class EntityBase
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public virtual bool IsActive { get; set; } = true;
}
