using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Data.Repository;

/// <summary>
/// Generic Repository implementation using Entity Framework Core
/// This is the default ORM implementation
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
public class Repository<TEntity>(DbContext context) : IRepository<TEntity>, ITransactionalRepository where TEntity : class
{
    protected DbContext Context => context;
    private DbSet<TEntity> DbSet => context.Set<TEntity>();

    public virtual async Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        await DbSet.AddRangeAsync(entityList, cancellationToken);
        return entityList;
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        // Get the ID from the incoming entity via reflection-free property access
        var idValue = context.Entry(entity).Property(nameof(EntityBase.Id)).CurrentValue
            ?? throw new InvalidOperationException("Entity must have a non-null Id value");

        // Check if an entity with the same key is already tracked
        var tracked = context.ChangeTracker.Entries<TEntity>()
            .FirstOrDefault(e => Equals(e.Property(nameof(EntityBase.Id)).CurrentValue, idValue));

        if (tracked != null)
        {
            // Entity already tracked — copy incoming values to the tracked instance
            tracked.CurrentValues.SetValues(entity);
        }
        else
        {
            // Not tracked — attach and mark as modified
            DbSet.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        }

        await Task.CompletedTask;
    }

    public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        DbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }

    public virtual async Task<(IEnumerable<TEntity> Items, int Total)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking();

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(e => EF.Property<long>(e, nameof(EntityBase.Id)))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task<IRepositoryTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (!context.Database.IsRelational())
        {
            return new NoOpRepositoryTransaction();
        }

        var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        return new EfRepositoryTransaction(transaction);
    }

    public virtual async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(operation);
    }
}
