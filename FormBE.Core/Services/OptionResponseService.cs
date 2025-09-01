using FormBE.Persistence.Model;
using FormBE.Persistence.Repositories;
using FormBE.Persistence.Util;
using OneOf.Types;
using OneOf;

namespace FormBE.Core.Services;

public interface IOptionResponseService
{
    /// <summary>
    /// Get all <see cref="OptionResponse"/>s.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="IReadOnlyCollection{OptionResponse}"/> of <see cref="OptionResponse"/>s.</returns>
    public ValueTask<IReadOnlyCollection<OptionResponse>> GetOptionResponsesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a <see cref="OptionResponse"/> by its id.
    /// </summary>
    /// <param name="optionResponseId">The id of the option response to get.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The <see cref="OptionResponse"/> or a <see cref="NotFound"/>.</returns>
    public ValueTask<OneOf<OptionResponse, NotFound>> GetOptionResponseByIdAsync(
        long optionResponseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a <see cref="OptionResponse"/>.
    /// </summary>
    /// <param name="optionId">The id of the option to append the response to.</param>
    /// <param name="telephoneNumber">The telephone number of the user who entered the information.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Success"/> with the newly created <see cref="OptionResponse"/> or a <see cref="OptionNotFound"/>.</returns>
    public ValueTask<OneOf<Success<OptionResponse>, OptionNotFound>> CreateOptionResponseAsync(
        long optionId, string telephoneNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a <see cref="OptionResponse"/> by its id.
    /// </summary>
    /// <param name="optionResponseId">The id of the response to delete.</param>
    /// <param name="cancellationToken">A <see cref="OptionResponse"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Success"/> or a <see cref="NotFound"/>.</returns>
    public ValueTask<OneOf<Success, NotFound>> DeleteOptionResponseAsync(
        long optionResponseId, CancellationToken cancellationToken = default);

    public struct OptionNotFound;
}

internal class OptionResponseService(
    IUnitOfWork uow,
    IClock clock,
    ILogger<OptionResponseService> logger) : IOptionResponseService
{
    private IOptionResponseRepository OptionResponseRepository => uow.OptionResponseRepository;
    
    public async ValueTask<IReadOnlyCollection<OptionResponse>>
        GetOptionResponsesAsync(CancellationToken cancellationToken = default) =>
        await OptionResponseRepository.GetOptionResponsesAsync(cancellationToken);

    public async ValueTask<OneOf<OptionResponse, NotFound>> GetOptionResponseByIdAsync(
        long optionResponseId, CancellationToken cancellationToken = default)
    {
        OptionResponse? optionResponse = await OptionResponseRepository.GetOptionResponseByIdAsync(optionResponseId, false, cancellationToken);

        if (optionResponse == null)
        {
            logger.LogInformation("Tried to get option response with id {OptionResponseId}, but was not found", optionResponseId);

            return new NotFound();
        }

        return optionResponse;
    }

    public async ValueTask<OneOf<Success<OptionResponse>, IOptionResponseService.OptionNotFound>> CreateOptionResponseAsync(
        long optionId, string telephoneNumber,
        CancellationToken cancellationToken = default)
    {
        Option? option = await OptionResponseRepository.GetOptionByIdAsync(optionId, true, cancellationToken);

        if (option == null)
        {
            logger.LogInformation("Tried to create option response of option with id {OptionId}, but the option was not found", optionId);

            return new IOptionResponseService.OptionNotFound();
        }

        OptionResponse response = new OptionResponse()
        {
            Option = option,
            TelephoneNumber = telephoneNumber,
            SubmittedAt = clock.GetCurrentInstant()
        };
        
        OptionResponseRepository.AddOptionResponse(response);
        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created new option response with id {OptionResponseId}", response.Id);
        
        return new Success<OptionResponse>(response);
    }

    public async ValueTask<OneOf<Success, NotFound>> DeleteOptionResponseAsync(
        long optionResponseId, CancellationToken cancellationToken = default)
    {
        OptionResponse? response = await OptionResponseRepository.GetOptionResponseByIdAsync(optionResponseId, true, cancellationToken);

        if (response == null)
        {
            logger.LogInformation("Tried to delete option response with id {OptionResponseId}, but was not found", optionResponseId);

            return new NotFound();
        }
        
        OptionResponseRepository.RemoveOptionResponse(response);
        await  uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Deleted option response with id {OptionResponseId}", optionResponseId);

        return new Success();
    }
}
