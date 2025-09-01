using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface IFieldResponseRepository
{
    /// <summary>
    /// Get a field response by its id.
    /// </summary>
    /// <param name="fieldResponseId">The id of the field response to get.</param>
    /// <param name="tracking">If EF Core should track the entity.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The field response or null if not found.</returns>
    public ValueTask<FieldResponse?> GetFieldResponseByIdAsync(long fieldResponseId, bool tracking = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all field responses without tracking.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>All field responses without tracking.</returns>
    public ValueTask<IReadOnlyCollection<FieldResponse>> GetAllFieldResponses(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Begin tracking for a field response.
    /// </summary>
    /// <param name="fieldResponse">The field response to track.</param>
    public void AddFieldResponse(FieldResponse fieldResponse);
    
    /// <summary>
    /// Begin to track a field response with the <see cref="EntityState.Deleted"/> state.
    /// </summary>
    /// <param name="fieldResponse">The field response to delete.</param>
    public void RemoveFieldResponse(FieldResponse fieldResponse);
}

internal class FieldResponseRepository(DbSet<FieldResponse> fieldResponses) : IFieldResponseRepository
{
    private IQueryable<FieldResponse> FieldResponses => fieldResponses;
    private IQueryable<FieldResponse> NoTracking => FieldResponses.AsNoTracking();
    
    public async ValueTask<FieldResponse?> GetFieldResponseByIdAsync(long fieldResponseId, bool tracking = true,
                                                                     CancellationToken cancellationToken = default)
    {
        IQueryable<FieldResponse> query = FieldResponses;

        if (!tracking)
        {
            query = NoTracking;
        }
        
        FieldResponse? response = await query.Include(fr => fr.Field)
                                             .FirstOrDefaultAsync(r => r.Id == fieldResponseId, cancellationToken);
        return response;
    }

    public async ValueTask<IReadOnlyCollection<FieldResponse>> GetAllFieldResponses(CancellationToken cancellationToken = default)
    {
        IQueryable<FieldResponse> query = NoTracking;
        
        IReadOnlyCollection<FieldResponse> responses = await query.ToListAsync(cancellationToken);
        
        return responses;
    }

    public void AddFieldResponse(FieldResponse fieldResponse)
    {
        fieldResponses.Add(fieldResponse);
    }

    public void RemoveFieldResponse(FieldResponse fieldResponse)
    {
        fieldResponses.Remove(fieldResponse);
    }
}
