using Microsoft.EntityFrameworkCore;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Entities.Orders;
using MusicShop.Infrastructure.Persistence;

namespace MusicShop.Infrastructure.Persistence.Repositories;

public sealed class CartRepository(AppDbContext context) : GenericRepository<Cart>(context), ICartRepository
{
    public async Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(cart => cart.Items)
                .ThenInclude(cartItem => cartItem.Product)
                    .ThenInclude(product => product.ReleaseVersion)
                        .ThenInclude(releaseVersion => releaseVersion!.Release)
                            .ThenInclude(release => release!.Artist)
            .FirstOrDefaultAsync(cart => cart.UserId == userId, ct);
    }

    public async Task<Cart?> GetByUserIdForUpdateAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(cart => cart.Items)
            .FirstOrDefaultAsync(cart => cart.UserId == userId, ct);
    }

    public async Task ClearCartAsync(Guid cartId, CancellationToken ct = default)
    {
        List<CartItem> cartItems = await _context.Set<CartItem>()
            .Where(cartItem => cartItem.CartId == cartId)
            .ToListAsync(ct);

        _context.Set<CartItem>().RemoveRange(cartItems);
    }
}

