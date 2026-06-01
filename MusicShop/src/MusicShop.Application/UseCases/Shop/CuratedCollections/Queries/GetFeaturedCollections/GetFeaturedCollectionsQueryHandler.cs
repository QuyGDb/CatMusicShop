using MediatR;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.Common.Mappings;
using MusicShop.Application.DTOs.Catalog;
using MusicShop.Application.DTOs.Shop;
using MusicShop.Domain.Common;

namespace MusicShop.Application.UseCases.Shop.CuratedCollections.Queries.GetFeaturedCollections;

public sealed class GetFeaturedCollectionsQueryHandler(
    ICuratedCollectionRepository repository) : IRequestHandler<GetFeaturedCollectionsQuery, Result<IReadOnlyList<CuratedCollectionFeaturedResponse>>>
{
    public async Task<Result<IReadOnlyList<CuratedCollectionFeaturedResponse>>> Handle(GetFeaturedCollectionsQuery request, CancellationToken cancellationToken)
    {
        var collections = await repository.GetFeaturedCollectionsAsync(request.Count, cancellationToken);

        var responses = collections
            .Select(c => c.ToFeaturedResponse())
            .ToList();

        return Result<IReadOnlyList<CuratedCollectionFeaturedResponse>>.Success(responses);
    }
}

