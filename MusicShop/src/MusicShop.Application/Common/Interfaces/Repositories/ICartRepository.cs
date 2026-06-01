using MusicShop.Domain.Entities.Orders;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Application.Common.Interfaces.Repositories;

public interface ICartRepository : IRepository<Cart>
{
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Cart?> GetByUserIdForUpdateAsync(Guid userId, CancellationToken ct = default);
    Task ClearCartAsync(Guid cartId, CancellationToken ct = default);
}
