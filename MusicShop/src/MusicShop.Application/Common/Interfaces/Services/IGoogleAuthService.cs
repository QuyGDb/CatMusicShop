using MusicShop.Application.DTOs.Auth;
using MusicShop.Domain.Common;

namespace MusicShop.Application.Common.Interfaces.Services;

public interface IGoogleAuthService
{
    Task<Result<GoogleUserPayload>> VerifyTokenAsync(string idToken, CancellationToken cancellationToken);
}
