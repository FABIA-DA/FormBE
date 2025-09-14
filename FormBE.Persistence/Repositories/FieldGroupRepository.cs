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
    public ValueTask<IReadOnlyCollection<(long Id, string Name, int SingleChoiceFieldCount, int FieldCount)>>
        GetFieldGroupsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a subset of existing field groups with tracking.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="fieldGroupIds">The ids of the field groups to get.</param>
    /// <returns>Gets the requested field groups with tracking.</returns>
    public ValueTask<IReadOnlyCollection<FieldGroup>> GetFieldGroupsByIdsAsync(
        CancellationToken cancellationToken = default, params List<long> fieldGroupIds);

    /// <summary>
    /// Adds a new <see cref="FieldGroup"/>, so that it is tracked.
    /// </summary>
    /// <param name="fieldGroup">The field group to add.</param>
    public void AddFieldGroup(FieldGroup fieldGroup);

    /// <summary>
    /// Begin tracking of a field group with the <see cref="EntityState.Deleted"/> state.
    /// </summary>
    /// <param name="fieldGroup">The field group to delete.</param>
    public void RemoveFieldGroup(FieldGroup fieldGroup);

    /// <summary>
    /// Adds multiple new <see cref="FieldGroupSingleChoiceField"/> to the tracking.
    /// </summary>
    /// <param name="fields">The items to add.</param>
    public void AddFieldGroupSingleChoiceFields(params IEnumerable<FieldGroupSingleChoiceField> fields);

    /// <summary>
    /// Begins tracking of a variable amount of <see cref="FieldGroupSingleChoiceField"/> with the <see cref="EntityState.Deleted"/> state.
    /// </summary>
    /// <param name="fields">The items to delete.</param>
    public void RemoveFieldGroupSingleChoiceFields(params IEnumerable<FieldGroupSingleChoiceField> fields);

    /// <summary>
    /// Adds multiple new <see cref="FieldGroupField"/> to the tracking.
    /// </summary>
    /// <param name="fields">The items to add.</param>
    public void AddFieldGroupFields(params IEnumerable<FieldGroupField> fields);

    /// <summary>
    /// Begins tracking of multiple <see cref="FieldGroupField"/> with the <see cref="EntityState.Deleted"/> state.
    /// </summary>
    /// <param name="fields">The items to delete.</param>
    public void RemoveFieldGroupFields(params IEnumerable<FieldGroupField> fields);
}

internal class FieldGroupRepository(
    DbSet<FieldGroup> fieldGroups,
    DbSet<FieldGroupSingleChoiceField> fieldGroupSingleChoiceFields,
    DbSet<FieldGroupField> fieldGroupFields) : IFieldGroupRepository
{
    private IQueryable<FieldGroup> FieldGroups => fieldGroups;
    private IQueryable<FieldGroup> NoTracking => FieldGroups.AsNoTracking();

    private static IQueryable<FieldGroup> FullInclude(IQueryable<FieldGroup> self) =>
        self.Include(f => f.FieldGroupSingleChoiceFields)
            .ThenInclude(fgscf => fgscf.SingleChoiceField)
            .ThenInclude(scf => scf.Options)
            .Include(f => f.FieldGroupFields)
            .ThenInclude(fgf => fgf.Field)
            .ThenInclude(f => f.FieldType)
            .Include(f => f.FieldGroupFields)
            .ThenInclude(fgf => fgf.Field)
            .ThenInclude(f => f.FieldType)
            .AsSplitQuery();

    public async ValueTask<FieldGroup?> GetFieldGroupByIdAsync(long fieldGroupId, bool tracking = true,
                                                               CancellationToken cancellationToken = default)
    {
        IQueryable<FieldGroup> query = FieldGroups;

        if (!tracking)
        {
            query = NoTracking;
        }

        FieldGroup? fieldGroup = await FullInclude(query)
            .FirstOrDefaultAsync(g => g.Id == fieldGroupId, cancellationToken);

        return fieldGroup;
    }

    public async ValueTask<IReadOnlyCollection<(long Id, string Name, int SingleChoiceFieldCount, int FieldCount)>> GetFieldGroupsAsync(
        CancellationToken cancellationToken = default)
    {
        IQueryable<FieldGroup> query = NoTracking;

        IReadOnlyCollection<(long Id, string Name, int SingleChoiceFieldCount, int FieldCount)> coll = (await query
                                                     .Select(fg => new
                                                     {
                                                         Id = fg.Id,
                                                         Name = fg.Name,
                                                         SingleChoiceFieldCount = fg.FieldGroupSingleChoiceFields.Count,
                                                         FieldCount = fg.FieldGroupFields.Count
                                                     })
            .ToListAsync(cancellationToken)).Select(fg => (fg.Id, fg.Name, fg.SingleChoiceFieldCount, fg.FieldCount)).ToList();

        return coll;
    }

    public async ValueTask<IReadOnlyCollection<FieldGroup>> GetFieldGroupsByIdsAsync(
        CancellationToken cancellationToken = default, params List<long> fieldGroupIds)
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

    public void AddFieldGroupSingleChoiceFields(params IEnumerable<FieldGroupSingleChoiceField> fields)
    {
        fieldGroupSingleChoiceFields.AddRange(fields);
    }

    public void RemoveFieldGroupSingleChoiceFields(params IEnumerable<FieldGroupSingleChoiceField> fields)
    {
        fieldGroupSingleChoiceFields.RemoveRange(fields);
    }

    public void AddFieldGroupFields(params IEnumerable<FieldGroupField> fields)
    {
        fieldGroupFields.AddRange(fields);
    }

    public void RemoveFieldGroupFields(params IEnumerable<FieldGroupField> fields)
    {
        fieldGroupFields.RemoveRange(fields);
    }
}
