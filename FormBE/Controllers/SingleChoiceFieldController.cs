using FluentValidation;
using FluentValidation.Results;
using FormBE.Core.Logic;
using FormBE.Core.Services;
using FormBE.Persistence.Util;
using FormBE.Util;
using Microsoft.AspNetCore.Mvc;

namespace FormBE.Controllers;

[ApiController]
[Route("api/singleChoiceFields")]
public sealed class SingleChoiceFieldController(
    ITransactionProvider transaction,
    ISingleChoiceFieldService singleChoiceFieldService,
    ILogger<SingleChoiceFieldController> logger) : BaseController
{
    [HttpGet]
    [Route("")]
    [ProducesResponseType<SingleChoiceFieldListResponse>(StatusCodes.Status200OK)]
    public async ValueTask<ActionResult<SingleChoiceFieldListResponse>> GetAllSingleChoiceFields(
        CancellationToken cancellationToken = default)
    {
        var list = await singleChoiceFieldService.GetSingleChoiceFieldsAsync(cancellationToken);

        return Ok(new SingleChoiceFieldListResponse()
        {
            Fields = list.Select(SingleChoiceFieldExtension.ToDto).ToList()
        });
    }

    [HttpGet]
    [Route("{id:long}")]
    [ProducesResponseType<SingleChoiceFieldDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<ActionResult<SingleChoiceFieldDto>> GetSingleChoiceFieldById(
        [FromRoute] long id, CancellationToken cancellationToken = default)
    {
        if (id < 1)
        {
            logger.LogInformation("Tried to get single choice field with id {singleChoiceFieldId}, but the id must be greater than 0",
                                  id);

            return BadRequest("Id must be greater than 0");
        }

        var result = await singleChoiceFieldService.GetSingleChoiceFieldByIdAsync(id, cancellationToken);

        return result.Match<ActionResult<SingleChoiceFieldDto>>(singleChoiceField => Ok(singleChoiceField.ToDto()),
                                                                notFound => NotFound());
    }

    [HttpPost]
    [Route("")]
    [ProducesResponseType<SingleChoiceFieldDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<ActionResult<SingleChoiceFieldDto>> CreateSingleChoiceField(
        [FromBody] SingleChoiceFieldCreationRequest request, CancellationToken cancellationToken = default)
    {
        SingleChoiceFieldCreationRequest.Validator validator = new SingleChoiceFieldCreationRequest.Validator();
        ValidationResult valResult = await validator.ValidateAsync(request, cancellationToken);
        if (!valResult.IsValid)
        {
            logger.LogInformation("Tried to create new single choice field with name {singleChoiceFieldName}, but the request is invalid: {errors}",
                                  request.Name, valResult.Errors);

            return BadRequest(valResult.Errors);
        }

        await transaction.BeginTransactionAsync(cancellationToken);

        var result = await singleChoiceFieldService.CreateSingleChoiceFieldAsync(request.Name,
             request.Options.Select(kv => (kv.Key, kv.Value)).ToList(), cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return CreatedAtAction(nameof(GetSingleChoiceFieldById), new { id = result.Id }, result.ToDto());
    }
}

public sealed class SingleChoiceFieldListResponse
{
    public required List<SingleChoiceFieldDto> Fields { get; set; }
}

public sealed class SingleChoiceFieldCreationRequest
{
    public required string Name { get; set; }
    public required Dictionary<string, List<long>> Options { get; set; }

    public sealed class Validator : AbstractValidator<SingleChoiceFieldCreationRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty();
            RuleFor(x => x.Options).NotNull();
            RuleFor(x => x.Options.Keys).NotEmpty();
        }
    }
}

public sealed class SingleChoiceFieldUpdateRequest
{
    public required string Name { get; set; }
    public required Dictionary<string, List<long>> OldOptions { get; set; }
    public required Dictionary<string, List<long>> NewOptions { get; set; }

    public sealed class Validator : AbstractValidator<SingleChoiceFieldUpdateRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty();
            RuleFor(x => x.OldOptions).NotNull();
            RuleFor(x => x.OldOptions.Keys).NotEmpty();
            RuleFor(x => x.NewOptions).NotNull();
            RuleFor(x => x.NewOptions.Keys).NotEmpty();
        }
    }
}
