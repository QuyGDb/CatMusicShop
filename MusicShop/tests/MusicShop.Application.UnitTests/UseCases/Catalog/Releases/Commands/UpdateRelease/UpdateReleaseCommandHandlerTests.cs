using FluentAssertions;
using MusicShop.Application.UseCases.Catalog.Releases.Commands.UpdateRelease;
using MusicShop.Application.UseCases.Catalog.Releases.Commands.CreateRelease;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Interfaces;
using MusicShop.Domain.Errors;
using MusicShop.Domain.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MusicShop.Application.UnitTests.UseCases.Catalog.Releases.Commands.UpdateRelease;

public class UpdateReleaseCommandHandlerTests
{
    private readonly IReleaseRepository _releaseRepository;
    private readonly IRepository<Artist> _artistRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateReleaseCommandHandler> _logger;
    private readonly UpdateReleaseCommandHandler _handler;

    public UpdateReleaseCommandHandlerTests()
    {
        _releaseRepository = Substitute.For<IReleaseRepository>();
        _artistRepository = Substitute.For<IRepository<Artist>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<UpdateReleaseCommandHandler>>();
        _handler = new UpdateReleaseCommandHandler(_releaseRepository, _artistRepository, _unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_ValidRequestWithoutArtistChange()
    {
        // Arrange
        Guid releaseId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        UpdateReleaseCommand command = new UpdateReleaseCommand(
            releaseId,
            "Updated Title",
            "updated-slug",
            2024,
            artistId,
            null,
            null,
            null,
            null
        );

        Release release = new Release 
        { 
            Id = releaseId, 
            ArtistId = artistId,
            Title = "Old Title"
        };
        
        _releaseRepository.GetWithDetailsAsync(releaseId, track: true, Arg.Any<CancellationToken>())
            .Returns(release);

        // Act
        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(releaseId);
        release.Title.Should().Be("Updated Title");
        _releaseRepository.Received(1).Update(release);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        // Should NOT check artist repository if ID hasn't changed
        await _artistRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_ArtistIdChangedAndArtistExists()
    {
        // Arrange
        Guid releaseId = Guid.NewGuid();
        Guid oldArtistId = Guid.NewGuid();
        Guid newArtistId = Guid.NewGuid();
        UpdateReleaseCommand command = new UpdateReleaseCommand(
            releaseId,
            "Updated Title",
            "updated-slug",
            2024,
            newArtistId,
            null,
            null,
            null,
            null
        );

        Release release = new Release 
        { 
            Id = releaseId, 
            ArtistId = oldArtistId 
        };
        
        _releaseRepository.GetWithDetailsAsync(releaseId, track: true, Arg.Any<CancellationToken>())
            .Returns(release);
        
        _artistRepository.GetByIdAsync(newArtistId, Arg.Any<CancellationToken>())
            .Returns(new Artist { Id = newArtistId });

        // Act
        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        release.ArtistId.Should().Be(newArtistId);
        _releaseRepository.Received(1).Update(release);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_ReleaseNotFound()
    {
        // Arrange
        Guid releaseId = Guid.NewGuid();
        UpdateReleaseCommand command = new UpdateReleaseCommand(
            releaseId, "Title", "slug", 2024, Guid.NewGuid(), null, null, null, null);
        
        _releaseRepository.GetWithDetailsAsync(releaseId, track: true, Arg.Any<CancellationToken>())
            .Returns((Release?)null);

        // Act
        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReleaseErrors.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_ArtistIdChangedButArtistNotFound()
    {
        // Arrange
        Guid releaseId = Guid.NewGuid();
        Guid oldArtistId = Guid.NewGuid();
        Guid newArtistId = Guid.NewGuid();
        UpdateReleaseCommand command = new UpdateReleaseCommand(
            releaseId, "Title", "slug", 2024, newArtistId, null, null, null, null);

        Release release = new Release { Id = releaseId, ArtistId = oldArtistId };
        
        _releaseRepository.GetWithDetailsAsync(releaseId, track: true, Arg.Any<CancellationToken>())
            .Returns(release);
        
        _artistRepository.GetByIdAsync(newArtistId, Arg.Any<CancellationToken>())
            .Returns((Artist?)null);

        // Act
        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ArtistErrors.NotFound);
    }
}

