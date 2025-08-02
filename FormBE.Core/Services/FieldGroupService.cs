using FormBE.Persistence.Model;
using FormBE.Persistence.Repositories;
using FormBE.Persistence.Util;
using OneOf.Types;
using OneOf;

namespace FormBE.Core.Services;

public interface IFieldGroupService
{
    public ValueTask<IReadOnlyCollection<FieldGroup>> GetFieldGroupsAsync(CancellationToken cancellationToken = default);
    public ValueTask<OneOf<FieldGroup, NotFound>> GetFieldGroupByIdAsync(long fieldGroupId, CancellationToken cancellationToken = default);
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
