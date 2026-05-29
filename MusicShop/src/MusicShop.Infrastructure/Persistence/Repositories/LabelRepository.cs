using Microsoft.EntityFrameworkCore;
using MusicShop.Application.Common.Interfaces;
using MusicShop.Application.UseCases.Catalog.Labels.Queries.GetLabels;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Infrastructure.Persistence.Repositories;

public sealed class LabelRepository(AppDbContext context) : GenericRepository<Label>(context), ILabelRepository
{
    public async Task<(IReadOnlyList<Label> Items, int TotalCount)> GetPagedAsync(
        GetLabelsQuery request,
        CancellationToken ct = default)
    {
        IQueryable<Label> query = _dbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            query = query.Where(label => EF.Functions.ILike(label.Name, $"%{request.Q}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.Country))
        {
            query = query.Where(label => label.Country == request.Country);
        }

        int totalCount = await query.CountAsync(ct);

        List<Label> items = await query
            .OrderBy(label => label.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<Label?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(label => label.Slug == slug, ct);
    }

    public async Task<Label?> GetWithVersionsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(label => label.ReleaseVersions)
            .FirstOrDefaultAsync(label => label.Id == id, ct);
    }

    public async Task<Label?> GetWithVersionsBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(label => label.ReleaseVersions)
            .FirstOrDefaultAsync(label => label.Slug == slug, ct);
    }
}
