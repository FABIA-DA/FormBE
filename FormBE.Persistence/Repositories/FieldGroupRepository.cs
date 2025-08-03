using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface IFieldGroupRepository
{
    /// <summary>
    /// Gets a field group by its id.
    /// </summary>
    /// <param name="fieldGroupId">The id of the field group.</param>
    /// <param name="tracking">If the field group should be tracked by EF Core.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The requested field group or null if not found.</returns>
    public ValueTask<FieldGroup?> GetFieldGroupByIdAsync(long fieldGroupId, bool tracking = true,
                                                         CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all field groups without tracking.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>All field groups without tracking.</returns>
    public ValueTask<IReadOnlyCollection<FieldGroup>>
        GetFieldGroupsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a subset of existing field groups with tracking.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="fieldGroupIds">The ids of the field groups to get.</param>
    /// <returns>Gets the requested field groups with tracking.</returns>
    public ValueTask<IReadOnlyCollection<FieldGroup>> GetFieldGroupsByIdsAsync(CancellationToken cancellationToken = default, params List<long> fieldGroupIds);

    /// <summary>
    /// Adds a new field group, so that it is tracked.
    /// </summary>
    /// <param name="fieldGroup">The field group to add.</param>
    public void AddFieldGroup(FieldGroup fieldGroup);
    
    /// <summary>
    /// Adds a field group to the tracking with the <see cref="EntityState.Deleted"/> state.
    /// </summary>
    /// <param name="fieldGroup">The field group to delete.</param>
    public void RemoveFieldGroup(FieldGroup fieldGroup);
}

internal class FieldGroupRepository(DbSet<FieldGroup> fieldGroups) : IFieldGroupRepository
{
    private IQueryable<FieldGroup> FieldGroups => fieldGroups;
    private IQueryable<FieldGroup> NoTracking => FieldGroups.AsNoTracking();

    public async ValueTask<FieldGroup?> GetFieldGroupByIdAsync(long fieldGroupId, bool tracking = true,
                                                               CancellationToken cancellationToken = default)
    {
        IQueryable<FieldGroup> query = FieldGroups;

        if (!tracking)
        {
            query = NoTracking;
        }

        FieldGroup? fieldGroup = await query.Include(f => f.FieldGroupSingleChoiceFields)
                                            .Include(f => f.FieldGroupFields)
                                            .FirstOrDefaultAsync(g => g.Id == fieldGroupId, cancellationToken);

        return fieldGroup;
    }

    public async ValueTask<IReadOnlyCollection<FieldGroup>> GetFieldGroupsAsync(
        CancellationToken cancellationToken = default)
    {
        IQueryable<FieldGroup> query = NoTracking;
        
        IReadOnlyCollection<FieldGroup> coll = await query.ToListAsync(cancellationToken);

        return coll;
    }

    public async ValueTask<IReadOnlyCollection<FieldGroup>> GetFieldGroupsByIdsAsync(CancellationToken cancellationToken = default, params List<long> fieldGroupIds)
    {
        if (fieldGroupIds.Count == 0)
        {
            return [];
        }
        
        IQueryable<FieldGroup> query = FieldGroups.Where(g => fieldGroupIds.Contains(g.Id));
        
        IReadOnlyCollection<FieldGroup> coll = await query.ToListAsync(cancellationToken);

        return coll;
    }

    public void AddFieldGroup(FieldGroup fieldGroup)
    {
        fieldGroups.Add(fieldGroup);
    }

    public void RemoveFieldGroup(FieldGroup fieldGroup)
    {
        fieldGroups.Remove(fieldGroup);
    }
}
