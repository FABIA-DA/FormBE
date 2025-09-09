using FluentValidation;
using FluentValidation.Results;
using FormBE.Core.Logic;
using FormBE.Core.Services;
using FormBE.Persistence.Model;
using FormBE.Persistence.Util;
using FormBE.Util;
using Microsoft.AspNetCore.Mvc;

namespace FormBE.Controllers;

[Route("api/field-groups")]
public sealed class FieldGroupController(
    ITransactionProvider transaction,
    IFieldGroupService fieldGroupService,
    ILogger<FieldGroupController> logger) : BaseController
{
    [HttpGet]
    [Route("")]
    [ProducesResponseType<FieldGroupListResponse>(StatusCodes.Status200OK)]
    public async ValueTask<ActionResult<FieldGroupListResponse>> GetFieldGroupList(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<(long Id, string Name, int SingleChoiceFieldCount, int FieldCount)> list
            = await fieldGroupService.GetFieldGroupsAsync(cancellationToken);

        return Ok(new FieldGroupListResponse()
        {
            FieldGroups = list.Select(FieldGroupExtension.ToListDto).ToList()
        });
    }

    [HttpGet]
    [Route("{id:long}")]
    [ProducesResponseType<FieldGroupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<ActionResult<FieldGroupDto>> GetFieldGroupById(
        [FromRoute] long id, CancellationToken cancellationToken = default)
    {
        if (id < 1)
        {
            logger.LogInformation("Tried to get field group with id {fieldGroupId}, but Id must be greater than 0", id);

            return BadRequest("Id must be greater than 0");
        }

        var result = await fieldGroupService.GetFieldGroupByIdAsync(id, cancellationToken);

        return result.Match<ActionResult<FieldGroupDto>>(fieldGroup => Ok(fieldGroup.ToDto()),
                                                         notFound => NotFound());
    }

    [HttpPost]
    [ProducesResponseType<FieldGroupDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<ActionResult<FieldGroupDto>> CreateFieldGroup(
        [FromBody] FieldGroupCreationRequest request, CancellationToken cancellationToken = default)
    {
        FieldGroupCreationRequest.Validator validator = new FieldGroupCreationRequest.Validator();
        ValidationResult valResult = await validator.ValidateAsync(request, cancellationToken);
        if (!valResult.IsValid)
        {
            logger.LogInformation("Tried to create field group with name {fieldGroupName}, but the request is invalid: {errors}",
                                  request.Name, valResult.Errors);

            return BadRequest(valResult.Errors);
        }

        await transaction.BeginTransactionAsync(cancellationToken);

        var result = await fieldGroupService.CreateFieldGroupAsync(request.Name, request.SingleChoiceFieldIds,
                                                                   request.FieldIds, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return CreatedAtAction(nameof(GetFieldGroupById), new { id = result.Id }, result.ToDto());
    }

    [HttpPut]
    [Route("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> UpdateFieldGroupById([FromRoute] long id,
                                                               [FromBody] FieldGroupUpdateRequest request,
                                                               CancellationToken cancellationToken = default)
    {
        if (id < 1)
        {
            logger.LogInformation("Tried to update field group with id {fieldGroupId}, but the id must be greater than 0",
                                  id);

            return BadRequest("Id must be greater than 0");
        }

        FieldGroupUpdateRequest.Validator validator = new FieldGroupUpdateRequest.Validator();
        ValidationResult valResult = await validator.ValidateAsync(request, cancellationToken);
        if (!valResult.IsValid)
        {
            logger.LogInformation("Tried to update field group with id {fieldGroupId}, but the request is invalid: {errors}",
                                  id, valResult.Errors);

            return BadRequest(valResult.Errors);
        }

        await transaction.BeginTransactionAsync(cancellationToken);

        var result = await fieldGroupService.UpdateFieldGroupAsync(id, request.Name, request.SingleChoiceFieldIds,
                                                                   request.FieldIds, cancellationToken);

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
    public async ValueTask<IActionResult> DeleteFieldGroupById([FromRoute] long id,
                                                               CancellationToken cancellationToken = default)
    {
        if (id < 1)
        {
            logger.LogInformation("Tried to delete field group with id {fieldGroupId}, but the id must be greater than 0",
                                  id);

            return BadRequest("Id must be greater than 0");
        }

        await transaction.BeginTransactionAsync(cancellationToken);

        var result = await fieldGroupService.DeleteFieldGroupAsync(id, cancellationToken);

        return await result.Match<ValueTask<IActionResult>>(async success =>
                                                            {
                                                                await transaction.CommitAsync(cancellationToken);

                                                                return NoContent();
                                                            },
                                                            notFound => ValueTask
                                                                .FromResult<IActionResult>(NotFound()));
    }
}

public sealed class FieldGroupListResponse
{
    public required List<FieldGroupListDto> FieldGroups { get; set; }
}

public sealed class FieldGroupCreationRequest
{
    public required string Name { get; set; }
    public required List<long> SingleChoiceFieldIds { get; set; }
    public required List<long> FieldIds { get; set; }

    public sealed class Validator : AbstractValidator<FieldGroupCreationRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty();
            RuleFor(x => x.SingleChoiceFieldIds).NotNull();
            RuleFor(x => x.FieldIds).NotNull();
        }
    }
}

public sealed class FieldGroupUpdateRequest
{
    public required string Name { get; set; }
    public required List<long> SingleChoiceFieldIds { get; set; }
    public required List<long> FieldIds { get; set; }

    public sealed class Validator : AbstractValidator<FieldGroupUpdateRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty();
            RuleFor(x => x.SingleChoiceFieldIds).NotNull();
            RuleFor(x => x.FieldIds).NotNull();
        }
    }
}
