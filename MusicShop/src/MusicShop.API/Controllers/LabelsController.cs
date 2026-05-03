using Microsoft.AspNetCore.Mvc;
using MusicShop.API.Controllers.Base;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using MusicShop.Application.Common;
using MusicShop.Application.DTOs.Catalog;
using MusicShop.Application.UseCases.Catalog.Labels.Queries.GetLabels;
using MusicShop.Application.UseCases.Catalog.Labels.Queries.GetLabelBySlug;
using MusicShop.Application.UseCases.Catalog.Labels.Commands.CreateLabel;
using MusicShop.Application.UseCases.Catalog.Labels.Commands.UpdateLabel;
using MusicShop.Application.UseCases.Catalog.Labels.Commands.DeleteLabel;
using MusicShop.API.Infrastructure;
using MusicShop.Domain.Common;

namespace MusicShop.API.Controllers;

public class LabelsController(IMediator mediator) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LabelResponse>>>> GetLabels([FromQuery] GetLabelsQuery query)
    {
        Result<PaginatedResult<LabelResponse>> result = await mediator.Send(query);
        return HandlePaginatedResult(result);
    }

    [HttpGet("{slug}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LabelResponse>>> GetLabel(string slug)
    {
        Result<LabelResponse> result = await mediator.Send(new GetLabelBySlugQuery(slug));
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<string>>> CreateLabel([FromBody] CreateLabelCommand command)
    {
        Result<string> result = await mediator.Send(command);
        return HandleCreatedResult(result, nameof(GetLabel), value => new { slug = value });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<string>>> UpdateLabel(Guid id, [FromBody] UpdateLabelRequest request)
    {
        Result<string> result = await mediator.Send(new UpdateLabelCommand(
            id, 
            request.Name, 
            request.Slug, 
            request.Country, 
            request.FoundedYear, 
            request.Website));

        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLabel(Guid id)
    {
        Result result = await mediator.Send(new DeleteLabelCommand(id));
        return HandleNoContentResult(result);
    }
}
