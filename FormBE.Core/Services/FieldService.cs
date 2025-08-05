using FormBE.Persistence.Model;
using FormBE.Persistence.Repositories;
using FormBE.Persistence.Util;
using OneOf.Types;
using OneOf;

namespace FormBE.Core.Services;

public interface IFieldService
{
    /// <summary>
    /// Gets all <see cref="Field"/>s.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The requested fields.</returns>
    public ValueTask<IReadOnlyCollection<Field>> GetFieldsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a <see cref="Field"/> by its id.
    /// </summary>
    /// <param name="fieldId">The id of the field.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The <see cref="Field"/> or a <see cref="NotFound"/>.</returns>
    public ValueTask<OneOf<Field, NotFound>> GetFieldByIdAsync(long fieldId,
                                                               CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new <see cref="Field"/> with its initial values.
    /// </summary>
    /// <param name="fieldTypeId">The id of the field type.</param>
    /// <param name="name">The initial name of the field.</param>
    /// <param name="description">The initial description.</param>
    /// <param name="isOptional">If the field is optional to fill out.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Success"/> with the newly created <see cref="Field"/> or a <see cref="FieldTypeNotFound"/>.</returns>
    public ValueTask<OneOf<Success<Field>, FieldTypeNotFound>> CreateFieldAsync(
        long fieldTypeId, string name, string? description, bool isOptional,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the field with the new values.
    /// </summary>
    /// <param name="fieldId">The id of the field to update.</param>
    /// <param name="fieldTypeId">The id of the new field type.</param>
    /// <param name="name">The new name.</param>
    /// <param name="description">The new description.</param>
    /// <param name="isOptional">If the field is optional to fill out.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Success"/>, a <see cref="NotFound"/> if the field was not found or a <see cref="FieldTypeNotFound"/>.</returns>
    public ValueTask<OneOf<Success, NotFound, FieldTypeNotFound>> UpdateFieldAsync(
        long fieldId, long fieldTypeId, string name, string? description, bool isOptional,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a field by its id.
    /// </summary>
    /// <param name="fieldId">The id of the field to delete.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Success"/> or a <see cref="NotFound"/> if the field was not found.</returns>
    public ValueTask<OneOf<Success, NotFound>> DeleteFieldAsync(long fieldId,
                                                                CancellationToken cancellationToken = default);

    public struct FieldTypeNotFound;
}

internal class FieldService(
    IFieldRepository fieldRepository,
    IFieldTypeRepository fieldTypeRepository,
    IUnitOfWork uow,
    ILogger<FieldService> logger) : IFieldService
{
    public async ValueTask<IReadOnlyCollection<Field>> GetFieldsAsync(CancellationToken cancellationToken = default) =>
        await fieldRepository.GetFieldsAsync(cancellationToken);

    public async ValueTask<OneOf<Field, NotFound>> GetFieldByIdAsync(long fieldId,
                                                                     CancellationToken cancellationToken = default)
    {
        Field? field = await fieldRepository.GetFieldByIdAsync(fieldId, false, cancellationToken);

        if (field == null)
        {
            logger.LogInformation("Tried to get field with id {FieldId}, but was not found", fieldId);

            return new NotFound();
        }

        return field;
    }

    public async ValueTask<OneOf<Success<Field>, IFieldService.FieldTypeNotFound>> CreateFieldAsync(
        long fieldTypeId, string name, string? description, bool isOptional,
        CancellationToken cancellationToken = default)
    {
        FieldType? type = await fieldTypeRepository.GetFieldTypeByIdAsync(fieldTypeId, true, cancellationToken);

        if (type == null)
        {
            logger.LogInformation("Tried to create field with field type with id {FieldTypeId}, but field type was not found",
                                  fieldTypeId);

            return new IFieldService.FieldTypeNotFound();
        }

        Field field = new Field()
        {
            Name = name,
            Description = description,
            IsOptional = isOptional,
            FieldType = type,
            OptionFields = [],
            FieldGroupFields = [],
            FieldResponses = []
        };

        fieldRepository.AddField(field);
        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created field with id {FieldId}", field.Id);

        return new Success<Field>(field);
    }

    public async ValueTask<OneOf<Success, NotFound, IFieldService.FieldTypeNotFound>> UpdateFieldAsync(
        long fieldId, long fieldTypeId, string name, string? description, bool isOptional,
        CancellationToken cancellationToken = default)
    {
        Field? field = await fieldRepository.GetFieldByIdAsync(fieldId, true, cancellationToken);

        if (field == null)
        {
            logger.LogInformation("Tried to update field with id {FieldId}, but was not found", fieldId);

            return new NotFound();
        }

        if (field.FieldTypeId != fieldTypeId)
        {
            FieldType? type
                = await fieldTypeRepository.GetFieldTypeByIdAsync(fieldTypeId, true, cancellationToken);

            if (type == null)
            {
                logger.LogInformation("Tried to update field with id {FieldId} to field type wih id {FieldTypeId}, but field type was not found",
                                      fieldId, fieldTypeId);

                return new IFieldService.FieldTypeNotFound();
            }

            field.FieldType = type;
        }

        if (field.Name != name)
        {
            field.Name = name;
        }

        if (field.Description != description)
        {
            field.Description = description;
        }

        if (field.IsOptional != isOptional)
        {
            field.IsOptional = isOptional;
        }

        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Updated field with id {FieldId}", field.Id);

        return new Success();
    }

    public async ValueTask<OneOf<Success, NotFound>> DeleteFieldAsync(long fieldId,
                                                                      CancellationToken cancellationToken = default)
    {
        Field? field = await fieldRepository.GetFieldByIdAsync(fieldId, true, cancellationToken);

        if (field == null)
        {
            logger.LogInformation("Tried to delete field with id {FieldId}, but was not found", fieldId);

            return new NotFound();
        }

        fieldRepository.RemoveField(field);
        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Deleted field with id {FieldId}", field.Id);

        return new Success();
    }
}
