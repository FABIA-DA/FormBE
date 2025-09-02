using FluentValidation;
using FluentValidation.Results;
using FormBE.Core.Logic;
using FormBE.Core.Services;
using FormBE.Persistence.Util;
using FormBE.Util;
using Microsoft.AspNetCore.Mvc;

namespace FormBE.Controllers;

[Route("api/field-types")]
public sealed class FieldTypeController(
    ITransactionProvider transaction,
    IFieldTypeService fieldTypeService,
    ILogger<FieldTypeController> logger) : BaseController
{
    [HttpGet]
    [Route("")]
    [ProducesResponseType<FieldTypeListResponse>(StatusCodes.Status200OK)]
    public async ValueTask<ActionResult<FieldTypeListResponse>> GetAllFieldTypes(
        CancellationToken cancellationToken = default)
    {
        var result = await fieldTypeService.GetFieldTypesAsync(cancellationToken);

        return Ok(new FieldTypeListResponse()
        {
            Types = result.Select(FieldTypeExtension.ToDto).ToList()
        });
    }

    [HttpGet]
    [Route("{id:long}")]
    [ProducesResponseType<FieldTypeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<ActionResult<FieldTypeDto>> GetFieldTypeById(
        [FromRoute] long id, CancellationToken cancellationToken = default)
    {
        if (id < 1)
        {
            logger.LogInformation("Tried to get field type with id {fieldTypeId}, but the id must be greater than 0",
                                  id);

            return BadRequest("Id must be greater than 0");
        }

        var result = await fieldTypeService.GetFieldTypeByIdAsync(id, cancellationToken);

        return result.Match<ActionResult<FieldTypeDto>>(fieldType => Ok(fieldType.ToDto()),
                                                        notFound => NotFound());
    }

    [HttpPost]
    [Route("")]
    [ProducesResponseType<FieldTypeDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<ActionResult<FieldTypeDto>> CreateFieldType([FromBody] FieldTypeCreationRequest request,
                                                                       CancellationToken cancellationToken = default)
    {
        FieldTypeCreationRequest.Validator validator = new FieldTypeCreationRequest.Validator();
        ValidationResult valResult = await validator.ValidateAsync(request, cancellationToken);
        if (!valResult.IsValid)
        {
            logger.LogInformation("Tried to create field type with an invalid request: {errors}", valResult.Errors);

            return BadRequest(valResult.Errors);
        }

        await transaction.BeginTransactionAsync(cancellationToken);

        var result = await fieldTypeService.CreateFieldTypeAsync(request.Name, request.Description, request.Regex,
                                                                 cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return CreatedAtAction(nameof(GetFieldTypeById), new { id = result.Id }, result.ToDto());
    }

    [HttpPut]
    [Route("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<IActionResult> UpdateFieldTypeById(long id, [FromBody] FieldTypeUpdateRequest request,
                                                              CancellationToken cancellationToken = default)
    {
        if (id < 1)
        {
            logger.LogInformation("Tried to update field type with id {fieldTypeId}, but the id must be greater than 0",
                                  id);

            return BadRequest("Id must be greater than 0");
        }

        FieldTypeUpdateRequest.Validator validator = new FieldTypeUpdateRequest.Validator();
        ValidationResult valResult = await validator.ValidateAsync(request, cancellationToken);
        if (!valResult.IsValid)
        {
            logger.LogInformation("Tried to update field type with id {fieldTypeId}, but the request is invalid: {errors}",
                                  id, valResult.Errors);

            return BadRequest(valResult.Errors);
        }

        await transaction.BeginTransactionAsync(cancellationToken);

        var result = await fieldTypeService.UpdateFieldTypeAsync(id, request.Name, request.Description, request.Regex,
                                                                 cancellationToken);

        return await result.Match<ValueTask<IActionResult>>(async success =>
                                                            {
                                                                await transaction.CommitAsync(cancellationToken);

                                                                return NoContent();
                                                            },
                                                            notFound => ValueTask
                                                                .FromResult<IActionResult>(NotFound()));
    }

    [HttpDelete]
    [Route("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<IActionResult> DeleteFieldTypeById([FromRoute] long id,
                                                              CancellationToken cancellationToken = default)
    {
        if (id < 1)
        {
            logger.LogInformation("Tried to delete field type with id {fieldTypeId}, but id must be greater than 0",
                                  id);

            return BadRequest("Id must be greater than 0");
        }

        await transaction.BeginTransactionAsync(cancellationToken);

        var result = await fieldTypeService.DeleteFieldTypeAsync(id, cancellationToken);

        return await result.Match<ValueTask<IActionResult>>(async success =>
                                                            {
                                                                await transaction.CommitAsync(cancellationToken);

                                                                return NoContent();
                                                            },
                                                            notFound => ValueTask
                                                                .FromResult<IActionResult>(NotFound()));
    }
}

public sealed class FieldTypeListResponse
{
    public required List<FieldTypeDto> Types { get; set; }
}

public sealed class FieldTypeCreationRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string Regex { get; set; }

    public sealed class Validator : AbstractValidator<FieldTypeCreationRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty();
            When(x => x.Description != null, () => { RuleFor(x => x.Description).NotEmpty(); });
            RuleFor(x => x.Regex).NotNull().NotEmpty();
        }
    }
}

public sealed class FieldTypeUpdateRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string Regex { get; set; }

    public sealed class Validator : AbstractValidator<FieldTypeUpdateRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty();
            When(x => x.Description != null, () => { RuleFor(x => x.Description).NotEmpty(); });
            RuleFor(x => x.Regex).NotNull().NotEmpty();
        }
    }
}
