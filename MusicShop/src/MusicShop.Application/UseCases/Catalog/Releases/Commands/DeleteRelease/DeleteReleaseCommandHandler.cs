using MediatR;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Interfaces;
using MusicShop.Domain.Errors;

namespace MusicShop.Application.UseCases.Catalog.Releases.Commands.DeleteRelease;

public sealed class DeleteReleaseCommandHandler(
    IReleaseRepository releaseRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteReleaseCommand, Result>
{
    public async Task<Result> Handle(
        DeleteReleaseCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch with Versions to check associations
        Release? release = await releaseRepository.GetWithDetailsAsync(request.Id, track: true, cancellationToken);

        if (release == null)
        {
            return Result.Failure(ReleaseErrors.NotFound);
        }

        // 2. Prevent deletion if versions exist
        if (release.Versions.Count > 0)
        {
            return Result.Failure(ReleaseErrors.HasVersions);
        }

        // 3. Delete metadata associations (EF Core handles cascade usually, but explicit is fine)
        // Tracks and ReleaseGenres are part of the Aggregate - we can delete them.
        
        releaseRepository.Delete(release);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

