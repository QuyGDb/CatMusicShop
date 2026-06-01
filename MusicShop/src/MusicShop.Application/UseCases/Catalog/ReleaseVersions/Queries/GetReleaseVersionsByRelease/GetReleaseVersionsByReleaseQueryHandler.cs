using MediatR;
using MusicShop.Application.DTOs.Catalog;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.Common.Mappings;

namespace MusicShop.Application.UseCases.Catalog.ReleaseVersions.Queries.GetReleaseVersionsByRelease;

public sealed class GetReleaseVersionsByReleaseQueryHandler(IReleaseVersionRepository releaseVersionRepository)
    : IRequestHandler<GetReleaseVersionsByReleaseQuery, Result<IReadOnlyList<ReleaseVersionDto>>>
{
    public async Task<Result<IReadOnlyList<ReleaseVersionDto>>> Handle(
        GetReleaseVersionsByReleaseQuery request, 
        CancellationToken cancellationToken)
    {
        List<ReleaseVersion> versions = await releaseVersionRepository.GetByReleaseIdWithLabelAsync(
            request.ReleaseId, cancellationToken);

        List<ReleaseVersionDto> releaseVersionDtos = versions.Select(releaseVersion => releaseVersion.ToDto()).ToList();

        return Result<IReadOnlyList<ReleaseVersionDto>>.Success(releaseVersionDtos.AsReadOnly());
    }
}

