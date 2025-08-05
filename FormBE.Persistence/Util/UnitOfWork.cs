using System.Data;
using FormBE.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FormBE.Persistence.Util;

public interface ITransactionProvider : IAsyncDisposable, IDisposable
{
    public ValueTask BeginTransactionAsync(CancellationToken cancellationToken = default);
    public ValueTask CommitAsync(CancellationToken cancellationToken = default);
    public ValueTask RollbackAsync(CancellationToken cancellationToken = default);
}

public interface IUnitOfWork
{
    public IGroupRepository GroupRepository { get; }
    public IFormRepository FormRepository { get; }
    public IFieldGroupRepository FieldGroupRepository { get; }
    public ISingleChoiceFieldRepository SingleChoiceFieldRepository { get; }
    public IOptionResponseRepository OptionResponseRepository { get; }
    public IFieldRepository FieldRepository { get; }
    public IFieldTypeRepository FieldTypeRepository { get; }
    public IFieldResponseRepository FieldResponseRepository { get; }
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

internal sealed class UnitOfWork(DatabaseContext context, ILogger<UnitOfWork> logger)
    : IUnitOfWork, ITransactionProvider
{
    private IDbContextTransaction? _transaction;

    public IGroupRepository GroupRepository => new GroupRepository(context.Groups);
    public IFormRepository FormRepository => new FormRepository(context.Forms, context.FormFieldGroups);
    public IFieldGroupRepository FieldGroupRepository => new FieldGroupRepository(context.FieldGroups, context.FieldGroupSingleChoiceFields, context.FieldGroupFields);
    public ISingleChoiceFieldRepository SingleChoiceFieldRepository => new SingleChoiceFieldRepository(context.SingleChoiceFields, context.Options, context.OptionFields);
    public IOptionResponseRepository OptionResponseRepository => new OptionResponseRepository(context.OptionResponses, context.Options);
    public IFieldRepository FieldRepository => new FieldRepository(context.Fields);
    public IFieldTypeRepository FieldTypeRepository => new FieldTypeRepository(context.FieldTypes);
    public IFieldResponseRepository FieldResponseRepository => new FieldResponseRepository(context.FieldResponses);
    
    public async ValueTask BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            throw new TransactionException("Transaction already started, unable to start another");
        }

        _transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Snapshot, cancellationToken);
    }

    public async ValueTask CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            throw new TransactionException("No transaction started, unable to commit");
        }

        await _transaction.CommitAsync(cancellationToken);
        _transaction = null;
    }

    public async ValueTask RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            throw new TransactionException("No transaction started, unable to rollback");
        }

        await _transaction.RollbackAsync(cancellationToken);
        _transaction = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is null)
        {
            return;
        }

        // Transaction was neither committed nor rolled back, rolling back now - silent, this is acceptable
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
    }

    public void Dispose()
    {
        if (_transaction is null)
        {
            return;
        }

        logger
            .LogWarning($"Transaction was not disposed in {nameof(DisposeAsync)} and will now be rolled back and disposed in {nameof(Dispose)}");
        _transaction.Rollback();
        _transaction.Dispose();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);

    private sealed class TransactionException(string message) : Exception(message);
}
