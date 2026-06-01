using MediatR;
using MusicShop.Application.Common;
using MusicShop.Application.DTOs.Catalog;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.Common.Mappings;

namespace MusicShop.Application.UseCases.Catalog.Genres.Queries.GetGenres;

public sealed class GetGenresQueryHandler(IGenreRepository genreRepository)
    : IRequestHandler<GetGenresQuery, Result<PaginatedResult<GenreResponse>>>
{
    public async Task<Result<PaginatedResult<GenreResponse>>> Handle(
        GetGenresQuery request, 
        CancellationToken cancellationToken)
    {
        (IReadOnlyList<Genre> genres, int totalCount) = await genreRepository.GetPagedAsync(request, cancellationToken);

        List<GenreResponse> genreResponses = genres.Select(genre => genre.ToResponse()).ToList();

        PaginatedResult<GenreResponse> result = new PaginatedResult<GenreResponse>(
            genreResponses,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedResult<GenreResponse>>.Success(result);
    }
}

