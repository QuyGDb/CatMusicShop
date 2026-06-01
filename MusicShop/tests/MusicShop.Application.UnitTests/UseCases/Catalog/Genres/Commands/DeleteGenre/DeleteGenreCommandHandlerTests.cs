using FluentAssertions;
using MusicShop.Application.UseCases.Catalog.Genres.Commands.DeleteGenre;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Interfaces;
using MusicShop.Domain.Errors;
using NSubstitute;

namespace MusicShop.Application.UnitTests.UseCases.Catalog.Genres.Commands.DeleteGenre;

public class DeleteGenreCommandHandlerTests
{
    private readonly IGenreRepository _genreRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteGenreCommandHandler _handler;

    public DeleteGenreCommandHandlerTests()
    {
        _genreRepository = Substitute.For<IGenreRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new DeleteGenreCommandHandler(_genreRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_GenreExistsAndHasNoAssociations()
    {
        // Arrange
        Guid genreId = Guid.NewGuid();
        DeleteGenreCommand command = new DeleteGenreCommand(genreId);
        Genre genre = new Genre { Id = genreId };
        
        _genreRepository.GetWithAssociationsAsync(genreId, Arg.Any<CancellationToken>())
            .Returns(genre);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _genreRepository.Received(1).Delete(genre);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_GenreNotFound()
    {
        // Arrange
        Guid genreId = Guid.NewGuid();
        DeleteGenreCommand command = new DeleteGenreCommand(genreId);
        
        _genreRepository.GetWithAssociationsAsync(genreId, Arg.Any<CancellationToken>())
            .Returns((Genre?)null);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GenreErrors.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_GenreHasAssociations()
    {
        // Arrange
        Guid genreId = Guid.NewGuid();
        DeleteGenreCommand command = new DeleteGenreCommand(genreId);
        Genre genre = new Genre { Id = genreId };
        genre.ArtistGenres.Add(new ArtistGenre { ArtistId = Guid.NewGuid() });
        
        _genreRepository.GetWithAssociationsAsync(genreId, Arg.Any<CancellationToken>())
            .Returns(genre);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GenreErrors.HasAssociations);
        _genreRepository.DidNotReceive().Delete(Arg.Any<Genre>());
    }
}

