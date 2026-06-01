using MediatR;
using MusicShop.Application.DTOs.Catalog;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.Common.Mappings;
using MusicShop.Domain.Errors;

namespace MusicShop.Application.UseCases.Catalog.Artists.Queries.GetArtistBySlug;

public sealed class GetArtistBySlugQueryHandler(IArtistRepository artistRepository)
    : IRequestHandler<GetArtistBySlugQuery, Result<ArtistResponse>>
{
    public async Task<Result<ArtistResponse>> Handle(
        GetArtistBySlugQuery request,
        CancellationToken cancellationToken)
    {
        Artist? artist = await artistRepository.GetWithGenresBySlugAsync(request.Slug, cancellationToken);

        if (artist == null)
        {
            return Result<ArtistResponse>.Failure(ArtistErrors.NotFound);
        }

        ArtistResponse response = artist.ToResponse();

        return Result<ArtistResponse>.Success(response);
    }
}

