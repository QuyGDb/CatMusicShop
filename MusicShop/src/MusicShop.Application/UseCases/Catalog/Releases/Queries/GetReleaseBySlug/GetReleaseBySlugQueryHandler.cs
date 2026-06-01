using MediatR;
using MusicShop.Application.DTOs.Catalog;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.Common.Mappings;
using MusicShop.Domain.Errors;

namespace MusicShop.Application.UseCases.Catalog.Releases.Queries.GetReleaseBySlug;

public sealed class GetReleaseBySlugQueryHandler(IReleaseRepository releaseRepository)
    : IRequestHandler<GetReleaseBySlugQuery, Result<ReleaseDetailResponse>>
{
    public async Task<Result<ReleaseDetailResponse>> Handle(
        GetReleaseBySlugQuery request,
        CancellationToken cancellationToken)
    {
        Release? release = await releaseRepository.GetBySlugWithDetailsAsync(request.Slug, track: false, ct: cancellationToken);

        if (release == null)
        {
            return Result<ReleaseDetailResponse>.Failure(ReleaseErrors.NotFound);
        }

        ReleaseDetailResponse response = release.ToDetailResponse();

        return Result<ReleaseDetailResponse>.Success(response);
    }
}

