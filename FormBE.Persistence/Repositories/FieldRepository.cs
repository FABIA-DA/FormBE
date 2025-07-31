using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface IFieldRepository
{
    
}

internal class FieldRepository(DbSet<Field> fields) : IFieldRepository
{
    private IQueryable<Field> Fields => fields;
}
