using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface IOptionResponseRepository
{
    public ValueTask<OptionResponse?> GetOptionResponseByIdAsync(int optionResponseId, bool tracking = true, CancellationToken cancellationToken = default);
    public ValueTask<IReadOnlyCollection<OptionResponse>> GetOptionResponsesAsync(CancellationToken cancellationToken = default);
    public void AddOptionResponse(OptionResponse optionResponse);
    public void RemoveOptionResponse(OptionResponse optionResponse);
}

internal class OptionResponseRepository(DbSet<OptionResponse> optionResponses) : IOptionResponseRepository
{
    private IQueryable<OptionResponse> OptionResponses => optionResponses;
    private IQueryable<OptionResponse> NoTracking => OptionResponses.AsNoTracking();
    
    public async ValueTask<OptionResponse?> GetOptionResponseByIdAsync(int optionResponseId, bool tracking = true,
                                                                 CancellationToken cancellationToken = default)
    {
        IQueryable<OptionResponse> query = OptionResponses;

        if (!tracking)
        {
            query = NoTracking;
        }
        
        OptionResponse? optionResponse = await query.Include(or => or.Option)
                                              .ThenInclude(o => o.SingleChoiceField)
                                              .FirstOrDefaultAsync(or => or.Id == optionResponseId, cancellationToken);

        return optionResponse;
    }

    public async ValueTask<IReadOnlyCollection<OptionResponse>> GetOptionResponsesAsync(CancellationToken cancellationToken = default)
    {
        IQueryable<OptionResponse> query = NoTracking;

        IReadOnlyCollection<OptionResponse> coll = await query.ToListAsync(cancellationToken);

        return coll;
    }

    public void AddOptionResponse(OptionResponse optionResponse)
    {
        optionResponses.Add(optionResponse);
    }

    public void RemoveOptionResponse(OptionResponse optionResponse)
    {
        optionResponses.Remove(optionResponse);
    }
}
