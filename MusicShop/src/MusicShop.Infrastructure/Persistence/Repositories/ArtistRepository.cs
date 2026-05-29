using Microsoft.EntityFrameworkCore;
using MusicShop.Application.Common.Interfaces;
using MusicShop.Application.UseCases.Catalog.Artists.Queries.GetArtists;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Infrastructure.Persistence.Repositories;

public sealed class ArtistRepository(AppDbContext context) : GenericRepository<Artist>(context), IArtistRepository
{
    public async Task<Artist?> GetWithGenresAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(artist => artist.ArtistGenres)
                .ThenInclude(artistGenre => artistGenre.Genre)
            .FirstOrDefaultAsync(artist => artist.Id == id, cancellationToken);
    }

    public async Task<Artist?> GetWithGenresBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(artist => artist.ArtistGenres)
                .ThenInclude(artistGenre => artistGenre.Genre)
            .FirstOrDefaultAsync(artist => artist.Slug == slug, cancellationToken);
    }

    public async Task<Artist?> GetWithReleasesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(artist => artist.Releases)
            .Include(artist => artist.ArtistGenres)
            .FirstOrDefaultAsync(artist => artist.Id == id, cancellationToken);
    }

    public async Task<Artist?> GetWithReleasesBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(artist => artist.Releases)
            .FirstOrDefaultAsync(artist => artist.Slug == slug, cancellationToken);
    }

    public async Task<(IReadOnlyList<Artist> Items, int TotalCount)> GetPagedAsync(
        GetArtistsQuery request,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Artist> query = _dbSet
            .Include(artist => artist.ArtistGenres)
                .ThenInclude(artistGenre => artistGenre.Genre)
            .AsNoTracking();

        // 1. Apply Filtering
        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            query = query.Where(artist => EF.Functions.ILike(artist.Name, $"%{request.Q}%"));
        }

        if (request.GenreId.HasValue)
        {
            query = query.Where(artist => artist.ArtistGenres.Any(artistGenre => artistGenre.GenreId == request.GenreId.Value));
        }

        // 2. Wrap into TotalCount and Paging
        int totalCount = await query.CountAsync(cancellationToken);

        List<Artist> items = await query
            .OrderBy(artist => artist.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<List<Artist>> SearchByNameAsync(string searchTerm, int limit, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(artist => artist.ArtistGenres)
                .ThenInclude(artistGenre => artistGenre.Genre)
            .Where(artist => EF.Functions.ILike(artist.Name, $"%{searchTerm}%"))
            .Take(limit)
            .ToListAsync(ct);
    }
}
