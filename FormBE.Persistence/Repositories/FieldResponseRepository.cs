using FormBE.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace FormBE.Persistence.Repositories;

public interface IFieldResponseRepository
{
    public ValueTask<FieldResponse?> GetFieldResponseByIdAsync(int fieldResponseId, bool tracking = true, CancellationToken cancellationToken = default);
    public ValueTask<IReadOnlyCollection<FieldResponse>> GetAllFieldResponses(CancellationToken cancellationToken = default);
    public void AddFieldResponse(FieldResponse fieldResponse);
    public void RemoveFieldResponse(FieldResponse fieldResponse);
}

internal class FieldResponseRepository(DbSet<FieldResponse> fieldResponses) : IFieldResponseRepository
{
    private IQueryable<FieldResponse> FieldResponses => fieldResponses;
    private IQueryable<FieldResponse> NoTracking => FieldResponses.AsNoTracking();
    
    public async ValueTask<FieldResponse?> GetFieldResponseByIdAsync(int fieldResponseId, bool tracking = true,
                                                               CancellationToken cancellationToken = default)
    {
        IQueryable<FieldResponse> query = FieldResponses;

        if (!tracking)
        {
            query = NoTracking;
        }
        
        FieldResponse? response = await query.FirstOrDefaultAsync(r => r.Id == fieldResponseId, cancellationToken);
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
