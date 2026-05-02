using MediatR;
using Microsoft.AspNetCore.Mvc;
using MusicShop.API.Controllers.Base;
using MusicShop.API.Infrastructure;
using MusicShop.Domain.Common;
using MusicShop.Application.Common;
using MusicShop.Application.DTOs.Shop;
using MusicShop.Application.UseCases.Shop.CuratedCollections.Queries.GetCuratedCollectionById;
using MusicShop.Application.UseCases.Shop.CuratedCollections.Queries.GetCuratedCollections;
using MusicShop.Application.UseCases.Shop.CuratedCollections.Queries.GetFeaturedCollections;
using MusicShop.Application.UseCases.Shop.CuratedCollections.Commands.CreateCuratedCollection;
using MusicShop.Application.UseCases.Shop.CuratedCollections.Commands.UpdateCuratedCollection;
using MusicShop.Application.UseCases.Shop.CuratedCollections.Commands.AddProductToCollection;
using MusicShop.Application.UseCases.Shop.CuratedCollections.Commands.RemoveProductFromCollection;
using MusicShop.Application.UseCases.Shop.CuratedCollections.Commands.DeleteCuratedCollection;
using MusicShop.Application.UseCases.Shop.CuratedCollections.Commands.UpdateCuratedCollectionStatus;
using Microsoft.AspNetCore.Authorization;

namespace MusicShop.API.Controllers;

public sealed class CuratedCollectionsController(IMediator mediator) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CuratedCollectionResponse>>>> GetCuratedCollections(
        [FromQuery] bool includeUnpublished = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchQuery = null)
    {
        Result<PaginatedResult<CuratedCollectionResponse>> result = await mediator.Send(new GetCuratedCollectionsQuery(includeUnpublished, pageNumber, pageSize, searchQuery));
        return HandlePaginatedResult(result);
    }

    [HttpGet("featured")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CuratedCollectionFeaturedResponse>>>> GetFeaturedCollections([FromQuery] int count = 3)
    {
        Result<IReadOnlyList<CuratedCollectionFeaturedResponse>> result = await mediator.Send(new GetFeaturedCollectionsQuery(count));
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CuratedCollectionDetailResponse>>> GetCuratedCollectionById([FromRoute] Guid id)
    {
        Result<CuratedCollectionDetailResponse> result = await mediator.Send(new GetCuratedCollectionByIdQuery(id));
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateCuratedCollection([FromBody] CreateCuratedCollectionCommand command)
    {
        Result<Guid> result = await mediator.Send(command);
        return HandleCreatedResult(result, nameof(GetCuratedCollectionById), id => new { id });
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Guid>>> UpdateCuratedCollection([FromRoute] Guid id, [FromBody] UpdateCuratedCollectionRequest request)
    {
        Result<Guid> result = await mediator.Send(new UpdateCuratedCollectionCommand(id, request.Title, request.Description, request.IsPublished));
        return HandleResult(result);
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Unit>>> UpdateCuratedCollectionStatus([FromRoute] Guid id, [FromBody] bool isPublished)
    {
        Result<Unit> result = await mediator.Send(new UpdateCuratedCollectionStatusCommand(id, isPublished));
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/items")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Guid>>> AddProductToCollection([FromRoute] Guid id, [FromBody] AddProductToCollectionRequest request)
    {
        Result<Guid> result = await mediator.Send(new AddProductToCollectionCommand(id, request.ProductId, request.SortOrder));
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}/items/{productId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Unit>>> RemoveProductFromCollection([FromRoute] Guid id, [FromRoute] Guid productId)
    {
        Result<Unit> result = await mediator.Send(new RemoveProductFromCollectionCommand(id, productId));
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Unit>>> DeleteCuratedCollection([FromRoute] Guid id)
    {
        Result<Unit> result = await mediator.Send(new DeleteCuratedCollectionCommand(id));
        return HandleResult(result);
    }
}
