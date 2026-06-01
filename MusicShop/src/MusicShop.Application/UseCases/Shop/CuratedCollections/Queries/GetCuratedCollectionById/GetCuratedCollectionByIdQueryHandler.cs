using MediatR;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.DTOs.Shop;
using MusicShop.Domain.Common;
using MusicShop.Domain.Errors;

namespace MusicShop.Application.UseCases.Shop.CuratedCollections.Queries.GetCuratedCollectionById;

public sealed class GetCuratedCollectionByIdQueryHandler(
    ICuratedCollectionRepository repository) : IRequestHandler<GetCuratedCollectionByIdQuery, Result<CuratedCollectionDetailResponse>>
{
    public async Task<Result<CuratedCollectionDetailResponse>> Handle(GetCuratedCollectionByIdQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.Shop.CuratedCollection? curatedCollection = await repository.GetByIdWithItemsAsync(request.Id, cancellationToken);

        if (curatedCollection == null)
            return Result<CuratedCollectionDetailResponse>.Failure(CuratedCollectionErrors.NotFound);

        List<ProductListItemDto> productListItemDtos = curatedCollection.Items
            .Select(item => item.Product)
            .Select(product => new ProductListItemDto
            {
                Id = product.Id,
                Name = product.Name,
                ArtistName = product.ReleaseVersion?.Release?.Artist?.Name,
                Format = product.ReleaseVersion?.Format ?? Domain.Enums.ReleaseFormat.Vinyl,
                IsLimited = product.IsLimited,
                IsPreorder = product.IsPreorder,
                CoverUrl = product.ReleaseVersion?.Release?.CoverUrl,
                Price = product.Price,
                StockQty = product.StockQty,
                InStock = product.IsAvailable && product.StockQty > 0
            })
            .ToList();

        return Result<CuratedCollectionDetailResponse>.Success(new CuratedCollectionDetailResponse(
            curatedCollection.Id,
            curatedCollection.Title,
            curatedCollection.Description,
            productListItemDtos));
    }
}

