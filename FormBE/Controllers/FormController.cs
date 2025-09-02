using FluentValidation;
using FluentValidation.Results;
using FormBE.Core.Logic;
using FormBE.Core.Services;
using FormBE.Persistence.Util;
using FormBE.Util;
using Microsoft.AspNetCore.Mvc;

namespace FormBE.Controllers;

[Route("api/forms")]
public sealed class FormController(
    ITransactionProvider transaction,
    IFormService formService,
    ILogger<FormController> logger) : BaseController
{
    [HttpGet]
    [Route("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async ValueTask<ActionResult> GetAllForms(CancellationToken cancellationToken = default)
    {
        var list = await formService.GetFormsAsync(cancellationToken);

        return Ok(new FormListResponse()
        {
            Forms = list.Select(FormExtension.ToDto).ToList()
        });
    }

    [HttpGet]
    [Route("{id:long}")]
    [ProducesResponseType<FormDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<FormDto>> GetFormById([FromRoute] long id,
                                                              CancellationToken cancellationToken = default)
    {
        if (id < 1)
        {
            logger.LogInformation("Tried to get a form with id {formId}, but the id must be greater than 0", id);

            return BadRequest("Id must be greater than 0");
        }

        var result = await formService.GetFormByIdAsync(id, cancellationToken);

        return result.Match<ActionResult<FormDto>>(form => Ok(form.ToDto()),
                                                   notFound => NotFound());
    }

    [HttpPost]
    [Route("")]
    [ProducesResponseType<FormDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<ActionResult<FormDto>> CreateForm([FromBody] FormCreationRequest request,
                                                             CancellationToken cancellationToken = default)
    {
        FormCreationRequest.Validator validator = new FormCreationRequest.Validator();
        ValidationResult valResult = await validator.ValidateAsync(request, cancellationToken);
        if (!valResult.IsValid)
        {
            logger.LogInformation("Tried to create form with name {formName}, but the request is invalid: {errors}",
                                  request.Name, valResult.Errors);

            return BadRequest(valResult.Errors);
        }

        await transaction.BeginTransactionAsync(cancellationToken);

        var result = await formService.CreateFormAsync(request.GroupId, request.Name, request.FieldGroupIds,
                                                       cancellationToken);

        return await result.Match<ValueTask<ActionResult<FormDto>>>(async success =>
                                                                    {
                                                                        await transaction
                                                                            .CommitAsync(cancellationToken);

                                                                        return CreatedAtAction(nameof(GetFormById), new
                                                                        {
                                                                            Id = success.Value.Id
                                                                        }, success.Value.ToDto());
                                                                    },
                                                                    groupNotFound =>
                                                                        ValueTask
                                                                            .FromResult<
                                                                                ActionResult<
                                                                                    FormDto>>(BadRequest("Group with supplied id not found")));
    }

    [HttpPut]
    [Route("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<IActionResult> UpdateFormById([FromRoute] long id, [FromBody] FormUpdateRequest request,
                                                         CancellationToken cancellationToken = default)
    {
        if (id < 1)
        {
            logger.LogInformation("Tried to update form with id {formId}, but id must be greater than 0", id);

            return BadRequest("Id must be greater than 0");
        }

        FormUpdateRequest.Validator validator = new FormUpdateRequest.Validator();
        ValidationResult valResult = await validator.ValidateAsync(request, cancellationToken);
        if (!valResult.IsValid)
        {
            logger.LogInformation("Tried to update form with id {formId}, but the request is invalid: {errors}", id,
                                  valResult.Errors);

            return BadRequest(valResult.Errors);
        }

        await transaction.BeginTransactionAsync(cancellationToken);

        var result = await formService.UpdateFormAsync(id, request.GroupId, request.Name, request.FieldGroupIds,
                                                       cancellationToken);

        return await result.Match<ValueTask<IActionResult>>(async success =>
                                                            {
                                                                await transaction.CommitAsync(cancellationToken);

                                                                return NoContent();
                                                            },
                                                            notFound => ValueTask.FromResult<IActionResult>(NotFound()),
                                                            groupNotFound =>
                                                                ValueTask.FromResult<IActionResult>(BadRequest()));
    }

    [HttpDelete]
    [Route("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<IActionResult> DeleteFormById([FromRoute] long id,
                                                         CancellationToken cancellationToken = default)
    {
        if (id < 1)
        {
            logger.LogInformation("Tried to delete form with id {formId}, but id must be greater than 0", id);

            return BadRequest("Id must be greater than 0");
        }

        await transaction.BeginTransactionAsync(cancellationToken);

        var result = await formService.DeleteFormAsync(id, cancellationToken);

        return await result.Match<ValueTask<IActionResult>>(async success =>
                                                            {
                                                                await transaction.CommitAsync(cancellationToken);

                                                                return NoContent();
                                                            },
                                                            notFound => ValueTask
                                                                .FromResult<IActionResult>(NotFound()));
    }
}

public sealed class FormListResponse
{
    public required List<FormDto> Forms { get; set; }
}

public sealed class FormCreationRequest
{
    public required string Name { get; set; }
    public long? GroupId { get; set; }
    public required List<long> FieldGroupIds { get; set; }

    public sealed class Validator : AbstractValidator<FormCreationRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty();
            When(x => x.GroupId != null, () => { RuleFor(x => x.GroupId).GreaterThan(0); });
            RuleFor(x => x.FieldGroupIds).NotNull();
        }
    }
}

public sealed class FormUpdateRequest
{
    public required string Name { get; set; }
    public long? GroupId { get; set; }
    public required List<long> FieldGroupIds { get; set; }

    public sealed class Validator : AbstractValidator<FormUpdateRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty();
            When(x => x.GroupId != null, () => { RuleFor(x => x.GroupId).GreaterThan(0); });
            RuleFor(x => x.FieldGroupIds).NotNull();
        }
    }
}
