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
    /// Get all single choice fields without tracking or only specified ones with tracking.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="singleChoiceFieldIds">An optional set of ids to get with tracking.</param>
    /// <returns>All single choice fields without tracking or specified ones with tracking.</returns>
    public ValueTask<IReadOnlyCollection<SingleChoiceField>> GetSingleChoiceFieldsAsync(
        CancellationToken cancellationToken = default, params HashSet<long> singleChoiceFieldIds);

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
        CancellationToken cancellationToken = default, params HashSet<long> singleChoiceFieldIds)
    {
        IQueryable<SingleChoiceField> query = NoTracking;

        if (singleChoiceFieldIds.Count > 0)
        {
            query = SingleChoiceFields.Where(f => singleChoiceFieldIds.Contains(f.Id));
        }

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
