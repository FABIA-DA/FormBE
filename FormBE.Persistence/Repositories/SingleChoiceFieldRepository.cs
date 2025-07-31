using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface ISingleChoiceFieldRepository
{
    
}

internal class SingleChoiceFieldRepository(DbSet<SingleChoiceField> singleChoiceFields) : ISingleChoiceFieldRepository
{
    private  IQueryable<SingleChoiceField> SingleChoiceFields => singleChoiceFields;
}
