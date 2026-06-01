using MusicShop.Application.Common;
using MusicShop.Application.DTOs.Catalog;
using MusicShop.Application.UseCases.Catalog.Artists.Queries.GetArtists;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Application.Common.Interfaces.Repositories;

public interface IArtistRepository : IRepository<Artist>
{
    // Get Artist details including list of Genres
    Task<Artist?> GetWithGenresAsync(Guid id, CancellationToken ct = default);
    Task<Artist?> GetWithGenresBySlugAsync(string slug, CancellationToken ct = default);
    Task<Artist?> GetWithReleasesAsync(Guid id, CancellationToken ct = default);

    // Get a paged list of Artists with their Genres and filters
    Task<(IReadOnlyList<Artist> Items, int TotalCount)> GetPagedAsync(
        GetArtistsQuery query,
        CancellationToken ct = default);

    Task<List<Artist>> SearchByNameAsync(string searchTerm, int limit, CancellationToken ct = default);
}
