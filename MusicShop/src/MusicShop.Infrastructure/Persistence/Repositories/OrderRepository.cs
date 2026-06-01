using Microsoft.EntityFrameworkCore;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Entities.Orders;
using MusicShop.Domain.Enums;
using MusicShop.Infrastructure.Persistence;

namespace MusicShop.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository(AppDbContext context) : GenericRepository<Order>(context), IOrderRepository
{
    public async Task<Order?> GetByIdWithDetailsAsync(Guid orderId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(order => order.OrderItems)
                .ThenInclude(orderItem => orderItem.Product)
            .Include(order => order.Payment)
            .FirstOrDefaultAsync(order => order.Id == orderId, ct);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedByUserIdAsync(
        Guid userId,
        OrderStatus? status,
        int page,
        int limit,
        CancellationToken ct = default)
    {
        IQueryable<Order> query = _dbSet.AsNoTracking()
            .Where(order => order.UserId == userId)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(order => order.Status == status.Value);
        }

        int totalCount = await query.CountAsync(ct);

        List<Order> items = await query
            .Include(order => order.OrderItems)
            .OrderByDescending(order => order.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedAllAsync(
        OrderStatus? status,
        int page,
        int limit,
        CancellationToken ct = default)
    {
        IQueryable<Order> query = _dbSet.AsNoTracking().AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(order => order.Status == status.Value);
        }

        int totalCount = await query.CountAsync(ct);

        List<Order> items = await query
            .Include(order => order.OrderItems)
            .OrderByDescending(order => order.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}

