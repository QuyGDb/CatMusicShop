using FluentAssertions;
using MusicShop.Application.UseCases.Catalog.Artists.Commands.DeleteArtist;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Application.Common.Interfaces;
using MusicShop.Domain.Interfaces;
using MusicShop.Domain.Errors;
using NSubstitute;

namespace MusicShop.Application.UnitTests.UseCases.Catalog.Artists.Commands.DeleteArtist;

public class DeleteArtistCommandHandlerTests
{
    private readonly IArtistRepository _artistRepository;
    private readonly IImageService _imageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteArtistCommandHandler _handler;

    public DeleteArtistCommandHandlerTests()
    {
        _artistRepository = Substitute.For<IArtistRepository>();
        _imageService = Substitute.For<IImageService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new DeleteArtistCommandHandler(_artistRepository, _imageService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_ArtistExistsAndHasNoReleases()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        DeleteArtistCommand command = new DeleteArtistCommand(artistId);
        Artist artist = new Artist { Id = artistId };
        
        _artistRepository.GetWithReleasesAsync(artistId, Arg.Any<CancellationToken>())
            .Returns(artist);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _artistRepository.Received(1).Delete(artist);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_DeleteImage_When_ArtistHasImageUrl()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        string imageUrl = "https://example.com/image.jpg";
        DeleteArtistCommand command = new DeleteArtistCommand(artistId);
        Artist artist = new Artist { Id = artistId, ImageUrl = imageUrl };
        
        _artistRepository.GetWithReleasesAsync(artistId, Arg.Any<CancellationToken>())
            .Returns(artist);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _imageService.Received(1).DeleteImageAsync(imageUrl, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_ArtistNotFound()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        DeleteArtistCommand command = new DeleteArtistCommand(artistId);
        
        _artistRepository.GetWithReleasesAsync(artistId, Arg.Any<CancellationToken>())
            .Returns((Artist?)null);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ArtistErrors.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_ArtistHasReleases()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        DeleteArtistCommand command = new DeleteArtistCommand(artistId);
        Artist artist = new Artist { Id = artistId };
        artist.Releases.Add(new Release { Id = Guid.NewGuid() });
        
        _artistRepository.GetWithReleasesAsync(artistId, Arg.Any<CancellationToken>())
            .Returns(artist);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ArtistErrors.HasAssociations);
        _artistRepository.DidNotReceive().Delete(Arg.Any<Artist>());
    }
}
