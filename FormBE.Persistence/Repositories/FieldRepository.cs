using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface IFieldRepository
{
    public ValueTask<Field?> GetFieldByIdAsync(int fieldId, bool tracking = true,
                                               CancellationToken cancellationToken = default);

    public ValueTask<IReadOnlyCollection<Field>> GetAllFields(CancellationToken cancellationToken = default);
    public void AddField(Field field);
    public void RemoveField(Field field);
}

internal class FieldRepository(DbSet<Field> fields) : IFieldRepository
{
    private IQueryable<Field> Fields => fields;
    private IQueryable<Field> NoTracking => Fields.AsNoTracking();

    public async ValueTask<Field?> GetFieldByIdAsync(int fieldId, bool tracking = true,
                                                     CancellationToken cancellationToken = default)
    {
        IQueryable<Field> query = Fields;

        if (!tracking)
        {
            query = NoTracking;
        }

        Field? field = await query.Include(f => f.FieldType)
                                  .FirstOrDefaultAsync(f => f.Id == fieldId, cancellationToken);

        return field;
    }

    public async ValueTask<IReadOnlyCollection<Field>> GetAllFields(CancellationToken cancellationToken = default)
    {
        IQueryable<Field> query = NoTracking;

        IReadOnlyCollection<Field> coll = await query.ToListAsync();

        return coll;
    }

    public void AddField(Field field)
    {
        fields.Add(field);
    }

    public void RemoveField(Field field)
    {
        fields.Remove(field);
    }
}
