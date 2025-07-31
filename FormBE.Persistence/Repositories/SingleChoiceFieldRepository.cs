using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface ISingleChoiceFieldRepository
{
    public ValueTask<SingleChoiceField?> GetSingleChoiceFieldByIdAsync(int singleChoiceFieldId, bool tracking = true,
                                                                       CancellationToken cancellationToken = default);

    public ValueTask<IReadOnlyCollection<SingleChoiceField>> GetSingleChoiceFieldsAsync(
        CancellationToken cancellationToken = default);

    public void AddSingleChoiceField(SingleChoiceField field);
}

internal class SingleChoiceFieldRepository(DbSet<SingleChoiceField> singleChoiceFields) : ISingleChoiceFieldRepository
{
    private IQueryable<SingleChoiceField> SingleChoiceFields => singleChoiceFields;
    private IQueryable<SingleChoiceField> NoTracking => SingleChoiceFields.AsNoTracking();

    public async ValueTask<SingleChoiceField?> GetSingleChoiceFieldByIdAsync(
        int singleChoiceFieldId, bool tracking = true,
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

    public void AddSingleChoiceField(SingleChoiceField field)
    {
        singleChoiceFields.Add(field);
    }
}
