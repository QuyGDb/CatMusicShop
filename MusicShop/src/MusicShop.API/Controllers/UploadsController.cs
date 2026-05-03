using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicShop.API.Controllers.Base;
using MusicShop.API.Infrastructure;
using MusicShop.Application.UseCases.System.Uploads.Commands.UploadImage;
using MusicShop.Domain.Common;

namespace MusicShop.API.Controllers;

[Authorize(Roles = "Admin")]
public sealed class UploadsController(IMediator mediator) : BaseApiController
{
    [HttpPost("image")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<string>>> UploadImage(
        IFormFile file,
        [FromQuery] string folder = "general",
        CancellationToken cancellationToken = default)
    {
        using Stream stream = file?.OpenReadStream() ?? Stream.Null;

        UploadImageCommand command = new(
            stream,
            file?.FileName ?? string.Empty,
            file?.ContentType ?? string.Empty,
            folder);

        Result<string> result = await mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }
}
