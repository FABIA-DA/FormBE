using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface IGroupRepository
{
    /// <summary>
    /// Get a group by its id.
    /// </summary>
    /// <param name="groupId">The id of the group to get.</param>
    /// <param name="tracking">If EF Core should track the entity.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The group or null if not found.</returns>
    public ValueTask<Group?> GetGroupByIdAsync(long groupId, bool tracking = true,
                                               CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all groups without tracking.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>All groups without tracking.</returns>
    public ValueTask<IReadOnlyCollection<(long Id, string Name, long? ParentId, string? ParentName, int SubgroupCount,
        int FormCount)>> GetGroupsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a subset of existing groups with tracking.
    /// </summary>
    /// <param name="groupIds">The ids of the groups to get.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>Gets the requested groups with tracking.</returns>
    public ValueTask<IReadOnlyCollection<Group>> GetGroupsByIdsAsync(List<long> groupIds,
                                                                     CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a <see cref="group"/> to the tracking of EF Core.
    /// </summary>
    /// <param name="group">The group to be added.</param>
    public void AddGroup(Group group);

    /// <summary>
    /// Begins tracking of the <see cref="group"/> in a <see cref="EntityState.Deleted"/> state.
    /// </summary>
    /// <param name="group">The group to delete.</param>
    public void RemoveGroup(Group group);
}

internal class GroupRepository(DbSet<Group> groups) : IGroupRepository
{
    private IQueryable<Group> Groups => groups;
    private IQueryable<Group> NoTracking => Groups.AsNoTracking();

    public async ValueTask<Group?> GetGroupByIdAsync(long groupId, bool tracking = true,
                                                     CancellationToken cancellationToken = default)
    {
        IQueryable<Group> query = Groups;

        if (!tracking)
        {
            query = NoTracking;
        }

        Group? group = await query.Include(g => g.Parent)
                                  .Include(g => g.Forms)
                                  .ThenInclude(f => f.FormFieldGroups)
                                  .ThenInclude(ffg => ffg.FieldGroup)
                                  .ThenInclude(fg => fg.FieldGroupSingleChoiceFields)
                                  .ThenInclude(fgscf => fgscf.SingleChoiceField)
                                  .ThenInclude(scf => scf.Options)
                                  .ThenInclude(o => o.OptionFields)
                                  .ThenInclude(of => of.Field)
                                  .ThenInclude(f => f.FieldType)
                                  .Include(g => g.Forms)
                                  .ThenInclude(f => f.FormFieldGroups)
                                  .ThenInclude(ffg => ffg.FieldGroup)
                                  .ThenInclude(fg => fg.FieldGroupFields)
                                  .ThenInclude(fgf => fgf.Field)
                                  .ThenInclude(f => f.FieldType)
                                  .Include(g => g.SubGroups)
                                  .AsSplitQuery()
                                  .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);

        return group;
    }

    public async
        ValueTask<IReadOnlyCollection<(long Id, string Name, long? ParentId, string? ParentName, int SubgroupCount, int
            FormCount)>> GetGroupsAsync(CancellationToken cancellationToken = default)
    {
        IQueryable<Group> query = NoTracking;

        IReadOnlyCollection<(long Id, string Name, long? ParentId, string? ParentName, int SubgroupCount, int FormCount
            )> coll = (await query
                             .Select(g => new
                             {
                                 Id = g.Id, Name = g.Name, ParentId = g.ParentId,
                                 ParentName = g.Parent == null ? null : g.Parent.Name,
                                 SubgroupCount = g.SubGroups.Count, FormCount = g.Forms.Count
                             })
                             .ToListAsync(cancellationToken))
                      .Select(g => (g.Id, g.Name, g.ParentId, g.ParentName, g.SubgroupCount, g.FormCount)).ToList();

        return coll;
    }

    public async ValueTask<IReadOnlyCollection<Group>> GetGroupsByIdsAsync(
        List<long> groupIds, CancellationToken cancellationToken = default)
    {
        if (groupIds.Count == 0)
        {
            return [];
        }

        IQueryable<Group> query = Groups.Where(g => groupIds.Contains(g.Id));

        IReadOnlyCollection<Group> coll = await query.ToListAsync(cancellationToken);

        return coll;
    }

    public void AddGroup(Group group)
    {
        groups.Add(group);
    }

    public void RemoveGroup(Group group)
    {
        groups.Remove(group);
    }
}
