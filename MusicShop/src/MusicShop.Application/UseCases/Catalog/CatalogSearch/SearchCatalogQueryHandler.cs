using MediatR;
using MusicShop.Application.DTOs.Catalog;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.Common.Mappings;

namespace MusicShop.Application.UseCases.Catalog.CatalogSearch;

public sealed class SearchCatalogQueryHandler(
    IArtistRepository artistRepository,
    IReleaseRepository releaseRepository)
    : IRequestHandler<SearchCatalogQuery, Result<CatalogSearchResult>>
{
    public async Task<Result<CatalogSearchResult>> Handle(
        SearchCatalogQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Q))
        {
            return Result<CatalogSearchResult>.Success(new CatalogSearchResult());
        }

        // 1. Search Artists
        List<Artist> artists = await artistRepository.SearchByNameAsync(request.Q, 5, cancellationToken);
        List<ArtistResponse> artistResponses = artists.Select(artist => artist.ToResponse()).ToList();

        // 2. Search Releases
        List<Release> releases = await releaseRepository.SearchByTitleAsync(request.Q, 10, cancellationToken);
        List<ReleaseResponse> releaseResponses = releases.Select(release => release.ToResponse()).ToList();

        CatalogSearchResult result = new()
        {
            Artists = artistResponses,
            Releases = releaseResponses
        };

        return Result<CatalogSearchResult>.Success(result);
    }
}

