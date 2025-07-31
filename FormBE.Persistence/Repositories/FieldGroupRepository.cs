using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface IFieldGroupRepository
{
    
}

internal class FieldGroupRepository(DbSet<FieldGroup> fieldGroups) : IFieldGroupRepository
{
    private IQueryable<FieldGroup> FieldGroups => fieldGroups;
}
