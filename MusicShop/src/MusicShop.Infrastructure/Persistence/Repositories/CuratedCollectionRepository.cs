using Microsoft.EntityFrameworkCore;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Entities.Shop;
using MusicShop.Infrastructure.Persistence;

namespace MusicShop.Infrastructure.Persistence.Repositories;

public sealed class CuratedCollectionRepository(AppDbContext context) 
    : GenericRepository<CuratedCollection>(context), ICuratedCollectionRepository
{
    public async Task<IReadOnlyList<CuratedCollection>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.CuratedCollections
            .Include(c => c.Items)
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<CuratedCollection>> GetPublishedAsync(CancellationToken ct = default)
    {
        return await _context.CuratedCollections
            .AsNoTracking()
            .Where(collection => collection.IsPublished)
            .OrderBy(collection => collection.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<CuratedCollection>> GetFeaturedCollectionsAsync(int count, CancellationToken ct = default)
    {
        return await _context.CuratedCollections
            .Include(c => c.Items.OrderBy(item => item.SortOrder))
                .ThenInclude(item => item.Product)
                    .ThenInclude(p => p.ReleaseVersion)
                        .ThenInclude(rv => rv!.Release)
                            .ThenInclude(r => r!.Artist)
            .AsNoTracking()
            .Where(c => c.IsPublished)
            .OrderByDescending(c => c.CreatedAt)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<CuratedCollection?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.CuratedCollections
            .Include(collection => collection.Items.OrderBy(item => item.SortOrder))
                .ThenInclude(item => item.Product)
                    .ThenInclude(product => product.ReleaseVersion)
                        .ThenInclude(releaseVersion => releaseVersion!.Release)
                            .ThenInclude(release => release!.Artist)
            .AsNoTracking()
            .FirstOrDefaultAsync(collection => collection.Id == id, ct);
    }

    public async Task AddItemAsync(CuratedCollectionItem item, CancellationToken ct = default)
    {
        await _context.Set<CuratedCollectionItem>().AddAsync(item, ct);
    }

    public async Task RemoveItemAsync(Guid collectionId, Guid productId, CancellationToken ct = default)
    {
        CuratedCollectionItem? item = await _context.Set<CuratedCollectionItem>()
            .FirstOrDefaultAsync(item => item.CollectionId == collectionId && item.ProductId == productId, ct);

        if (item != null)
        {
            _context.Set<CuratedCollectionItem>().Remove(item);
        }
    }

    public async Task<bool> ItemExistsAsync(Guid collectionId, Guid productId, CancellationToken ct = default)
    {
        return await _context.Set<CuratedCollectionItem>()
            .AnyAsync(item => item.CollectionId == collectionId && item.ProductId == productId, ct);
    }

    public async Task<int> GetNextSortOrderAsync(Guid collectionId, CancellationToken ct = default)
    {
        int maxOrder = await _context.Set<CuratedCollectionItem>()
            .Where(item => item.CollectionId == collectionId)
            .MaxAsync(item => (int?)item.SortOrder, ct) ?? 0;

        return maxOrder + 1;
    }
}

