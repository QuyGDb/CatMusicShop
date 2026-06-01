using FluentAssertions;
using MusicShop.Application.UseCases.Catalog.Artists.Commands.UpdateArtist;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Domain.Interfaces;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Errors;
using NSubstitute;

namespace MusicShop.Application.UnitTests.UseCases.Catalog.Artists.Commands.UpdateArtist;

public class UpdateArtistCommandHandlerTests
{
    private readonly IArtistRepository _artistRepository;
    private readonly IRepository<Genre> _genreRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateArtistCommandHandler _handler;

    public UpdateArtistCommandHandlerTests()
    {
        _artistRepository = Substitute.For<IArtistRepository>();
        _genreRepository = Substitute.For<IRepository<Genre>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new UpdateArtistCommandHandler(_artistRepository, _genreRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_ValidRequest()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        UpdateArtistCommand command = new UpdateArtistCommand(
            artistId,
            "Updated Artist",
            "updated-slug",
            "New Bio",
            "USA",
            null,
            null);

        Artist existingArtist = new Artist { Id = artistId, Name = "Old Name", Slug = "old-slug" };
        
        _artistRepository.GetWithGenresAsync(artistId, Arg.Any<CancellationToken>())
            .Returns(existingArtist);
            
        _artistRepository.FirstOrDefaultAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<Artist, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((Artist?)null);

        // Act
        Result<string> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("updated-slug");
        existingArtist.Name.Should().Be("Updated Artist");
        _artistRepository.Received(1).Update(existingArtist);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_ArtistNotFound()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        UpdateArtistCommand command = new UpdateArtistCommand(artistId, "Name", "slug", null, null, null);
        
        _artistRepository.GetWithGenresAsync(artistId, Arg.Any<CancellationToken>())
            .Returns((Artist?)null);

        // Act
        Result<string> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ArtistErrors.NotFound);
    }
}

