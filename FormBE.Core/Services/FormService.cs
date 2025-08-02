using FormBE.Persistence.Model;
using FormBE.Persistence.Repositories;
using FormBE.Persistence.Util;
using OneOf.Types;
using OneOf;

namespace FormBE.Core.Services;

public interface IFormService
{
    public ValueTask<IReadOnlyCollection<Form>> GetFormsAsync(CancellationToken cancellationToken = default);
    public ValueTask<OneOf<Form, NotFound>> GetFormByIdAsync(long formId, CancellationToken cancellationToken = default);
    public ValueTask<OneOf<Success<Form>, GroupNotFound>> CreateFormAsync(long? groupId, string name, CancellationToken cancellationToken = default);
    public ValueTask<OneOf<Success, NotFound, GroupNotFound>> UpdateFormAsync(long formId, long? groupId, string name, CancellationToken cancellationToken = default);
    public ValueTask<OneOf<Success, NotFound>> DeleteFormAsync(long formId, CancellationToken cancellationToken = default);

    public struct GroupNotFound;
}

internal class FormService(IFormRepository formRepository, IGroupRepository groupRepository, IUnitOfWork uow, ILogger<FormService> logger) : IFormService
{
    public async ValueTask<IReadOnlyCollection<Form>> GetFormsAsync(CancellationToken cancellationToken = default) =>
        await formRepository.GetFormsAsync(cancellationToken);

    public async ValueTask<OneOf<Form, NotFound>> GetFormByIdAsync(long formId, CancellationToken cancellationToken = default)
    {
        Form? form = await formRepository.GetFormByIdAsync(formId, false, cancellationToken);

        if (form == null)
        {
            logger.LogInformation("Tried to get form with id {FormId}, but was not found", formId);

            return new NotFound();
        }

        return form;
    }

    public async ValueTask<OneOf<Success<Form>, IFormService.GroupNotFound>> CreateFormAsync(long? groupId, string name, CancellationToken cancellationToken = default)
    {
        Form form = new()
        {
            GroupId = groupId,
            Name = name,
            FormFieldGroups = []
        };
        
        if (groupId.HasValue)
        {
            Group? group = await groupRepository.GetGroupByIdAsync(groupId.Value, false, cancellationToken);

            if (group == null)
            {
                logger.LogInformation("Tried to create form, but the group with id {GroupId} was not found", groupId);

                return new IFormService.GroupNotFound();
            }
        }

        formRepository.AddForm(form);
        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created form with id {FormId}", form.Id);
        
        return new Success<Form>(form);
    }

    public async ValueTask<OneOf<Success, NotFound, IFormService.GroupNotFound>> UpdateFormAsync(long formId, long? groupId, string name, CancellationToken cancellationToken = default)
    {
        Form? form = await formRepository.GetFormByIdAsync(formId, true, cancellationToken);

        if (form == null)
        {
            logger.LogInformation("Tried to update form with id {FormId}, but form was not found", formId);

            return new NotFound();
        }

        if (groupId.HasValue &&
            form.GroupId != groupId.Value)
        {
            Group? group = await groupRepository.GetGroupByIdAsync(groupId.Value, false, cancellationToken);

            if (group == null)
            {
                logger.LogInformation("Tried to update form with id {FormId} to be in group with id {GroupId}, but the group was not found", formId, groupId.Value);
                return new IFormService.GroupNotFound();
            }

            form.GroupId = groupId.Value;
        }

        if (form.Name != name)
        {
            form.Name = name;
        }
        
        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Updated form with id {FormId}", form.Id);

        return new Success();
    }

    public async ValueTask<OneOf<Success, NotFound>> DeleteFormAsync(long formId, CancellationToken cancellationToken = default)
    {
        Form? form = await formRepository.GetFormByIdAsync(formId, true, cancellationToken);

        if (form == null)
        {
            logger.LogInformation("Tried to delete form with id {FormId}, but was not found", formId);

            return new NotFound();
        }
        
        formRepository.RemoveForm(form);
        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Deleted form with id {FormId}", formId);

        return new Success();
    }
}
