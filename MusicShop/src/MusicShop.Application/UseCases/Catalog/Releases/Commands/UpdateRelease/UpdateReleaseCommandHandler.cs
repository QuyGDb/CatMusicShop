using MediatR;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Interfaces;
using MusicShop.Domain.Errors;
using MusicShop.Application.DTOs.Catalog;
using Microsoft.Extensions.Logging;

namespace MusicShop.Application.UseCases.Catalog.Releases.Commands.UpdateRelease;

public sealed class UpdateReleaseCommandHandler(
    IReleaseRepository releaseRepository,
    IRepository<Artist> artistRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateReleaseCommandHandler> logger)
    : IRequestHandler<UpdateReleaseCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        UpdateReleaseCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating release with request: {@ReleaseRequest}", request);
        // 1. Fetch Release with all related data
        Release? release = await releaseRepository.GetWithDetailsAsync(request.Id, track: true, cancellationToken);

        if (release == null)
        {
            return Result<Guid>.Failure(ReleaseErrors.NotFound);
        }

        // 2. Verify Artist exists if changed
        if (release.ArtistId != request.ArtistId)
        {
            Artist? artist = await artistRepository.GetByIdAsync(request.ArtistId, cancellationToken);
            if (artist == null)
            {
                return Result<Guid>.Failure(ArtistErrors.NotFound);
            }
            release.ArtistId = request.ArtistId;
        }

        // 3. Update Basic Info
        if (release.Slug != request.Slug)
        {
            bool slugExists = await releaseRepository.AnyAsync(releaseItem => releaseItem.Slug == request.Slug && releaseItem.Id != request.Id, cancellationToken);
            if (slugExists)
            {
                return Result<Guid>.Failure(ReleaseErrors.DuplicateSlug);
            }
            release.Slug = request.Slug;
        }

        release.Title = request.Title;
        release.Year = request.Year;
        release.CoverUrl = request.CoverUrl;
        release.Description = request.Description;

        // 4. Update Genres (Sync)
        if (request.GenreIds != null)
        {
            release.ReleaseGenres.Clear();
            foreach (Guid genreId in request.GenreIds)
            {
                release.ReleaseGenres.Add(new ReleaseGenre { GenreId = genreId });
            }
        }

        // 5. Update Tracks (Replace all)
        if (request.Tracks != null)
        {
            release.Tracks.Clear();

            foreach (TrackCreateDto trackDto in request.Tracks)
            {
                release.Tracks.Add(new Track
                {
                    Title = trackDto.Title,
                    Position = trackDto.Position,
                    DurationSeconds = trackDto.DurationSeconds ?? 0,
                    Side = trackDto.Side
                });
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(release.Id);
    }
}

