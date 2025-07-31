using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface IFieldTypeRepository
{
    
}

internal class FieldTypeRepository(DbSet<FieldType> fieldTypes) : IFieldTypeRepository
{
    private IQueryable<FieldType> FieldTypes => fieldTypes;
}
