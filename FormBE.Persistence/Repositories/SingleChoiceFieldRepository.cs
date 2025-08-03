using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface ISingleChoiceFieldRepository
{
    /// <summary>
    /// Get a single choice field by its id.
    /// </summary>
    /// <param name="singleChoiceFieldId">The id of the single choice field.</param>
    /// <param name="tracking">If EF Core should track the entity.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The single choice field to get or null if not found.</returns>
    public ValueTask<SingleChoiceField?> GetSingleChoiceFieldByIdAsync(long singleChoiceFieldId, bool tracking = true,
                                                                       CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all single choice fields without tracking.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>All single choice fields without tracking.</returns>
    public ValueTask<IReadOnlyCollection<SingleChoiceField>> GetSingleChoiceFieldsAsync(
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a subset of existing single choice fields with tracking.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="singleChoiceFieldIds">The ids of the single choice fields to get.</param>
    /// <returns>Gets the requested single choice fields with tracking.</returns>
    public ValueTask<IReadOnlyCollection<SingleChoiceField>> GetSingleChoiceFieldsByIdsAsync(CancellationToken cancellationToken = default, params List<long> singleChoiceFieldIds);

    /// <summary>
    /// Add a single choice field to the tracking of EF Core.
    /// </summary>
    /// <param name="field">The single choice field to add.</param>
    public void AddSingleChoiceField(SingleChoiceField field);
    
    /// <summary>
    /// Add a single choice field to the tracking with a <see cref="EntityState.Deleted"/> state.
    /// </summary>
    /// <param name="field">The single choice field to delete.</param>
    public void RemoveSingleChoiceField(SingleChoiceField field);
}

internal class SingleChoiceFieldRepository(DbSet<SingleChoiceField> singleChoiceFields) : ISingleChoiceFieldRepository
{
    private IQueryable<SingleChoiceField> SingleChoiceFields => singleChoiceFields;
    private IQueryable<SingleChoiceField> NoTracking => SingleChoiceFields.AsNoTracking();

    public async ValueTask<SingleChoiceField?> GetSingleChoiceFieldByIdAsync(
        long singleChoiceFieldId, bool tracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<SingleChoiceField> query = SingleChoiceFields;

        if (!tracking)
        {
            query = NoTracking;
        }

        SingleChoiceField? field = await query.Include(f => f.Options)
                                              .FirstOrDefaultAsync(f => f.Id == singleChoiceFieldId, cancellationToken);

        return field;
    }

    public async ValueTask<IReadOnlyCollection<SingleChoiceField>> GetSingleChoiceFieldsAsync(
        CancellationToken cancellationToken = default)
    {
        IQueryable<SingleChoiceField> query = NoTracking;

        IReadOnlyCollection<SingleChoiceField> coll = await query.ToListAsync(cancellationToken);

        return coll;
    }

    public async ValueTask<IReadOnlyCollection<SingleChoiceField>> GetSingleChoiceFieldsByIdsAsync(CancellationToken cancellationToken = default,
                                                                                             params List<long> singleChoiceFieldIds)
    {
        if (singleChoiceFieldIds.Count == 0)
        {
            return [];
        }
        
        IQueryable<SingleChoiceField> query = SingleChoiceFields.Where(f => singleChoiceFieldIds.Contains(f.Id));
        
        IReadOnlyCollection<SingleChoiceField> coll = await query.ToListAsync(cancellationToken);
        
        return coll;
    }

    public void AddSingleChoiceField(SingleChoiceField field)
    {
        singleChoiceFields.Add(field);
    }

    public void RemoveSingleChoiceField(SingleChoiceField field)
    {
        singleChoiceFields.Remove(field);
    }
}
