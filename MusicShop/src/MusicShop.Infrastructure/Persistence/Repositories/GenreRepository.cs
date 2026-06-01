using Microsoft.EntityFrameworkCore;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.UseCases.Catalog.Genres.Queries.GetGenres;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Infrastructure.Persistence.Repositories;

public sealed class GenreRepository(AppDbContext context) : GenericRepository<Genre>(context), IGenreRepository
{
    public async Task<(IReadOnlyList<Genre> Items, int TotalCount)> GetPagedAsync(
        GetGenresQuery request,
        CancellationToken ct = default)
    {
        IQueryable<Genre> query = _dbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            query = query.Where(genre => EF.Functions.ILike(genre.Name, $"%{request.Q}%"));
        }

        int totalCount = await query.CountAsync(ct);

        List<Genre> items = await query
            .OrderBy(genre => genre.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<Genre?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(genre => genre.Slug == slug, ct);
    }

    public async Task<Genre?> GetWithAssociationsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(genre => genre.ArtistGenres)
            .Include(genre => genre.ReleaseGenres)
            .FirstOrDefaultAsync(genre => genre.Id == id, ct);
    }

    public async Task<Genre?> GetWithAssociationsBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(genre => genre.ArtistGenres)
            .Include(genre => genre.ReleaseGenres)
            .FirstOrDefaultAsync(genre => genre.Slug == slug, ct);
    }
}

