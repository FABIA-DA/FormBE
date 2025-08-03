using FormBE.Persistence.Model;
using FormBE.Persistence.Repositories;
using FormBE.Persistence.Util;
using FormBE.Shared;
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
    public ValueTask<IReadOnlyCollection<FieldGroup>>
        GetFieldGroupsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a <see cref="FieldGroup"/> by its id.
    /// </summary>
    /// <param name="fieldGroupId">The id of the field group to get.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The field group or a <see cref="NotFound"/>.</returns>
    public ValueTask<OneOf<FieldGroup, NotFound>> GetFieldGroupByIdAsync(
        long fieldGroupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new <see cref="FieldGroup"/> by passing its initial values.
    /// </summary>
    /// <param name="name">The name of the new field group.</param>
    /// <param name="singleChoiceFieldIds">The ids of the single choice fields to add, where invalid ids are ignored.</param>
    /// <param name="fieldIds">The ids of the fields to add, where invalid ids are ignored.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The new <see cref="FieldGroup"/>.</returns>
    public ValueTask<FieldGroup> CreateFieldGroupAsync(string name, List<long> singleChoiceFieldIds,
                                                       List<long> fieldIds,
                                                       CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a <see cref="FieldGroup"/> by its id and override its fields.
    /// </summary>
    /// <param name="fieldGroupId">The id of the field group to update.</param>
    /// <param name="name">The new name.</param>
    /// <param name="singleChoiceFieldIds">The ids of the new single choice fields to add, where invalid ids are ignored.</param>
    /// <param name="fieldIds">The ids of the new fields to add, where invalid ids are ignored.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Success"/> if there were no problems or a <see cref="NotFound"/> if the field group was not found.</returns>
    public ValueTask<OneOf<Success, NotFound>> UpdateFieldGroupAsync(long fieldGroupId, string name,
                                                                     List<long> singleChoiceFieldIds,
                                                                     List<long> fieldIds,
                                                                     CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a <see cref="FieldGroup"/> with its id.
    /// </summary>
    /// <param name="fieldGroupId">The id of the field group to delete.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Success"/> if there were no problems or a <see cref="NotFound"/> if the field group was not found.</returns>
    public ValueTask<OneOf<Success, NotFound>> DeleteFieldGroupAsync(long fieldGroupId,
                                                                     CancellationToken cancellationToken = default);
}

internal class FieldGroupService(
    IFieldGroupRepository fieldGroupRepository,
    ISingleChoiceFieldRepository singleChoiceFieldRepository,
    IFieldRepository fieldRepository,
    IUnitOfWork uow,
    ILogger<FieldGroupService> logger) : IFieldGroupService
{
    public async ValueTask<IReadOnlyCollection<FieldGroup>>
        GetFieldGroupsAsync(CancellationToken cancellationToken = default) =>
        await fieldGroupRepository.GetFieldGroupsAsync(cancellationToken);

    public async ValueTask<OneOf<FieldGroup, NotFound>> GetFieldGroupByIdAsync(
        long fieldGroupId, CancellationToken cancellationToken = default)
    {
        FieldGroup? fieldGroup
            = await fieldGroupRepository.GetFieldGroupByIdAsync(fieldGroupId, false, cancellationToken);

        if (fieldGroup == null)
        {
            logger.LogInformation("Tried to get field group with id {FieldGroupId}, but was not found", fieldGroupId);

            return new NotFound();
        }

        return fieldGroup;
    }

    public async ValueTask<FieldGroup> CreateFieldGroupAsync(string name, List<long> singleChoiceFieldIds,
                                                             List<long> fieldIds,
                                                             CancellationToken cancellationToken = default)
    {
        FieldGroup fieldGroup = new()
        {
            Name = name,
            FormFieldGroups = [],
            FieldGroupSingleChoiceFields = [],
            FieldGroupFields = []
        };

        IReadOnlyCollection<SingleChoiceField> singleChoiceFields
            = await singleChoiceFieldRepository.GetSingleChoiceFieldsByIdsAsync(cancellationToken,
                                                                                singleChoiceFieldIds);

        IReadOnlyCollection<Field> fields = await fieldRepository.GetFieldsByIdsAsync(cancellationToken,
         fieldIds);

        fieldGroup.FieldGroupSingleChoiceFields = singleChoiceFields.Select(f => new FieldGroupSingleChoiceField()
        {
            FieldGroup = fieldGroup,
            SingleChoiceField = f
        }).ToList();

        fieldGroup.FieldGroupFields = fields.Select(f => new FieldGroupField()
        {
            FieldGroup = fieldGroup,
            Field = f
        }).ToList();

        fieldGroupRepository.AddFieldGroup(fieldGroup);
        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created field group with id {FieldGroupId}", fieldGroup.Id);

        return fieldGroup;
    }

    public async ValueTask<OneOf<Success, NotFound>> UpdateFieldGroupAsync(long fieldGroupId, string name,
                                                                           List<long> singleChoiceFieldIds,
                                                                           List<long> fieldIds,
                                                                           CancellationToken cancellationToken
                                                                               = default)
    {
        FieldGroup? fieldGroup
            = await fieldGroupRepository.GetFieldGroupByIdAsync(fieldGroupId, true, cancellationToken);

        if (fieldGroup == null)
        {
            logger.LogInformation("Tried to update a field group with id {FieldGroupId}, but was not found",
                                  fieldGroupId);

            return new NotFound();
        }

        if (!fieldGroup.FieldGroupSingleChoiceFields.IdsEqual(singleChoiceFieldIds, f => f.SingleChoiceFieldId))
        {
            (List<long> newIds, List<FieldGroupSingleChoiceField> stillSingleChoiceFields,
             List<FieldGroupSingleChoiceField> oldSingleChoiceFields)
                = fieldGroup.FieldGroupSingleChoiceFields.SeparateItemsById(singleChoiceFieldIds,
                                                                            fgscf => fgscf.SingleChoiceFieldId);

            foreach (FieldGroupSingleChoiceField fgscf in oldSingleChoiceFields)
            {
                fieldGroupRepository.RemoveFieldGroupSingleChoiceField(fgscf);
            }
            
            List<FieldGroupSingleChoiceField> newSingleChoiceFields
                = (await singleChoiceFieldRepository.GetSingleChoiceFieldsByIdsAsync(cancellationToken,
                 newIds)).Select(f => new FieldGroupSingleChoiceField()
                {
                    FieldGroup = fieldGroup,
                    SingleChoiceField = f
                }).ToList();

            foreach (FieldGroupSingleChoiceField fgscf in newSingleChoiceFields)
            {
                fieldGroupRepository.RemoveFieldGroupSingleChoiceField(fgscf);
            }

            fieldGroup.FieldGroupSingleChoiceFields = stillSingleChoiceFields.Concat(newSingleChoiceFields).ToList();
        }

        if (!fieldGroup.FieldGroupFields.IdsEqual(fieldIds, f => f.FieldId))
        {
            (List<long> newIds, List<FieldGroupField> stillFieldGroupFields, List<FieldGroupField> oldfieldGroupFields)
                = fieldGroup.FieldGroupFields.SeparateItemsById(fieldIds, fgf => fgf.FieldId);

            foreach (FieldGroupField fgf in oldfieldGroupFields)
            {
                fieldGroupRepository.RemoveFieldGroupField(fgf);
            }

            List<FieldGroupField> newFields = (await fieldRepository.GetFieldsByIdsAsync(cancellationToken, newIds))
                                              .Select(f => new FieldGroupField()
                                              {
                                                  FieldGroup = fieldGroup,
                                                  Field = f
                                              }).ToList();

            foreach (FieldGroupField fgf in newFields)
            {
                fieldGroupRepository.AddFieldGroupField(fgf);
            }

            fieldGroup.FieldGroupFields = stillFieldGroupFields.Concat(newFields).ToList();
        }

        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Updated field group with id {FieldGroupId}", fieldGroup.Id);

        return new Success();
    }

    public async ValueTask<OneOf<Success, NotFound>> DeleteFieldGroupAsync(long fieldGroupId,
                                                                           CancellationToken cancellationToken
                                                                               = default)
    {
        FieldGroup? fieldGroup
            = await fieldGroupRepository.GetFieldGroupByIdAsync(fieldGroupId, true, cancellationToken);

        if (fieldGroup == null)
        {
            logger.LogInformation("Tried to delete field group with id {FieldGroupId}, but was not found",
                                  fieldGroupId);

            return new NotFound();
        }

        fieldGroupRepository.RemoveFieldGroup(fieldGroup);
        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Deleted the field group with id {FieldGroupId}", fieldGroup.Id);

        return new Success();
    }
}
