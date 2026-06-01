using MusicShop.Domain.Entities.Shop;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Application.Common.Interfaces.Repositories;

public interface ICuratedCollectionRepository : IRepository<CuratedCollection>
{
    Task<IReadOnlyList<CuratedCollection>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CuratedCollection>> GetPublishedAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CuratedCollection>> GetFeaturedCollectionsAsync(int count, CancellationToken ct = default);
    Task<CuratedCollection?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);

    // Items
    Task AddItemAsync(CuratedCollectionItem item, CancellationToken ct = default);
    Task RemoveItemAsync(Guid collectionId, Guid productId, CancellationToken ct = default);
    Task<bool> ItemExistsAsync(Guid collectionId, Guid productId, CancellationToken ct = default);
    Task<int> GetNextSortOrderAsync(Guid collectionId, CancellationToken ct = default);
}
