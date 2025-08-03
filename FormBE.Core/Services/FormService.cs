using FormBE.Persistence.Model;
using FormBE.Persistence.Repositories;
using FormBE.Persistence.Util;
using OneOf.Types;
using OneOf;

namespace FormBE.Core.Services;

public interface IFormService
{
    /// <summary>
    /// Get all forms.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="IReadOnlyCollection{T}"/> of <see cref="Form"/>s</returns>
    public ValueTask<IReadOnlyCollection<Form>> GetFormsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a <see cref="Form"/> by its id.
    /// </summary>
    /// <param name="formId">The id of the form.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Form"/> or a <see cref="NotFound"/> if the form was not found.</returns>
    public ValueTask<OneOf<Form, NotFound>>
        GetFormByIdAsync(long formId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new <see cref="Form"/>.
    /// </summary>
    /// <param name="groupId">The optional group id of the new form.</param>
    /// <param name="name">The name of the new form.</param>
    /// <param name="fieldGroupIds">The field groups of the form.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Success"/> with the new <see cref="Form"/> or a <see cref="GroupNotFound"/> if the group was not found.</returns>
    public ValueTask<OneOf<Success<Form>, GroupNotFound>> CreateFormAsync(
        long? groupId, string name, List<long> fieldGroupIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a form by its id.
    /// </summary>
    /// <param name="formId">The id of the form to update.</param>
    /// <param name="groupId">The new group id.</param>
    /// <param name="name">The new name.</param>
    /// <param name="fieldGroupIds">The ids of the new field groups.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Success"/>. A <see cref="NotFound"/> if the form id was not found or a <see cref="GroupNotFound"/> if the group id was not found.</returns>
    public ValueTask<OneOf<Success, NotFound, GroupNotFound>> UpdateFormAsync(
        long formId, long? groupId, string name, List<long> fieldGroupIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a form by its id.
    /// </summary>
    /// <param name="formId">The id of the form to delete.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Success"/> or a <see cref="NotFound"/> if the formId was not found.</returns>
    public ValueTask<OneOf<Success, NotFound>> DeleteFormAsync(long formId,
                                                               CancellationToken cancellationToken = default);

    public struct GroupNotFound;
}

internal class FormService(
    IFormRepository formRepository,
    IGroupRepository groupRepository,
    IFieldGroupRepository fieldGroupRepository,
    IUnitOfWork uow,
    ILogger<FormService> logger) : IFormService
{
    public async ValueTask<IReadOnlyCollection<Form>> GetFormsAsync(CancellationToken cancellationToken = default) =>
        await formRepository.GetFormsAsync(cancellationToken);

    public async ValueTask<OneOf<Form, NotFound>> GetFormByIdAsync(long formId,
                                                                   CancellationToken cancellationToken = default)
    {
        Form? form = await formRepository.GetFormByIdAsync(formId, false, cancellationToken);

        if (form == null)
        {
            logger.LogInformation("Tried to get form with id {FormId}, but was not found", formId);

            return new NotFound();
        }

        return form;
    }

    public async ValueTask<OneOf<Success<Form>, IFormService.GroupNotFound>> CreateFormAsync(
        long? groupId, string name, List<long> fieldGroupIds, CancellationToken cancellationToken = default)
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
        
        IReadOnlyCollection<FieldGroup> fieldGroups
            = await fieldGroupRepository.GetFieldGroupsByIdsAsync(cancellationToken, fieldGroupIds);

        form.FormFieldGroups = fieldGroups.Select(g => new FormFieldGroup()
        {
            Form = form,
            FieldGroup = g
        }).ToList();

        formRepository.AddForm(form);
        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created form with id {FormId}", form.Id);

        return new Success<Form>(form);
    }

    public async ValueTask<OneOf<Success, NotFound, IFormService.GroupNotFound>> UpdateFormAsync(
        long formId, long? groupId, string name, List<long> fieldGroupIds,
        CancellationToken cancellationToken = default)
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
                logger.LogInformation("Tried to update form with id {FormId} to be in group with id {GroupId}, but the group was not found",
                                      formId, groupId.Value);

                return new IFormService.GroupNotFound();
            }

            form.GroupId = groupId.Value;
        }

        if (form.Name != name)
        {
            form.Name = name;
        }

        HashSet<long> currentFieldGroupIds = form.FormFieldGroups.Select(ffg => ffg.FieldGroupId).ToHashSet();
        HashSet<long> newFieldGroupIds = fieldGroupIds.ToHashSet();
        
        if (!currentFieldGroupIds.SetEquals(newFieldGroupIds))
        {
            IReadOnlyCollection<FieldGroup> fieldGroups
                = await fieldGroupRepository.GetFieldGroupsByIdsAsync(cancellationToken, fieldGroupIds);
            
            form.FormFieldGroups = fieldGroups.Select(g => new FormFieldGroup()
            {
                Form = form,
                FieldGroup = g
            }).ToList();
        }

        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Updated form with id {FormId}", form.Id);

        return new Success();
    }

    public async ValueTask<OneOf<Success, NotFound>> DeleteFormAsync(long formId,
                                                                     CancellationToken cancellationToken = default)
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
