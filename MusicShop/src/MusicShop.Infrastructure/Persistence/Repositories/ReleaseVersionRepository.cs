using Microsoft.EntityFrameworkCore;
using MusicShop.Application.Common.Interfaces;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Infrastructure.Persistence.Repositories;

public sealed class ReleaseVersionRepository(AppDbContext context) : GenericRepository<ReleaseVersion>(context), IReleaseVersionRepository
{
    public async Task<List<ReleaseVersion>> GetByReleaseIdWithLabelAsync(Guid releaseId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(releaseVersion => releaseVersion.Label)
            .Where(releaseVersion => releaseVersion.ReleaseId == releaseId)
            .AsNoTracking()
            .ToListAsync(ct);
    }
}
