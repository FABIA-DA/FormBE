using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface IGroupRepository
{

}

internal class GroupRepository(DbSet<Group> groups) : IGroupRepository
{
    private IQueryable<Group> Groups => groups;
}
