using MusicShop.Application.Common;
using MusicShop.Application.DTOs.Catalog;
using MusicShop.Application.UseCases.Catalog.Labels.Queries.GetLabels;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Application.Common.Interfaces.Repositories;

public interface ILabelRepository : IRepository<Label>
{
    Task<(IReadOnlyList<Label> Items, int TotalCount)> GetPagedAsync(
        GetLabelsQuery query, 
        CancellationToken ct = default);

    Task<Label?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Label?> GetWithVersionsAsync(Guid id, CancellationToken ct = default);
    Task<Label?> GetWithVersionsBySlugAsync(string slug, CancellationToken ct = default);
}
