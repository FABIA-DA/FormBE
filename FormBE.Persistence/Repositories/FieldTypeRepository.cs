using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface IFieldTypeRepository
{
    /// <summary>
    /// Get a field type by its id.
    /// </summary>
    /// <param name="fieldTypeId">The id of the field tpye to get.</param>
    /// <param name="tracking">If EF Core should track the entity.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The specified field type or null if not found.</returns>
    public ValueTask<FieldType?> GetFieldTypeByIdAsync(long fieldTypeId, bool tracking = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all field types without tracking.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>All field types without tracking.</returns>
    public ValueTask<IReadOnlyCollection<FieldType>> GetAllFieldTypes(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Begins tracking of a field type.
    /// </summary>
    /// <param name="fieldType">The field type to add.</param>
    public void AddFieldType(FieldType fieldType);
    
    /// <summary>
    /// Begins to track a field type with the <see cref="EntityState.Deleted"/> state.
    /// </summary>
    /// <param name="fieldType">The field type to delete.</param>
    public void RemoveFieldType(FieldType fieldType);
}

internal class FieldTypeRepository(DbSet<FieldType> fieldTypes) : IFieldTypeRepository
{
    private IQueryable<FieldType> FieldTypes => fieldTypes;
    private IQueryable<FieldType> NoTracking => FieldTypes.AsNoTracking();
    
    public async ValueTask<FieldType?> GetFieldTypeByIdAsync(long fieldTypeId, bool tracking = true, CancellationToken cancellationToken = default)
    {
        IQueryable<FieldType> query = FieldTypes;

        if (!tracking)
        {
            query = NoTracking;
        }
        
        FieldType? fieldType = await query.FirstOrDefaultAsync(t => t.Id == fieldTypeId, cancellationToken);
        return fieldType;
    }

    public async ValueTask<IReadOnlyCollection<FieldType>> GetAllFieldTypes(CancellationToken cancellationToken = default)
    {
        IQueryable<FieldType> query = NoTracking;

        IReadOnlyCollection<FieldType> coll = await query.ToListAsync(cancellationToken);
        
        return coll;
    }

    public void AddFieldType(FieldType fieldType)
    {
        fieldTypes.Add(fieldType);
    }

    public void RemoveFieldType(FieldType fieldType)
    {
        fieldTypes.Remove(fieldType);
    }
}
