using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface IFormRepository
{
    
}

internal class FormRepository(DbSet<Form> forms) : IFormRepository
{
    private IQueryable<Form> Forms => forms;
}
