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
    /// Gets all groups without tracking if the <see cref="groupIds"/> are empty or gets only those groups, which ids are in <see cref="groupIds"/> and enables tracking for those.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="groupIds">An optional set of ids to get if you only need those, but with tracking.</param>
    /// <returns>All groups without tracking if <see cref="groupIds"/> is empty or only those groups, which ids are in <see cref="groupIds"/> with tracking</returns>
    public ValueTask<IReadOnlyCollection<Group>> GetGroupsAsync(CancellationToken cancellationToken = default, params HashSet<long> groupIds);
    
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

        Group? group = await query.Include(g => g.Forms)
                                  .Include(g => g.SubGroups)
                                  .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);

        return group;
    }

    public async ValueTask<IReadOnlyCollection<Group>> GetGroupsAsync(CancellationToken cancellationToken = default, params HashSet<long> groupIds)
    {
        IQueryable<Group> query = NoTracking;

        if (groupIds.Count > 0)
        {
            query = Groups.Where(g => groupIds.Contains(g.Id));
        }

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
