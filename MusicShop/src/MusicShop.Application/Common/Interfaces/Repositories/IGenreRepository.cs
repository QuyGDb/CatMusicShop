using MusicShop.Application.Common;
using MusicShop.Application.DTOs.Catalog;
using MusicShop.Application.UseCases.Catalog.Genres.Queries.GetGenres;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Application.Common.Interfaces.Repositories;

public interface IGenreRepository : IRepository<Genre>
{
    Task<(IReadOnlyList<Genre> Items, int TotalCount)> GetPagedAsync(
        GetGenresQuery query, 
        CancellationToken ct = default);

    Task<Genre?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Genre?> GetWithAssociationsAsync(Guid id, CancellationToken ct = default);
    Task<Genre?> GetWithAssociationsBySlugAsync(string slug, CancellationToken ct = default);
}
