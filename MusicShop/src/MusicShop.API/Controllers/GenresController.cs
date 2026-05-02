using Microsoft.AspNetCore.Mvc;
using MusicShop.API.Controllers.Base;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using MusicShop.Application.Common;
using MusicShop.Application.DTOs.Catalog;
using MusicShop.Application.UseCases.Catalog.Genres.Queries.GetGenres;
using MusicShop.Application.UseCases.Catalog.Genres.Queries.GetGenreBySlug;
using MusicShop.Application.UseCases.Catalog.Genres.Commands.CreateGenre;
using MusicShop.Application.UseCases.Catalog.Genres.Commands.DeleteGenre;
using MusicShop.Application.UseCases.Catalog.Genres.Commands.UpdateGenre;
using MusicShop.API.Infrastructure;
using MusicShop.Domain.Common;

namespace MusicShop.API.Controllers;

public class GenresController(IMediator mediator) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GenreResponse>>>> GetGenres([FromQuery] GetGenresQuery query)
    {
        Result<PaginatedResult<GenreResponse>> result = await mediator.Send(query);
        return HandlePaginatedResult(result);
    }

    [HttpGet("{slug}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<GenreResponse>>> GetGenre(string slug)
    {
        Result<GenreResponse> result = await mediator.Send(new GetGenreBySlugQuery(slug));
        return HandleResult(result);
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<string>>> CreateGenre([FromBody] CreateGenreCommand command)
    {
        Result<string> result = await mediator.Send(command);
        return HandleCreatedResult(result, nameof(GetGenre), value => new { slug = value });
    }


    [Authorize(Roles = "admin")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<GenreResponse>>> UpdateGenre(Guid id, [FromBody] UpdateGenreRequest request)
    {
        Result<GenreResponse> result = await mediator.Send(new UpdateGenreCommand(id, request.Name, request.Slug));
        return HandleResult(result);
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteGenre(Guid id)
    {
        Result result = await mediator.Send(new DeleteGenreCommand(id));
        return HandleNoContentResult(result);
    }
}
