using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface IFieldTypeRepository
{
    public ValueTask<FieldType?> GetFieldTypeByIdAsync(int fieldTypeId, bool tracking = true, CancellationToken cancellationToken = default);
    public ValueTask<IReadOnlyCollection<FieldType>> GetAllFieldTypes(CancellationToken cancellationToken = default);
    public void AddFieldType(FieldType fieldType);
    public void RemoveFieldType(FieldType fieldType);
}

internal class FieldTypeRepository(DbSet<FieldType> fieldTypes) : IFieldTypeRepository
{
    private IQueryable<FieldType> FieldTypes => fieldTypes;
    private IQueryable<FieldType> NoTracking => FieldTypes.AsNoTracking();
    
    public async ValueTask<FieldType?> GetFieldTypeByIdAsync(int fieldTypeId, bool tracking = true, CancellationToken cancellationToken = default)
    {
        IQueryable<FieldType> query = FieldTypes;

        if (!tracking)
        {
            query = NoTracking;
        }
        
        FieldType? fieldType = await query.FirstOrDefaultAsync(t => t.Id == fieldTypeId, cancellationToken);
        return fieldType;
    }

    public async ValueTask<IReadOnlyCollection<FieldType>> GetAllFieldTypes(CancellationToken cancellationToken = default)
    {
        IQueryable<FieldType> query = NoTracking;

        IReadOnlyCollection<FieldType> coll = await query.ToListAsync(cancellationToken);
        
        return coll;
    }

    public void AddFieldType(FieldType fieldType)
    {
        fieldTypes.Add(fieldType);
    }

    public void RemoveFieldType(FieldType fieldType)
    {
        fieldTypes.Remove(fieldType);
    }
}
