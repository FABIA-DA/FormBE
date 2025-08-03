using FormBE.Persistence.Model;
using FormBE.Persistence.Repositories;
using FormBE.Persistence.Util;
using OneOf.Types;
using OneOf;

namespace FormBE.Core.Services;

public interface IFieldGroupService
{
    /// <summary>
    /// Get all <see cref="FieldGroup"/>s.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="IReadOnlyCollection{T}"/> of <see cref="FieldGroup"/>s.</returns>
    public ValueTask<IReadOnlyCollection<FieldGroup>> GetFieldGroupsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a <see cref="FieldGroup"/> by its id.
    /// </summary>
    /// <param name="fieldGroupId">The id of the field group to get.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The field group or a <see cref="NotFound"/>.</returns>
    public ValueTask<OneOf<FieldGroup, NotFound>> GetFieldGroupByIdAsync(long fieldGroupId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public ValueTask<OneOf<Success<FieldGroup>, NotFound>> CreateFieldGroupAsync(string name, CancellationToken cancellationToken = default);
    public ValueTask<OneOf<Success, NotFound>> UpdateFieldGroupAsync(long fieldGroupId, string name, CancellationToken cancellationToken = default);
    public ValueTask<OneOf<Success, NotFound>> DeleteFieldGroupAsync(long fieldGroupId, CancellationToken cancellationToken = default);
}

internal class FieldGroupService(IFieldGroupRepository fieldGroupRepository, IUnitOfWork uow, ILogger<FieldGroupService> logger) : IFieldGroupService
{
    private void Dummy()
    {
        var a = fieldGroupRepository;
        var b = uow;
        var c = logger;
    }

    public ValueTask<IReadOnlyCollection<FieldGroup>> GetFieldGroupsAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public ValueTask<OneOf<FieldGroup, NotFound>> GetFieldGroupByIdAsync(long fieldGroupId, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public ValueTask<OneOf<Success<FieldGroup>, NotFound>> CreateFieldGroupAsync(string name, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public ValueTask<OneOf<Success, NotFound>> UpdateFieldGroupAsync(long fieldGroupId, string name, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public ValueTask<OneOf<Success, NotFound>> DeleteFieldGroupAsync(long fieldGroupId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
