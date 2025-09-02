using FluentValidation;
using FluentValidation.Results;
using FormBE.Core.Logic;
using FormBE.Core.Services;
using FormBE.Persistence.Util;
using FormBE.Util;
using Microsoft.AspNetCore.Mvc;

namespace FormBE.Controllers;

[Route("api/field-responses")]
public sealed class FieldResponseController(
    ITransactionProvider transaction,
    IFieldResponseService fieldResponseService,
    ILogger<FieldResponseController> logger) : BaseController
{
    [HttpGet]
    [Route("")]
    [ProducesResponseType<FieldResponseListResponse>(StatusCodes.Status200OK)]
    public async ValueTask<ActionResult<FieldResponseListResponse>> GetAllFieldResponses(
        CancellationToken cancellationToken = default)
    {
        var list = await fieldResponseService.GetAllFieldResponsesAsync(cancellationToken);

        return Ok(new FieldResponseListResponse()
        {
            Responses = list.Select(FieldResponseExtension.ToDto).ToList()
        });
    }

    [HttpGet]
    [Route("{id:long}")]
    [ProducesResponseType<FieldResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<ActionResult<FieldResponseDto>> GetFieldResponseById(
        [FromRoute] long id, CancellationToken cancellationToken = default)
    {
        if (id < 1)
        {
            logger.LogInformation("Tried to get field response with id {fieldResponseId}, but the id must be greater than 0",
                                  id);

            return BadRequest("Id must be greater than 0");
        }

        var result = await fieldResponseService.GetFieldResponseByIdAsync(id, cancellationToken);

        return result.Match<ActionResult<FieldResponseDto>>(fieldResponse => Ok(fieldResponse.ToDto()),
                                                            notFound => NotFound());
    }

    [HttpPost]
    [Route("")]
    [ProducesResponseType<FieldResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<ActionResult<FieldResponseDto>> CreateFieldResponse(
        [FromBody] FieldResponseCreationRequest request, CancellationToken cancellationToken = default)
    {
        FieldResponseCreationRequest.Validator validator = new FieldResponseCreationRequest.Validator();
        ValidationResult valResult = await validator.ValidateAsync(request, cancellationToken);
        if (!valResult.IsValid)
        {
            logger.LogInformation("Tried to create field response, but the request is invalid: {errors}",
                                  valResult.Errors);

            return BadRequest(valResult.Errors);
        }

        await transaction.BeginTransactionAsync(cancellationToken);

        var result = await fieldResponseService.CreateFieldResponseAsync(request.FieldId, request.TelephoneNumber,
                                                                         request.Value, cancellationToken);

        return await result.Match<ValueTask<ActionResult<FieldResponseDto>>>(async success =>
                                                                             {
                                                                                 await transaction
                                                                                     .CommitAsync(cancellationToken);

                                                                                 return
                                                                                     CreatedAtAction(nameof(
                                                                                          GetFieldResponseById),
                                                                                      new { Id = success.Value.Id },
                                                                                      success.Value.ToDto());
                                                                             },
                                                                             fieldNotFound =>
                                                                                 ValueTask
                                                                                     .FromResult<
                                                                                         ActionResult<
                                                                                             FieldResponseDto>>(BadRequest("Field with the supplied id is not found")));
    }

    [HttpDelete]
    [Route("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<IActionResult> DeleteFieldResponseById([FromRoute] long id,
                                                                  CancellationToken cancellationToken = default)
    {
        if (id < 1)
        {
            logger.LogInformation("Tried to delete field response with id {fieldResponseId}, but the id must be greater than 0",
                                  id);

            return BadRequest("Id must be greater than 0");
        }

        await transaction.BeginTransactionAsync(cancellationToken);

        var result = await fieldResponseService.DeleteFieldResponseAsync(id, cancellationToken);

        return await result.Match<ValueTask<IActionResult>>(async success =>
                                                            {
                                                                await transaction.CommitAsync(cancellationToken);

                                                                return NoContent();
                                                            },
                                                            notFound => ValueTask
                                                                .FromResult<IActionResult>(NotFound()));
    }
}

public sealed class FieldResponseListResponse
{
    public required List<FieldResponseDto> Responses { get; set; }
}

public sealed class FieldResponseCreationRequest
{
    public long FieldId { get; set; }
    public required string TelephoneNumber { get; set; }
    public required string Value { get; set; }

    public sealed class Validator : AbstractValidator<FieldResponseCreationRequest>
    {
        public Validator()
        {
            RuleFor(x => x.FieldId).NotNull().NotEmpty().GreaterThan(0);
            RuleFor(x => x.TelephoneNumber).NotNull().NotEmpty();
            RuleFor(x => x.Value).NotNull().NotEmpty();
        }
    }
}
