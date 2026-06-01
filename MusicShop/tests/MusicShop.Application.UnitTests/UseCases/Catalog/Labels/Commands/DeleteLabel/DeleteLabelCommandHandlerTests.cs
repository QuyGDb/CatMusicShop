using FluentAssertions;
using MusicShop.Application.UseCases.Catalog.Labels.Commands.DeleteLabel;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Catalog;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Interfaces;
using MusicShop.Domain.Errors;
using NSubstitute;

namespace MusicShop.Application.UnitTests.UseCases.Catalog.Labels.Commands.DeleteLabel;

public class DeleteLabelCommandHandlerTests
{
    private readonly ILabelRepository _labelRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteLabelCommandHandler _handler;

    public DeleteLabelCommandHandlerTests()
    {
        _labelRepository = Substitute.For<ILabelRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new DeleteLabelCommandHandler(_labelRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_LabelExistsAndHasNoVersions()
    {
        // Arrange
        Guid labelId = Guid.NewGuid();
        DeleteLabelCommand command = new DeleteLabelCommand(labelId);
        Label label = new Label { Id = labelId };
        
        _labelRepository.GetWithVersionsAsync(labelId, Arg.Any<CancellationToken>())
            .Returns(label);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _labelRepository.Received(1).Delete(label);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_LabelNotFound()
    {
        // Arrange
        Guid labelId = Guid.NewGuid();
        DeleteLabelCommand command = new DeleteLabelCommand(labelId);
        
        _labelRepository.GetWithVersionsAsync(labelId, Arg.Any<CancellationToken>())
            .Returns((Label?)null);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LabelErrors.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_LabelHasVersions()
    {
        // Arrange
        Guid labelId = Guid.NewGuid();
        DeleteLabelCommand command = new DeleteLabelCommand(labelId);
        Label label = new Label { Id = labelId };
        label.ReleaseVersions.Add(new ReleaseVersion { Id = Guid.NewGuid() });
        
        _labelRepository.GetWithVersionsAsync(labelId, Arg.Any<CancellationToken>())
            .Returns(label);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LabelErrors.HasAssociations);
        _labelRepository.DidNotReceive().Delete(Arg.Any<Label>());
    }
}

