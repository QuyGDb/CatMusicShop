using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicShop.API.Controllers.Base;
using MusicShop.Application.UseCases.Catalog.ReleaseVersions.Commands.CreateReleaseVersion;
using MusicShop.Application.UseCases.Catalog.ReleaseVersions.Commands.UpdateReleaseVersion;
using MusicShop.Application.UseCases.Catalog.ReleaseVersions.Commands.DeleteReleaseVersion;
using MusicShop.Application.UseCases.Catalog.ReleaseVersions.Queries.GetReleaseVersionsByRelease;
using MusicShop.Application.DTOs.Catalog;
using MusicShop.API.Infrastructure;
using MusicShop.Domain.Common;
using MusicShop.Domain.Enums;

namespace MusicShop.API.Controllers;

public class ReleaseVersionsController(IMediator mediator) : BaseApiController
{
    [HttpGet("by-release/{releaseId:guid}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ReleaseVersionDto>>>> GetByRelease(Guid releaseId)
    {
        Result<IReadOnlyList<ReleaseVersionDto>> result = await mediator.Send(new GetReleaseVersionsByReleaseQuery(releaseId));
        return HandleResult(result);
    }

    [HttpGet("formats")]
    public ActionResult<ApiResponse<IReadOnlyList<string>>> GetReleaseFormats()
    {
        IReadOnlyList<string> formats = Enum.GetNames<ReleaseFormat>();
        return Ok(ApiResponse<IReadOnlyList<string>>.SuccessResult(formats));
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateReleaseVersion([FromBody] CreateReleaseVersionCommand command)
    {
        Result<Guid> result = await mediator.Send(command);
        return HandleCreatedResult(result, nameof(GetByRelease), _ => new { releaseId = command.ReleaseId });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Guid>>> UpdateReleaseVersion(Guid id, [FromBody] UpdateReleaseVersionCommand command)
    {
        if (id != command.Id) return BadRequest();
        Result<Guid> result = await mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReleaseVersion(Guid id)
    {
        Result result = await mediator.Send(new DeleteReleaseVersionCommand(id));
        return HandleNoContentResult(result);
    }
}
