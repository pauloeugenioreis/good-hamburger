using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Data.Repository.Mongo;

/// <summary>
/// Generic MongoDB repository implementation.
/// Maps MongoEntityBase.Id to ObjectId via BsonClassMap (keeps Domain layer clean).
/// Each entity is stored in a collection named after the entity type in lowercase + "s".
/// </summary>
/// <typeparam name="TEntity">Document entity type inheriting MongoEntityBase</typeparam>
public class MongoRepository<TEntity> : IMongoRepository<TEntity>
    where TEntity : MongoEntityBase
{
    private static readonly object _classMapLock = new();
    private static bool _classMapRegistered;

    protected IMongoCollection<TEntity> Collection { get; }

    public MongoRepository(IMongoDatabase database)
    {
        EnsureClassMapRegistered();

        var collectionName = typeof(TEntity).Name.ToLowerInvariant() + "s";
        Collection = database.GetCollection<TEntity>(collectionName);
    }

    public virtual async Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        var filter = Builders<TEntity>.Filter.Eq(e => e.Id, id);
        return await Collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Collection.Find(_ => true).ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await Collection.Find(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<(IEnumerable<TEntity> Items, long Total)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.Empty;
        var total = await Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var items = await Collection
            .Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await Collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        return entity;
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        foreach (var entity in entityList)
        {
            entity.CreatedAt = DateTime.UtcNow;
        }

        await Collection.InsertManyAsync(entityList, cancellationToken: cancellationToken);
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        var filter = Builders<TEntity>.Filter.Eq(e => e.Id, entity.Id);
        await Collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
    }

    public virtual async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        var filter = Builders<TEntity>.Filter.Eq(e => e.Id, id);
        var result = await Collection.DeleteOneAsync(filter, cancellationToken);
        return result.DeletedCount > 0;
    }

    public virtual async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        return await Collection.CountDocumentsAsync(
            Builders<TEntity>.Filter.Empty,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Registers BsonClassMap for MongoEntityBase so the Domain layer
    /// stays free of MongoDB attributes. Maps Id to ObjectId.
    /// Thread-safe — runs once per AppDomain.
    /// </summary>
    private static void EnsureClassMapRegistered()
    {
        if (_classMapRegistered) return;

        lock (_classMapLock)
        {
            if (_classMapRegistered) return;

            if (!BsonClassMap.IsClassMapRegistered(typeof(MongoEntityBase)))
            {
                BsonClassMap.RegisterClassMap<MongoEntityBase>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdMember(c => c.Id)
                        .SetIdGenerator(StringObjectIdGenerator.Instance)
                        .SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
            }

            _classMapRegistered = true;
        }
    }
}
