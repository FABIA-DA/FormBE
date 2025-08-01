using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface IGroupRepository
{
    public ValueTask<Group?> GetGroupByIdAsync(int groupId, bool tracking = true,
                                               CancellationToken cancellationToken = default);

    public ValueTask<IReadOnlyCollection<Group>> GetGroupsAsync(CancellationToken cancellationToken = default);
    public void AddGroup(Group group);
    public void RemoveGroup(Group group);
}

internal class GroupRepository(DbSet<Group> groups) : IGroupRepository
{
    private IQueryable<Group> Groups => groups;
    private IQueryable<Group> NoTracking => Groups.AsNoTracking();

    public async ValueTask<Group?> GetGroupByIdAsync(int groupId, bool tracking = true,
                                                     CancellationToken cancellationToken = default)
    {
        IQueryable<Group> query = Groups;

        if (!tracking)
        {
            query = NoTracking;
        }

        Group? group = await query.Include(g => g.Forms)
                                  .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);

        return group;
    }

    public async ValueTask<IReadOnlyCollection<Group>> GetGroupsAsync(CancellationToken cancellationToken = default)
    {
        IQueryable<Group> query = NoTracking;

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
