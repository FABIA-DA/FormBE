using FormBE.Persistence.Model;
using FormBE.Persistence.Repositories;
using FormBE.Persistence.Util;
using OneOf.Types;
using OneOf;

namespace FormBE.Core.Services;

public interface IFieldResponseService
{
    /// <summary>
    /// Get all <see cref="FieldResponse"/>s.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An <see cref="IReadOnlyCollection{FieldResponse}"/> of <see cref="FieldResponse"/>s.</returns>
    public ValueTask<IReadOnlyCollection<FieldResponse>> GetAllFieldResponsesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a specific <see cref="FieldResponse"/> with its id.
    /// </summary>
    /// <param name="fieldResponseId">The id of the field response to get.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The <see cref="FieldResponse"/> or a <see cref="NotFound"/>.</returns>
    public ValueTask<OneOf<FieldResponse, NotFound>> GetFieldResponseByIdAsync(long fieldResponseId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Create a <see cref="FieldResponse"/> with its initial values.
    /// </summary>
    /// <param name="fieldId">The id of the field the response is for.</param>
    /// <param name="telephoneNumber">The telephone number of the user.</param>
    /// <param name="value">The submitted value.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Success"/> with the newly created <see cref="FieldResponse"/> or a <see cref="FieldNotFound"/> if the field was not found.</returns>
    public ValueTask<OneOf<Success<FieldResponse>, FieldNotFound>> CreateFieldResponseAsync(long fieldId, string telephoneNumber, string value, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a <see cref="FieldResponse"/> by its id.
    /// </summary>
    /// <param name="fieldResponseId">The id of the field response.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>Either a <see cref="Success"/> or a <see cref="NotFound"/> if the field response was not found.</returns>
    public ValueTask<OneOf<Success, NotFound>> DeleteFieldResponseAsync(long fieldResponseId, CancellationToken cancellationToken = default);

    public struct FieldNotFound;
}

internal class FieldResponseService(
    IUnitOfWork uow,
    IClock clock,
    ILogger<FieldResponseService> logger) : IFieldResponseService
{
    private IFieldResponseRepository FieldResponseRepository => uow.FieldResponseRepository;
    private IFieldRepository FieldRepository => uow.FieldRepository;
    
    public async ValueTask<IReadOnlyCollection<FieldResponse>> GetAllFieldResponsesAsync(CancellationToken cancellationToken = default) => await FieldResponseRepository.GetAllFieldResponses(cancellationToken);

    public async ValueTask<OneOf<FieldResponse, NotFound>> GetFieldResponseByIdAsync(long fieldResponseId, CancellationToken cancellationToken = default)
    {
        FieldResponse? fieldResponse = await FieldResponseRepository.GetFieldResponseByIdAsync(fieldResponseId, false, cancellationToken);

        if (fieldResponse == null)
        {
            logger.LogInformation("Tried to get field response with id {FieldResponseId}, but was not found", fieldResponseId);

            return new NotFound();
        }

        return fieldResponse;
    }

    public async ValueTask<OneOf<Success<FieldResponse>, IFieldResponseService.FieldNotFound>> CreateFieldResponseAsync(long fieldId, string telephoneNumber, string value,
                                                                                                                  CancellationToken cancellationToken = default)
    {
        Field? field = await FieldRepository.GetFieldByIdAsync(fieldId, true, cancellationToken);

        if (field == null)
        {
            logger.LogInformation("Tried to create field response of field with id {FieldId}, but field was not found", fieldId);

            return new IFieldResponseService.FieldNotFound();
        }
        
        FieldResponse response = new FieldResponse()
        {
            TelephoneNumber = telephoneNumber,
            Value = value,
            Field = field,
            SubmittedAt = clock.GetCurrentInstant()
        };
        
        FieldResponseRepository.AddFieldResponse(response);
        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created field response with id {FieldResponseId}", response.Id);
        
        return new Success<FieldResponse>(response);
    }

    public async ValueTask<OneOf<Success, NotFound>> DeleteFieldResponseAsync(long fieldResponseId, CancellationToken cancellationToken = default)
    {
        FieldResponse? response = await FieldResponseRepository.GetFieldResponseByIdAsync(fieldResponseId, true, cancellationToken);

        if (response == null)
        {
            logger.LogInformation("Tried to delete field response with id {FieldResponseId}, but was not found", fieldResponseId);

            return new NotFound();
        }
        
        FieldResponseRepository.RemoveFieldResponse(response);
        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Deleted field response with id {FieldResponseId}", response.Id);

        return new Success();
    }
}
