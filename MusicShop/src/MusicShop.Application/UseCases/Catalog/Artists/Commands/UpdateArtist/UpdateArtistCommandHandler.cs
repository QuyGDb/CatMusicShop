using MediatR;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Domain.Errors;
using MusicShop.Domain.Interfaces;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;

namespace MusicShop.Application.UseCases.Catalog.Artists.Commands.UpdateArtist;

public sealed class UpdateArtistCommandHandler(
    IArtistRepository artistRepository,
    IRepository<Genre> genreRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateArtistCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        UpdateArtistCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch artist including Genres
        Artist? artist = await artistRepository.GetWithGenresAsync(request.Id, cancellationToken);

        if (artist == null)
        {
            return Result<string>.Failure(ArtistErrors.NotFound);
        }

        // 2. Check for duplicate name
        Artist? existingWithSameName = await artistRepository.FirstOrDefaultAsync(
            artistItem => artistItem.Name == request.Name && artistItem.Id != artist.Id, cancellationToken);

        if (existingWithSameName != null)
        {
            return Result<string>.Failure(ArtistErrors.DuplicateName);
        }

        // 3. Check for duplicate slug
        Artist? existingWithSameSlug = await artistRepository.FirstOrDefaultAsync(
            artistItem => artistItem.Slug == request.Slug && artistItem.Id != artist.Id, cancellationToken);

        if (existingWithSameSlug != null)
        {
            return Result<string>.Failure(ArtistErrors.DuplicateSlug);
        }

        // 4. Validate Genres existence
        if (request.GenreIds is { Count: > 0 })
        {
            List<Guid> distinctGenreIds = request.GenreIds.Distinct().ToList();

            int existingGenresCount = await genreRepository.CountAsync(
                genre => distinctGenreIds.Contains(genre.Id), cancellationToken);

            if (existingGenresCount != distinctGenreIds.Count)
            {
                return Result<string>.Failure(GenreErrors.NotFound);
            }
        }

        // 5. Update basic info
        artist.Name = request.Name;
        artist.Slug = request.Slug;
        artist.Bio = request.Bio;
        artist.Country = request.Country;
        artist.ImageUrl = request.ImageUrl;

        // 6. Update Genres
        if (request.GenreIds != null)
        {
            artist.ArtistGenres.Clear();
            foreach (Guid genreId in request.GenreIds.Distinct())
            {
                artist.ArtistGenres.Add(new ArtistGenre { GenreId = genreId });
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(artist.Slug);
    }
}

