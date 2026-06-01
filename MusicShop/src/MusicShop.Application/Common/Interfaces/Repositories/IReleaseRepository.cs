using MusicShop.Application.Common;
using MusicShop.Application.DTOs.Catalog;
using MusicShop.Application.UseCases.Catalog.Releases.Queries.GetReleases;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Application.Common.Interfaces.Repositories;

public interface IReleaseRepository : IRepository<Release>
{
    Task<(IReadOnlyList<Release> Items, int TotalCount)> GetPagedAsync(
        GetReleasesQuery query,
        CancellationToken ct = default);


    Task<Release?> GetWithDetailsAsync(Guid id, bool track = false, CancellationToken ct = default);
    Task<Release?> GetBySlugWithDetailsAsync(string slug, bool track = false, CancellationToken ct = default);

    Task<List<Release>> SearchByTitleAsync(string searchTerm, int limit, CancellationToken ct = default);
}
