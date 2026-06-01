using MediatR;
using MusicShop.Application.Common;
using MusicShop.Application.DTOs.Catalog;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.Common.Mappings;

namespace MusicShop.Application.UseCases.Catalog.Releases.Queries.GetReleases;

public sealed class GetReleasesQueryHandler(IReleaseRepository releaseRepository)
    : IRequestHandler<GetReleasesQuery, Result<PaginatedResult<ReleaseResponse>>>
{
    public async Task<Result<PaginatedResult<ReleaseResponse>>> Handle(
        GetReleasesQuery request,
        CancellationToken cancellationToken)
    {
        (IReadOnlyList<Release> releases, int totalCount) = await releaseRepository.GetPagedAsync(request, cancellationToken);

        List<ReleaseResponse> releaseResponses = releases.Select(release => release.ToResponse()).ToList();

        PaginatedResult<ReleaseResponse> result = new PaginatedResult<ReleaseResponse>(
            releaseResponses,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedResult<ReleaseResponse>>.Success(result);
    }
}

