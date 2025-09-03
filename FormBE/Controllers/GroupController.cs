using FluentValidation;
using FluentValidation.Results;
using FormBE.Core.Logic;
using FormBE.Core.Services;
using FormBE.Persistence.Model;
using FormBE.Persistence.Util;
using FormBE.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using OneOf.Types;

namespace FormBE.Controllers;

[Route("api/groups")]
public sealed class GroupController(
    ITransactionProvider transaction,
    IGroupService groupService,
    ILogger<GroupController> logger) : BaseController
{
    [HttpGet]
    [Route("")]
    [ProducesResponseType<GroupListResponse>(StatusCodes.Status200OK)]
    public async ValueTask<ActionResult<GroupListResponse>> GetAllGroups(CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Group> list = await groupService.GetGroupsAsync(cancellationToken);

        return Ok(new GroupListResponse()
        {
            Groups = list.Select(GroupExtension.ToDto).ToList()
        });
    }

    [HttpGet]
    [Route("{id:long}")]
    [ProducesResponseType<GroupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<GroupDto>> GetGroupById([FromRoute] long id,
                                                                CancellationToken cancellationToken = default)
    {
        if (id < 1L)
        {
            logger.LogInformation("Tried to get group with id {groupId}, but the id must be over 0", id);
            return BadRequest("Id must be greater than zero");
        }

        var result = await groupService.GetGroupByIdAsync(id, cancellationToken);

        return result.Match<ActionResult<GroupDto>>(group => Ok(group.ToDto()),
                                                    notFound => NotFound());
    }

    [HttpPost]
    [Route("")]
    [ProducesResponseType<GroupDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<ActionResult<GroupDto>> CreateGroup([FromBody] GroupCreationRequest request,
                                                               CancellationToken cancellationToken = default)
    {
        GroupCreationRequest.Validator validator = new GroupCreationRequest.Validator();
        ValidationResult valResult = await validator.ValidateAsync(request, cancellationToken);
        if (!valResult.IsValid)
        {
            logger.LogInformation("Tried to create group with name {groupName}, but request was invalid: {errors}", request.Name, valResult.Errors);
            return BadRequest(valResult.Errors);
        }

        await transaction.BeginTransactionAsync(cancellationToken);

        var result = await groupService.CreateGroupAsync(request.ParentId, request.Name, request.SubgroupIds,
                                                         request.FormIds, cancellationToken);

        return await result.Match<ValueTask<ActionResult<GroupDto>>>(async success =>
                                                                     {
                                                                         await transaction
                                                                             .CommitAsync(cancellationToken);

                                                                         return CreatedAtAction(nameof(GetGroupById),
                                                                          new
                                                                          {
                                                                              Id = success.Value.Id
                                                                          }, success.Value.ToDto());
                                                                     },
                                                                     parentNotFound =>
                                                                         ValueTask
                                                                             .FromResult<
                                                                                 ActionResult<GroupDto>>(BadRequest()));
    }

    [HttpPut]
    [Route("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async ValueTask<IActionResult> UpdateGroupById(
        [FromRoute] long id,
        [FromBody] GroupUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id < 1L)
        {
            logger.LogInformation("Tried to update group with id {groupId}, but id must be greater than 0", id);
            return BadRequest("Id must be greater than zero");
        }

        GroupUpdateRequest.Validator validator = new GroupUpdateRequest.Validator();
        ValidationResult valResult = await validator.ValidateAsync(request, cancellationToken);
        if (!valResult.IsValid)
        {
            logger.LogInformation("Tried to update group with id {groupId}, but request was invalid: {errors}", id, valResult.Errors);
            return BadRequest(valResult.Errors);
        }

        await transaction.BeginTransactionAsync(cancellationToken);

        var result = await groupService.UpdateGroupAsync(id, request.ParentId, request.Name, request.SubgroupIds,
                                                         request.FormIds, cancellationToken);

        return await result.Match<ValueTask<IActionResult>>(async success =>
                                                            {
                                                                await transaction.CommitAsync(cancellationToken);

                                                                return NoContent();
                                                            },
                                                            notFound => ValueTask.FromResult<IActionResult>(NotFound()),
                                                            parentNotFound =>
                                                                ValueTask
                                                                    .FromResult<
                                                                        IActionResult>(BadRequest("Parent group not found")),
                                                            parentIsSelf =>
                                                                ValueTask
                                                                    .FromResult<
                                                                        IActionResult>(Conflict("Parent group can't be the group itself")));
    }

    [HttpDelete]
    [Route("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> DeleteGroupById([FromRoute] long id,
                                                      CancellationToken cancellationToken = default)
    {
        if (id < 1)
        {
            logger.LogInformation("Tried to delete group with id {groupId}, but id must be greater than 0", id);
            return BadRequest("Id must be greater than zero");
        }

        await transaction.BeginTransactionAsync(cancellationToken);

        var result = await groupService.DeleteGroupAsync(id, cancellationToken);

        return await result.Match<ValueTask<IActionResult>>(async success =>
                                                            {
                                                                await transaction.CommitAsync(cancellationToken);

                                                                return NoContent();
                                                            },
                                                            notFound => ValueTask
                                                                .FromResult<IActionResult>(NotFound()));
    }
}

public sealed class GroupListResponse
{
    public required List<GroupDto> Groups { get; set; }
}

public sealed class GroupCreationRequest
{
    public required string Name { get; set; }
    public long? ParentId { get; set; }
    public required List<long> SubgroupIds { get; set; }
    public required List<long> FormIds { get; set; }

    public sealed class Validator : AbstractValidator<GroupCreationRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty();
            When(x => x.ParentId != null, () => { RuleFor(x => x.ParentId).GreaterThan(0L); });
            RuleFor(x => x.SubgroupIds).NotNull();
            RuleFor(x => x.FormIds).NotNull();
        }
    }
}

public sealed class GroupUpdateRequest
{
    public required string Name { get; set; }
    public long? ParentId { get; set; }
    public required List<long> SubgroupIds { get; set; }
    public required List<long> FormIds { get; set; }

    public sealed class Validator : AbstractValidator<GroupUpdateRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty();
            When(x => x.ParentId != null, () => { RuleFor(x => x.ParentId).GreaterThan(0L); });
            RuleFor(x => x.SubgroupIds).NotNull();
            RuleFor(x => x.FormIds).NotNull();
        }
    }
}
