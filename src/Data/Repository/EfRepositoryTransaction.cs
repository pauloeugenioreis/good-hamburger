using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Data.Repository;

/// <summary>
/// Wraps EF Core IDbContextTransaction so repositories can expose ORM-agnostic transactions.
/// </summary>
internal sealed class EfRepositoryTransaction : IRepositoryTransaction
{
    private readonly IDbContextTransaction _transaction;

    public EfRepositoryTransaction(IDbContextTransaction transaction)
    {
        _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return _transaction.CommitAsync(cancellationToken);
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        return _transaction.RollbackAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return _transaction.DisposeAsync();
    }
}
