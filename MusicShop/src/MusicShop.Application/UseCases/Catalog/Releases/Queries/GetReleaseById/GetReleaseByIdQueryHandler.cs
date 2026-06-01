using MediatR;
using MusicShop.Application.DTOs.Catalog;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.Common.Mappings;
using MusicShop.Domain.Errors;

namespace MusicShop.Application.UseCases.Catalog.Releases.Queries.GetReleaseById;

public sealed class GetReleaseByIdQueryHandler(IReleaseRepository releaseRepository)
    : IRequestHandler<GetReleaseByIdQuery, Result<ReleaseDetailResponse>>
{
    public async Task<Result<ReleaseDetailResponse>> Handle(
        GetReleaseByIdQuery request,
        CancellationToken cancellationToken)
    {
        Release? release = await releaseRepository.GetWithDetailsAsync(request.Id, ct: cancellationToken);

        if (release == null)
        {
            return Result<ReleaseDetailResponse>.Failure(ReleaseErrors.NotFound);
        }

        ReleaseDetailResponse response = release.ToDetailResponse();

        return Result<ReleaseDetailResponse>.Success(response);
    }
}

