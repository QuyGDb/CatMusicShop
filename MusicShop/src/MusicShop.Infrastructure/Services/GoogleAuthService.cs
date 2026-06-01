using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Common;
using MusicShop.Domain.Errors;
using MusicShop.Infrastructure.Security;
using MusicShop.Application.DTOs.Auth;

namespace MusicShop.Infrastructure.Services;

public sealed class GoogleAuthService(
    IOptions<GoogleSettings> googleSettings) : IGoogleAuthService
{
    public async Task<Result<GoogleUserPayload>> VerifyTokenAsync(string idToken, CancellationToken cancellationToken)
    {
        try
        {
            GoogleJsonWebSignature.ValidationSettings? settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [googleSettings.Value.ClientId]
            };

            GoogleJsonWebSignature.Payload? payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            if (payload == null)
            {
                return Result<GoogleUserPayload>.Failure(AuthErrors.GoogleInvalidToken);
            }

            return Result<GoogleUserPayload>.Success(new GoogleUserPayload(
                payload.Email,
                payload.Name,
                payload.Subject // Google sub ID
            ));
        }
        catch (InvalidJwtException)
        {
            return Result<GoogleUserPayload>.Failure(AuthErrors.GoogleInvalidToken);
        }
        catch (Exception)
        {
            return Result<GoogleUserPayload>.Failure(AuthErrors.GoogleError);
        }
    }
}

