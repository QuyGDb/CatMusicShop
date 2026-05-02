using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MusicShop.API.Infrastructure;
using MusicShop.Application.DTOs.Catalog;
using MusicShop.Application.UseCases.Catalog.Artists.Commands.CreateArtist;
using MusicShop.Application.UseCases.Catalog.Artists.Commands.DeleteArtist;
using MusicShop.Application.UseCases.Catalog.Artists.Commands.UpdateArtist;
using MusicShop.Application.UseCases.Catalog.Artists.Queries.GetArtistBySlug;
using MusicShop.Application.UseCases.Catalog.Artists.Queries.GetArtists;
using MusicShop.Domain.Common;
using MusicShop.Application.Common;
using MusicShop.API.Controllers.Base;

namespace MusicShop.API.Controllers;

public class ArtistsController(IMediator mediator) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ArtistResponse>>>> GetArtists([FromQuery] GetArtistsQuery query)
    {
        Result<PaginatedResult<ArtistResponse>> result = await mediator.Send(query);
        return HandlePaginatedResult(result);
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<ApiResponse<ArtistResponse>>> GetArtist(string slug)
    {
        Result<ArtistResponse> result = await mediator.Send(new GetArtistBySlugQuery(slug));
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<string>>> CreateArtist([FromBody] CreateArtistCommand command)
    {
        Result<string> result = await mediator.Send(command);
        return HandleCreatedResult(result, nameof(GetArtist), value => new { slug = value });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)] // Kept because result is updated slug string
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<string>>> UpdateArtist(Guid id, [FromBody] UpdateArtistRequest request)
    {
        Result<string> result = await mediator.Send(new UpdateArtistCommand(
            id,
            request.Name,
            request.Slug,
            request.Bio,
            request.Country,
            request.ImageUrl,
            request.GenreIds));

        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteArtist(Guid id)
    {
        Result result = await mediator.Send(new DeleteArtistCommand(id));
        return HandleNoContentResult(result);
    }
}
