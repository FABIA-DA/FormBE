using FormBE.Persistence.Model;
using FormBE.Persistence.Repositories;
using FormBE.Persistence.Util;
using OneOf.Types;
using OneOf;

namespace FormBE.Core.Services;

public interface IFieldTypeService
{
    /// <summary>
    /// Get all <see cref="FieldType"/>s.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="IReadOnlyCollection{FieldType}"/> of <see cref="FieldType"/>s.</returns>
    public ValueTask<IReadOnlyCollection<FieldType>> GetFieldTypesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a specific <see cref="FieldType"/> by its id.
    /// </summary>
    /// <param name="fieldTypeId">The id of the field type to get.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The <see cref="FieldType"/> or a <see cref="NotFound"/> if it was not found.</returns>
    public ValueTask<OneOf<FieldType, NotFound>> GetFieldTypeByIdAsync(long fieldTypeId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a new <see cref="FieldType"/> with its initial values.
    /// </summary>
    /// <param name="name">The initial name of the field type.</param>
    /// <param name="description">Its initial description.</param>
    /// <param name="regex">The initial regex.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The newly created <see cref="FieldType"/>.</returns>
    public ValueTask<FieldType> CreateFieldTypeAsync(string name, string? description, string regex, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates a <see cref="FieldType"/> with the new values.
    /// </summary>
    /// <param name="fieldTypeId">The id of the field type to update.</param>
    /// <param name="name">The new name.</param>
    /// <param name="description">The new description.</param>
    /// <param name="regex">The new regex.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>Either a <see cref="Success"/> or a <see cref="NotFound"/> if the field type was not found.</returns>
    public ValueTask<OneOf<Success, NotFound>> UpdateFieldTypeAsync(long fieldTypeId, string name, string? description, string regex, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a <see cref="FieldType"/> by its id.
    /// </summary>
    /// <param name="fieldTypeId">the id of the field type to delete.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe whil waiting for the task to complete.</param>
    /// <returns>Either a <see cref="Success"/> or a <see cref="NotFound"/> if the <see cref="FieldType"/> was not found.</returns>
    public ValueTask<OneOf<Success, NotFound>> DeleteFieldTypeAsync(long fieldTypeId, CancellationToken cancellationToken = default);
}

internal class FieldTypeService(IUnitOfWork uow, ILogger<FieldTypeService> logger) : IFieldTypeService
{
    private IFieldTypeRepository FieldTypeRepository => uow.FieldTypeRepository;
    
    public async ValueTask<IReadOnlyCollection<FieldType>> GetFieldTypesAsync(CancellationToken cancellationToken = default) => await FieldTypeRepository.GetAllFieldTypes(cancellationToken);

    public async ValueTask<OneOf<FieldType, NotFound>> GetFieldTypeByIdAsync(long fieldTypeId, CancellationToken cancellationToken = default)
    {
        FieldType? fieldType = await FieldTypeRepository.GetFieldTypeByIdAsync(fieldTypeId, false, cancellationToken);

        if (fieldType == null)
        {
            logger.LogInformation("Tried to get field type with id {FieldTypeId}, but was not found", fieldTypeId);

            return new NotFound();
        }

        return fieldType;
    }

    public async ValueTask<FieldType> CreateFieldTypeAsync(string name, string? description, string regex,
                                                     CancellationToken cancellationToken = default)
    {
        FieldType type = new()
        {
            Name = name,
            Description = description,
            Regex = regex,
            Fields = []
        };
        
        FieldTypeRepository.AddFieldType(type);
        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created field type with id {FieldTypeId}", type.Id);

        return type;
    }

    public async ValueTask<OneOf<Success, NotFound>> UpdateFieldTypeAsync(long fieldTypeId, string name, string? description, string regex,
                                                                    CancellationToken cancellationToken = default)
    {
        FieldType? type = await FieldTypeRepository.GetFieldTypeByIdAsync(fieldTypeId, true, cancellationToken);

        if (type == null)
        {
            logger.LogInformation("Tried to update field type with id {FieldTypeId}, but not found", fieldTypeId);
            return new NotFound();
        }

        if (type.Name != name)
        {
            type.Name = name;
        }

        if (type.Description != description)
        {
            type.Description = description;
        }

        if (type.Regex != regex)
        {
            type.Regex = regex;
        }
        
        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Updated field type with id {FieldTypeId}", fieldTypeId);

        return new Success();
    }

    public async ValueTask<OneOf<Success, NotFound>> DeleteFieldTypeAsync(long fieldTypeId, CancellationToken cancellationToken = default)
    {
        FieldType? type = await FieldTypeRepository.GetFieldTypeByIdAsync(fieldTypeId, true, cancellationToken);

        if (type == null)
        {
            logger.LogInformation("Tried to delete field type with id {FieldTypeId}, but was not found", fieldTypeId);
            return new NotFound();
        }
        
        FieldTypeRepository.RemoveFieldType(type);
        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Deleted field type with id {FieldTypeId}", fieldTypeId);

        return new Success();
    }
}
