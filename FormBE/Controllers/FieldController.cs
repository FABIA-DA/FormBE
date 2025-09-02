using FluentValidation;
using FluentValidation.Results;
using FormBE.Core.Logic;
using FormBE.Core.Services;
using FormBE.Persistence.Util;
using FormBE.Util;
using Microsoft.AspNetCore.Mvc;

namespace FormBE.Controllers;

[Route("api/fields")]
public sealed class FieldController(
    ITransactionProvider transaction,
    IFieldService fieldService,
    ILogger<FieldController> logger) : BaseController
{
    [HttpGet]
    [Route("")]
    [ProducesResponseType<FieldListResponse>(StatusCodes.Status200OK)]
    public async ValueTask<ActionResult<FieldListResponse>> GetAllFields(CancellationToken cancellationToken = default)
    {
        var list = await fieldService.GetFieldsAsync(cancellationToken);

        return Ok(new FieldListResponse()
        {
            Fields = list.Select(FieldExtension.ToDto).ToList()
        });
    }

    [HttpGet]
    [Route("{id:long}")]
    [ProducesResponseType<FieldDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<ActionResult<FieldDto>> GetFieldById([FromRoute] long id,
                                                                CancellationToken cancellationToken = default)
    {
        if (id < 1)
        {
            logger.LogInformation("Tried to get field with id {fieldId}, but the id must be greater than 0", id);

            return BadRequest("Id must be greater than 0");
        }

        var result = await fieldService.GetFieldByIdAsync(id, cancellationToken);

        return result.Match<ActionResult<FieldDto>>(field => Ok(field.ToDto()),
                                                    notFound => NotFound());
    }

    [HttpPost]
    [Route("")]
    [ProducesResponseType<FieldDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<ActionResult<FieldDto>> CreateField([FromBody] FieldCreationRequest request,
                                                               CancellationToken cancellationToken = default)
    {
        FieldCreationRequest.Validator validator = new FieldCreationRequest.Validator();
        ValidationResult valResult = await validator.ValidateAsync(request, cancellationToken);
        if (!valResult.IsValid)
        {
            logger.LogInformation("Tried to create field with an invalid request: {errors}", valResult.Errors);

            return BadRequest(valResult.Errors);
        }

        await transaction.BeginTransactionAsync(cancellationToken);

        var result = await fieldService.CreateFieldAsync(request.FieldTypeId, request.Name, request.Description,
                                                         request.IsOptional, cancellationToken);

        return await result.Match<ValueTask<ActionResult<FieldDto>>>(async success =>
                                                                     {
                                                                         await transaction
                                                                             .CommitAsync(cancellationToken);

                                                                         return CreatedAtAction(nameof(GetFieldById),
                                                                          new { Id = success.Value.Id },
                                                                          success.Value.ToDto());
                                                                     },
                                                                     fieldTypeNotFound =>
                                                                         ValueTask
                                                                             .FromResult<
                                                                                 ActionResult<
                                                                                     FieldDto>>(BadRequest("Field type with supplied id not found")));
    }

    [HttpPut]
    [Route("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> UpdateFieldById([FromRoute] long id, [FromBody] FieldUpdateRequest request,
                                                          CancellationToken cancellationToken = default)
    {
        if (id < 1)
        {
            logger.LogInformation("Tried to update field with id {fieldId}, but the id must be greater than 0", id);

            return BadRequest("Id must be greater than 0");
        }

        FieldUpdateRequest.Validator validator = new FieldUpdateRequest.Validator();
        ValidationResult valResult = await validator.ValidateAsync(request, cancellationToken);
        if (!valResult.IsValid)
        {
            logger.LogInformation("Tried to update field with an invalid request: {errors}", valResult);

            return BadRequest(valResult.Errors);
        }

        await transaction.BeginTransactionAsync(cancellationToken);

        var result = await fieldService.UpdateFieldAsync(id, request.FieldTypeId, request.Name, request.Description,
                                                         request.IsOptional, cancellationToken);

        return await result.Match<ValueTask<IActionResult>>(async success =>
                                                            {
                                                                await transaction.CommitAsync(cancellationToken);

                                                                return NoContent();
                                                            },
                                                            notFound => ValueTask.FromResult<IActionResult>(NotFound()),
                                                            fieldTypeNotFound =>
                                                                ValueTask
                                                                    .FromResult<
                                                                        IActionResult>(BadRequest("Field type with supplied id not found")));
    }

    [HttpDelete]
    [Route("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<IActionResult> DeleteFieldById([FromRoute] long id,
                                                          CancellationToken cancellationToken = default)
    {
        if (id < 1)
        {
            logger.LogInformation("Tried to delete field with id {fieldId}, but the id must be greater than 0", id);

            return BadRequest("Id must be greater than 0");
        }

        await transaction.BeginTransactionAsync(cancellationToken);

        var result = await fieldService.DeleteFieldAsync(id, cancellationToken);

        return await result.Match<ValueTask<IActionResult>>(async success =>
                                                            {
                                                                await transaction.CommitAsync(cancellationToken);

                                                                return NoContent();
                                                            },
                                                            notFound => ValueTask
                                                                .FromResult<IActionResult>(NotFound()));
    }
}

public sealed class FieldListResponse
{
    public required List<FieldDto> Fields { get; set; }
}

public sealed class FieldCreationRequest
{
    public long FieldTypeId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsOptional { get; set; }

    public sealed class Validator : AbstractValidator<FieldCreationRequest>
    {
        public Validator()
        {
            RuleFor(x => x.FieldTypeId).NotNull().NotEmpty();
            RuleFor(x => x.Name).NotNull().NotEmpty();
            When(x => x.Description != null, () => { RuleFor(x => x.Description).NotEmpty(); });
            RuleFor(x => x.IsOptional).NotNull();
        }
    }
}

public sealed class FieldUpdateRequest
{
    public long FieldTypeId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsOptional { get; set; }

    public sealed class Validator : AbstractValidator<FieldUpdateRequest>
    {
        public Validator()
        {
            RuleFor(x => x.FieldTypeId).NotNull().NotEmpty();
            RuleFor(x => x.Name).NotNull().NotEmpty();
            When(x => x.Description != null, () => { RuleFor(x => x.Description).NotEmpty(); });
            RuleFor(x => x.IsOptional).NotNull();
        }
    }
}
