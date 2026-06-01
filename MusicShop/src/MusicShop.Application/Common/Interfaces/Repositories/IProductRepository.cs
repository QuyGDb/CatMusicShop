using MusicShop.Domain.Entities.Shop;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Application.Common.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        UseCases.Shop.Products.Queries.GetProducts.GetProductsQuery query,
        CancellationToken ct = default);

    Task<Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetBySlugWithDetailsAsync(string slug, CancellationToken ct = default);

    Task<bool> HasOrdersAsync(Guid productId, CancellationToken ct = default);

}
