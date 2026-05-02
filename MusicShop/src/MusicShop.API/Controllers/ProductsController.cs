using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicShop.API.Controllers.Base;
using MusicShop.API.Infrastructure;
using MusicShop.Application.Common;
using MusicShop.Application.DTOs.Shop;
using MusicShop.Application.UseCases.Shop.Products.Commands.CreateProduct;
using MusicShop.Application.UseCases.Shop.Products.Commands.DeactivateProduct;
using MusicShop.Application.UseCases.Shop.Products.Commands.UpdateProduct;
using MusicShop.Application.UseCases.Shop.Products.Queries.GetProductById;
using MusicShop.Application.UseCases.Shop.Products.Queries.GetProductBySlug;
using MusicShop.Application.UseCases.Shop.Products.Queries.GetProducts;
using MusicShop.Domain.Common;

namespace MusicShop.API.Controllers;

public sealed class ProductsController(IMediator mediator) : BaseApiController
{
    // ── Products ────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProductListItemDto>>>> GetProducts(
        [FromQuery] GetProductsQuery query,
        CancellationToken cancellationToken)
    {
        Result<PaginatedResult<ProductListItemDto>> result = await mediator.Send(query, cancellationToken);
        return HandlePaginatedResult(result);
    }

    [HttpGet("{slug}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> GetProductBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        Result<ProductDetailDto> result = await mediator.Send(new GetProductBySlugQuery(slug), cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = "admin")]
    [HttpGet("admin/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> GetProductById(
        Guid id,
        CancellationToken cancellationToken)
    {
        Result<ProductDetailDto> result = await mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateProduct(
        [FromBody] CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        Result<Guid> result = await mediator.Send(command, cancellationToken);
        return HandleCreatedResult(result, nameof(GetProductBySlug), _ => new { slug = command.Slug });
    }

    [Authorize(Roles = "admin")]
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        Result result = await mediator.Send(new UpdateProductCommand(
            id, 
            request.Name, 
            request.Slug, 
            request.Description, 
            request.IsActive, 
            request.IsPreorder, 
            request.PreorderReleaseDate, 
            request.LimitedQty,
            request.Price,
            request.StockQty,
            request.IsSigned,
            request.VinylAttributes,
            request.CdAttributes,
            request.CassetteAttributes), cancellationToken);

        return HandleNoContentResult(result);
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateProduct(
        Guid id,
        CancellationToken cancellationToken)
    {
        Result result = await mediator.Send(new DeactivateProductCommand(id), cancellationToken);
        return HandleNoContentResult(result);
    }
}
