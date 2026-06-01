using MediatR;
using MusicShop.Application.DTOs.Catalog;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.Common.Mappings;
using MusicShop.Domain.Errors;

namespace MusicShop.Application.UseCases.Catalog.Artists.Queries.GetArtistById;

public sealed class GetArtistByIdQueryHandler(IArtistRepository artistRepository)
    : IRequestHandler<GetArtistByIdQuery, Result<ArtistResponse>>
{
    public async Task<Result<ArtistResponse>> Handle(
        GetArtistByIdQuery request,
        CancellationToken cancellationToken)
    {
        Artist? artist = await artistRepository.GetWithGenresAsync(request.Id, cancellationToken);

        if (artist == null)
        {
            return Result<ArtistResponse>.Failure(ArtistErrors.NotFound);
        }

        ArtistResponse response = artist.ToResponse();

        return Result<ArtistResponse>.Success(response);
    }
}

