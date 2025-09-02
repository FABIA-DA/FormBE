using FluentValidation;
using FluentValidation.Results;
using FormBE.Core.Logic;
using FormBE.Core.Services;
using FormBE.Persistence.Util;
using FormBE.Util;
using Microsoft.AspNetCore.Mvc;

namespace FormBE.Controllers;

[Route("api/option-responses")]
public sealed class OptionResponseController(
    ITransactionProvider transaction,
    IOptionResponseService optionResponseService,
    ILogger<OptionResponseController> logger) : BaseController
{
    private void Dummy()
    {
        var a = transaction;
        var b = logger;
    }

    [HttpGet]
    [Route("")]
    [ProducesResponseType<OptionResponseListResponse>(StatusCodes.Status200OK)]
    public async ValueTask<ActionResult<OptionResponseListResponse>> GetAllOptionResponses(
        CancellationToken cancellationToken = default)
    {
        var list = await optionResponseService.GetOptionResponsesAsync(cancellationToken);

        return Ok(new OptionResponseListResponse()
        {
            Responses = list.Select(OptionResponseExtension.ToDto).ToList()
        });
    }

    [HttpGet]
    [Route("{id:long}")]
    [ProducesResponseType<OptionResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<ActionResult<OptionResponseDto>> GetOptionResponseById(
        [FromRoute] long id, CancellationToken cancellationToken = default)
    {
        if (id < 1)
        {
            logger.LogInformation("Tried to get option response with id {optionResponseId}, but the id must be greater than 0",
                                  id);

            return BadRequest("Id must be greater than 0");
        }

        var result = await optionResponseService.GetOptionResponseByIdAsync(id, cancellationToken);

        return result.Match<ActionResult>(optionResponse => Ok(optionResponse.ToDto()),
                                          notFound => NotFound());
    }

    [HttpPost]
    [Route("")]
    [ProducesResponseType<OptionResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<ActionResult<OptionResponseDto>> CreateOptionResponse(
        [FromBody] OptionResponseCreationRequest request, CancellationToken cancellationToken = default)
    {
        OptionResponseCreationRequest.Validator validator = new OptionResponseCreationRequest.Validator();
        ValidationResult valResult = await validator.ValidateAsync(request, cancellationToken);
        if (!valResult.IsValid)
        {
            logger.LogInformation("Tried to create option response with invalid request: {errors}", valResult.Errors);

            return BadRequest(valResult.Errors);
        }

        await transaction.BeginTransactionAsync(cancellationToken);

        var result = await optionResponseService.CreateOptionResponseAsync(request.OptionId, request.TelephoneNumber,
                                                                           cancellationToken);

        return await result.Match<ValueTask<ActionResult<OptionResponseDto>>>(async success =>
                                                                              {
                                                                                  await transaction
                                                                                      .CommitAsync(cancellationToken);

                                                                                  return
                                                                                      CreatedAtAction(nameof(
                                                                                           GetOptionResponseById),
                                                                                       new
                                                                                       {
                                                                                           Id = success.Value.Id
                                                                                       }, success.Value.ToDto());
                                                                              },
                                                                              optionNotFound =>
                                                                                  ValueTask
                                                                                      .FromResult<
                                                                                          ActionResult<
                                                                                              OptionResponseDto>>(BadRequest("Option with supplied id not found")));
    }

    [HttpDelete]
    [Route("{id:long}")]
    public async ValueTask<IActionResult> DeleteOptionResponseById([FromRoute] long id,
                                                                   CancellationToken cancellationToken = default)
    {
        if (id < 1)
        {
            logger.LogInformation("Tried to delete option response with id {optionResponseId}, but the id must be greater than 0",
                                  id);

            return BadRequest("Id must be greater than 0");
        }

        await transaction.BeginTransactionAsync(cancellationToken);

        var result = await optionResponseService.DeleteOptionResponseAsync(id, cancellationToken);

        return await result.Match<ValueTask<IActionResult>>(async success =>
        {
            await transaction.CommitAsync(cancellationToken);

            return NoContent();
        }, notFound => ValueTask.FromResult<IActionResult>(NotFound()));
    }
}

public sealed class OptionResponseListResponse
{
    public required List<OptionResponseDto> Responses { get; set; }
}

public sealed class OptionResponseCreationRequest
{
    public required long OptionId { get; set; }
    public required string TelephoneNumber { get; set; }

    public sealed class Validator : AbstractValidator<OptionResponseCreationRequest>
    {
        public Validator()
        {
            RuleFor(x => x.OptionId).NotNull().GreaterThan(0);
            RuleFor(x => x.TelephoneNumber).NotNull().NotEmpty();
        }
    }
}
