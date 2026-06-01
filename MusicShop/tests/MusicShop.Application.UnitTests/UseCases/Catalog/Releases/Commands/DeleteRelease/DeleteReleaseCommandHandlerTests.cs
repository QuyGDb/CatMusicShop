using FluentAssertions;
using MusicShop.Application.UseCases.Catalog.Releases.Commands.DeleteRelease;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Interfaces;
using MusicShop.Domain.Errors;
using NSubstitute;

namespace MusicShop.Application.UnitTests.UseCases.Catalog.Releases.Commands.DeleteRelease;

public class DeleteReleaseCommandHandlerTests
{
    private readonly IReleaseRepository _releaseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteReleaseCommandHandler _handler;

    public DeleteReleaseCommandHandlerTests()
    {
        _releaseRepository = Substitute.For<IReleaseRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new DeleteReleaseCommandHandler(_releaseRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_ReleaseExistsAndHasNoVersions()
    {
        // Arrange
        Guid releaseId = Guid.NewGuid();
        DeleteReleaseCommand command = new DeleteReleaseCommand(releaseId);
        Release release = new Release { Id = releaseId }; // Versions is empty by default
        
        _releaseRepository.GetWithDetailsAsync(releaseId, track: true, Arg.Any<CancellationToken>())
            .Returns(release);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _releaseRepository.Received(1).Delete(release);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_ReleaseNotFound()
    {
        // Arrange
        Guid releaseId = Guid.NewGuid();
        DeleteReleaseCommand command = new DeleteReleaseCommand(releaseId);
        
        _releaseRepository.GetWithDetailsAsync(releaseId, track: true, Arg.Any<CancellationToken>())
            .Returns((Release?)null);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReleaseErrors.NotFound);
        _releaseRepository.DidNotReceive().Delete(Arg.Any<Release>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_ReleaseHasVersions()
    {
        // Arrange
        Guid releaseId = Guid.NewGuid();
        DeleteReleaseCommand command = new DeleteReleaseCommand(releaseId);
        Release release = new Release { Id = releaseId };
        release.Versions.Add(new ReleaseVersion { Id = Guid.NewGuid() });
        
        _releaseRepository.GetWithDetailsAsync(releaseId, track: true, Arg.Any<CancellationToken>())
            .Returns(release);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReleaseErrors.HasVersions);
        _releaseRepository.DidNotReceive().Delete(Arg.Any<Release>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

