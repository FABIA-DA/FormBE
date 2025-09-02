using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface IOptionResponseRepository
{
    /// <summary>
    /// Get an option response by its id.
    /// </summary>
    /// <param name="optionResponseId">The id of the option response.</param>
    /// <param name="tracking">If EF Core should track the entity.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The specified option response.</returns>
    public ValueTask<OptionResponse?> GetOptionResponseByIdAsync(long optionResponseId, bool tracking = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a <see cref="Option"/> by its id.
    /// </summary>
    /// <param name="optionId">The id of the option to add.</param>
    /// <param name="tracking">If EF Core should track the entity.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The <see cref="Option"/> or null if not found.</returns>
    public ValueTask<Option?> GetOptionByIdAsync(long optionId, bool tracking = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all option responses.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>All option responses without tracking.</returns>
    public ValueTask<IReadOnlyCollection<OptionResponse>> GetOptionResponsesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Add an option response to the tracking of EF Core.
    /// </summary>
    /// <param name="optionResponse">The option response to add.</param>
    public void AddOptionResponse(OptionResponse optionResponse);
    
    /// <summary>
    /// Adds an option response to the tracking with the <see cref="EntityState.Deleted"/> state.
    /// </summary>
    /// <param name="optionResponse">The option response to delete.</param>
    public void RemoveOptionResponse(OptionResponse optionResponse);
}

internal class OptionResponseRepository(DbSet<OptionResponse> optionResponses, DbSet<Option> options) : IOptionResponseRepository
{
    private IQueryable<OptionResponse> OptionResponses => optionResponses;
    private IQueryable<OptionResponse> NoTracking => optionResponses.AsNoTracking();
    
    public async ValueTask<OptionResponse?> GetOptionResponseByIdAsync(long optionResponseId, bool tracking = true,
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

    public async ValueTask<Option?> GetOptionByIdAsync(long optionId, bool tracking = true, CancellationToken cancellationToken = default)
    {
        IQueryable<Option> query = options;

        if (!tracking)
        {
            query = options.AsNoTracking();
        }
        
        Option? option = await query.FirstOrDefaultAsync(o => o.Id == optionId, cancellationToken);
        return option;
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
