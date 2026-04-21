namespace GoodHamburger.Domain.Entities;

/// <summary>
/// Base entity class for MongoDB documents.
/// Uses string Id (mapped to ObjectId via BsonClassMap in the Data layer).
/// Domain stays free of MongoDB dependencies — mapping is handled in MongoRepository.
/// </summary>
public abstract class MongoEntityBase
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
