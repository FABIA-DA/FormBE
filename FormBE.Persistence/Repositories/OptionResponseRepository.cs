using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface IOptionResponseRepository
{
    
}

internal class OptionResponseRepository(DbSet<OptionResponse> optionResponses) : IOptionResponseRepository
{
    private IQueryable<OptionResponse> OptionResponses => optionResponses;
}
