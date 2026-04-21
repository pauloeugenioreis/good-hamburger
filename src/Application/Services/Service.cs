using Microsoft.Extensions.Logging;
using GoodHamburger.Domain.Exceptions;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Application.Services;

/// <summary>
/// Generic Service base class implementing common business logic
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
public class Service<TEntity> : IService<TEntity> where TEntity : class
{
    protected readonly IRepository<TEntity> _repository;
    protected readonly ILogger<Service<TEntity>> _logger;

    public Service(IRepository<TEntity> repository, ILogger<Service<TEntity>> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public virtual async Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }

    public virtual async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var created = await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return created;
    }

    public virtual async Task UpdateAsync(long id, TEntity entity, CancellationToken cancellationToken = default)
    {
        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new NotFoundException($"Entity with ID {id} not found");
        }

        await _repository.DeleteAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task<(IEnumerable<TEntity> Items, int Total)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetPagedAsync(page, pageSize, cancellationToken);
    }
}
