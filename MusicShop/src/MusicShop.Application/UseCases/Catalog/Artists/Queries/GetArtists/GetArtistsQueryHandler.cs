using MediatR;
using MusicShop.Application.Common;
using MusicShop.Application.DTOs.Catalog;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.Common.Mappings;

namespace MusicShop.Application.UseCases.Catalog.Artists.Queries.GetArtists;

public sealed class GetArtistsQueryHandler(IArtistRepository artistRepository)
    : IRequestHandler<GetArtistsQuery, Result<PaginatedResult<ArtistResponse>>>
{
    public async Task<Result<PaginatedResult<ArtistResponse>>> Handle(
        GetArtistsQuery request,
        CancellationToken cancellationToken)
    {
        (IReadOnlyList<Artist> artists, int totalCount) = await artistRepository.GetPagedAsync(request, cancellationToken);

        IReadOnlyList<ArtistResponse> artistResponses = artists.Select(artist => artist.ToResponse()).ToList();

        // 3. Wrap result
        PaginatedResult<ArtistResponse> result = new PaginatedResult<ArtistResponse>(
            artistResponses,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedResult<ArtistResponse>>.Success(result);
    }
}

