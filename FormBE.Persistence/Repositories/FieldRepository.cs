using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface IFieldRepository
{
    /// <summary>
    /// Get a field by its id.
    /// </summary>
    /// <param name="fieldId">The id of the field.</param>
    /// <param name="tracking">If EF Core should track this entity.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The specified field or null if not found.</returns>
    public ValueTask<Field?> GetFieldByIdAsync(long fieldId, bool tracking = true,
                                               CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all fields without tracking. 
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>All fields without tracking.</returns>
    public ValueTask<IReadOnlyCollection<Field>> GetFieldsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a subset of existing fields with tracking, where invalid ids are ignored.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="fieldIds">The ids of the fields to get.</param>
    /// <returns>Gets the requested fields with tracking.</returns>
    public ValueTask<IReadOnlyCollection<Field>> GetFieldsByIdsAsync(CancellationToken cancellationToken = default, params List<long> fieldIds);
    
    /// <summary>
    /// Add a field to EF Cores tracking.
    /// </summary>
    /// <param name="field">The field to add.</param>
    public void AddField(Field field);
    
    /// <summary>
    /// Begin to track a field with the <see cref="EntityState.Deleted"/> state. 
    /// </summary>
    /// <param name="field">The field to delete.</param>
    public void RemoveField(Field field);
}

internal class FieldRepository(DbSet<Field> fields) : IFieldRepository
{
    private IQueryable<Field> Fields => fields;
    private IQueryable<Field> NoTracking => Fields.AsNoTracking();

    public async ValueTask<Field?> GetFieldByIdAsync(long fieldId, bool tracking = true,
                                                     CancellationToken cancellationToken = default)
    {
        IQueryable<Field> query = Fields;

        if (!tracking)
        {
            query = NoTracking;
        }

        Field? field = await query.Include(f => f.FieldType)
                                  .FirstOrDefaultAsync(f => f.Id == fieldId, cancellationToken);

        return field;
    }

    public async ValueTask<IReadOnlyCollection<Field>> GetFieldsAsync(CancellationToken cancellationToken = default)
    {
        IQueryable<Field> query = NoTracking;

        IReadOnlyCollection<Field> coll = await query.ToListAsync(cancellationToken);

        return coll;
    }

    public async ValueTask<IReadOnlyCollection<Field>> GetFieldsByIdsAsync(CancellationToken cancellationToken = default, params List<long> fieldIds)
    {
        if (fieldIds.Count == 0)
        {
            return [];
        }
        
        IQueryable<Field> query = Fields.Where(f => fieldIds.Contains(f.Id));
        
        IReadOnlyCollection<Field> coll = await query.Include(f => f.FieldType)
                                                     .ToListAsync(cancellationToken);
        
        return coll;
    }

    public void AddField(Field field)
    {
        fields.Add(field);
    }

    public void RemoveField(Field field)
    {
        fields.Remove(field);
    }
}
