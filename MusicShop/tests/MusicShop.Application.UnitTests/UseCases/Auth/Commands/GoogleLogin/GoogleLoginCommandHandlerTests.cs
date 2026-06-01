using FluentAssertions;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.DTOs.Auth;
using MusicShop.Application.UseCases.Auth.Commands.GoogleLogin;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.System;
using MusicShop.Domain.Errors;
using MusicShop.Domain.Interfaces;
using NSubstitute;
using System.Linq.Expressions;

namespace MusicShop.Application.UnitTests.UseCases.Auth.Commands.GoogleLogin;

public class GoogleLoginCommandHandlerTests
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<RefreshToken> _refreshTokenRepository;
    private readonly IRefreshTokenHasher _refreshTokenHasher;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly GoogleLoginCommandHandler _handler;

    public GoogleLoginCommandHandlerTests()
    {
        _googleAuthService = Substitute.For<IGoogleAuthService>();
        _userRepository = Substitute.For<IRepository<User>>();
        _refreshTokenRepository = Substitute.For<IRepository<RefreshToken>>();
        _refreshTokenHasher = Substitute.For<IRefreshTokenHasher>();
        _tokenService = Substitute.For<ITokenService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new GoogleLoginCommandHandler(
            _googleAuthService,
            _userRepository,
            _refreshTokenRepository,
            _refreshTokenHasher,
            _tokenService,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenGoogleTokenIsInvalid()
    {
        // Arrange
        GoogleLoginCommand command = new GoogleLoginCommand("invalid-token");
        _googleAuthService.VerifyTokenAsync(command.IdToken, Arg.Any<CancellationToken>())
            .Returns(Result<GoogleUserPayload>.Failure(AuthErrors.InvalidToken));

        // Act
        Result<AuthResponse> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.InvalidToken);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserDoesNotExist_ShouldAutoRegister()
    {
        // Arrange
        GoogleLoginCommand command = new GoogleLoginCommand("valid-token");
        GoogleUserPayload payload = new GoogleUserPayload("google-id", "test@example.com", "Test User");
        
        _googleAuthService.VerifyTokenAsync(command.IdToken, Arg.Any<CancellationToken>())
            .Returns(Result<GoogleUserPayload>.Success(payload));

        _userRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        Result<AuthResponse> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userRepository.Received(1).Add(Arg.Is<User>(u => u.Email == payload.Email && u.ExternalId == payload.ExternalId));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserExistsWithSameEmail_ShouldLinkAccount()
    {
        // Arrange
        GoogleLoginCommand command = new GoogleLoginCommand("valid-token");
        GoogleUserPayload payload = new GoogleUserPayload("google-id", "existing@example.com", "Existing User");
        User existingLocalUser = new User { Email = "existing@example.com", ExternalId = null };
        
        _googleAuthService.VerifyTokenAsync(command.IdToken, Arg.Any<CancellationToken>())
            .Returns(Result<GoogleUserPayload>.Success(payload));

        _userRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existingLocalUser);

        // Act
        Result<AuthResponse> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingLocalUser.ExternalId.Should().Be(payload.ExternalId);
        existingLocalUser.IdentityProvider.Should().Be("Google");
        _userRepository.Received(1).Update(existingLocalUser);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

