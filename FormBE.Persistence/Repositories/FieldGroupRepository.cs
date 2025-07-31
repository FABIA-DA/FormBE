using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface IFieldGroupRepository
{
    public ValueTask<FieldGroup?> GetFieldGroupByIdAsync(int fieldGroupId, bool tracking = true,
                                                         CancellationToken cancellationToken = default);

    public ValueTask<IReadOnlyCollection<FieldGroup>>
        GetFieldGroupsAsync(CancellationToken cancellationToken = default);

    public void AddFieldGroup(FieldGroup fieldGroup);
}

internal class FieldGroupRepository(DbSet<FieldGroup> fieldGroups) : IFieldGroupRepository
{
    private IQueryable<FieldGroup> FieldGroups => fieldGroups;
    private IQueryable<FieldGroup> NoTracking => FieldGroups.AsNoTracking();

    public async ValueTask<FieldGroup?> GetFieldGroupByIdAsync(int fieldGroupId, bool tracking = true,
                                                               CancellationToken cancellationToken = default)
    {
        IQueryable<FieldGroup> query = FieldGroups;

        if (!tracking)
        {
            query = NoTracking;
        }

        FieldGroup? fieldGroup = await query.Include(f => f.FieldGroupSingleChoiceFields)
                                            .Include(f => f.FieldGroupFields)
                                            .FirstOrDefaultAsync(cancellationToken);

        return fieldGroup;
    }

    public async ValueTask<IReadOnlyCollection<FieldGroup>> GetFieldGroupsAsync(
        CancellationToken cancellationToken = default)
    {
        IQueryable<FieldGroup> query = NoTracking;

        IReadOnlyCollection<FieldGroup> coll = await query.ToListAsync(cancellationToken);

        return coll;
    }

    public void AddFieldGroup(FieldGroup fieldGroup)
    {
        fieldGroups.Add(fieldGroup);
    }
}
