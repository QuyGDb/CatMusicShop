using MediatR;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.Common.Mappings;
using MusicShop.Application.DTOs.Auth;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.System;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Application.UseCases.Auth.Commands.GoogleLogin;

public sealed class GoogleLoginCommandHandler(
    IGoogleAuthService googleAuthService,
    IRepository<User> userRepository,
    IRepository<RefreshToken> refreshTokenRepository,
    IRefreshTokenHasher refreshTokenHasher,
    ITokenService tokenService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<GoogleLoginCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(
        GoogleLoginCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Verify Google Token
        Result<GoogleUserPayload> googleResult = await googleAuthService.VerifyTokenAsync(request.IdToken, cancellationToken);

        if (googleResult.IsFailure)
        {
            return Result<AuthResponse>.Failure(googleResult.Error);
        }

        GoogleUserPayload payload = googleResult.Value;

        // 2. Find user by Email OR ExternalId
        User? user = await userRepository.FirstOrDefaultAsync(
            u => u.Email == payload.Email || (u.IdentityProvider == "Google" && u.ExternalId == payload.ExternalId),
            cancellationToken);

        if (user == null)
        {
            // 3. Auto-registration
            user = new User
            {
                Email = payload.Email,
                FullName = payload.Name,
                IdentityProvider = "Google",
                ExternalId = payload.ExternalId,
                Role = Domain.Enums.UserRole.Customer,
                PasswordHash = null
            };

            userRepository.Add(user);
        }
        else if (user.ExternalId == null)
        {
            // 4. Link Local account to Google
            user.IdentityProvider = "Google";
            user.ExternalId = payload.ExternalId;
            userRepository.Update(user);
        }

        // 5. Issue System Tokens
        (string accessToken, DateTime accessTokenExpiresAtUtc) = tokenService.GenerateAccessToken(user);
        (string refreshToken, DateTime refreshTokenExpiresAtUtc) = tokenService.GenerateRefreshToken();

        refreshTokenRepository.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHasher.Hash(refreshToken),
            ExpiresAt = refreshTokenExpiresAtUtc
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Success(user.ToAuthResponse(accessToken, refreshToken, accessTokenExpiresAtUtc));
    }
}

