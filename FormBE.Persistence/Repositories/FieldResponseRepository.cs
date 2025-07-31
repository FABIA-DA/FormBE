using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface IFieldResponseRepository
{
    
}

internal class FieldResponseRepository(DbSet<FieldResponse> fieldResponses) : IFieldResponseRepository
{
    private IQueryable<FieldResponse> FieldResponses => fieldResponses;
}
