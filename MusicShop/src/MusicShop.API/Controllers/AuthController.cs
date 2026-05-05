using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using MusicShop.Application.DTOs.Auth;
using MusicShop.Application.UseCases.Auth.Commands.TokenRefresh;
using MusicShop.Application.UseCases.Auth.Commands.Register;
using MusicShop.Application.UseCases.Auth.Commands.Logout;
using MusicShop.Application.UseCases.Auth.Commands.ChangePassword;
using MusicShop.Application.UseCases.Auth.Queries.Login;
using MusicShop.Application.UseCases.Auth.Queries.GetMe;
using MusicShop.Application.UseCases.Auth.Commands.GoogleLogin;
using MusicShop.API.Infrastructure;
using MusicShop.Domain.Common;
using MusicShop.API.Controllers.Base;

namespace MusicShop.API.Controllers;

public class AuthController(IMediator mediator, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env) : BaseApiController
{
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterCommand command)
    {
        Result<AuthResponse> result = await mediator.Send(command);

        if (result.IsSuccess)
        {
            SetRefreshTokenCookie(result.Value.RefreshToken);
        }

        return HandleResult(result);
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginQuery query)
    {
        Result<AuthResponse> result = await mediator.Send(query);

        if (result.IsSuccess)
        {
            SetRefreshTokenCookie(result.Value.RefreshToken);
        }

        return HandleResult(result);
    }

    [HttpPost("google-login")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> GoogleLogin([FromBody] GoogleLoginCommand command)
    {
        Result<AuthResponse> result = await mediator.Send(command);

        if (result.IsSuccess)
        {
            SetRefreshTokenCookie(result.Value.RefreshToken);
        }

        return HandleResult(result);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken()
    {
        string refreshToken = Request.Cookies["refreshToken"] ?? string.Empty;

        Result<AuthResponse> result = await mediator.Send(new RefreshTokenCommand(refreshToken));

        if (result.IsSuccess)
        {
            SetRefreshTokenCookie(result.Value.RefreshToken);
        }

        return HandleResult(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetMe()
    {
        Result<UserResponse> result = await mediator.Send(new GetMeQuery());
        return HandleResult(result);
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<Unit>>> Logout()
    {
        string refreshToken = Request.Cookies["refreshToken"] ?? string.Empty;

        Result<Unit> result = await mediator.Send(new LogoutCommand(refreshToken));

        if (result.IsSuccess)
        {
            Response.Cookies.Delete("refreshToken");
        }

        return HandleResult(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<Unit>>> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        Result<Unit> result = await mediator.Send(command);
        return HandleResult(result);
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        CookieOptions cookieOptions = new()
        {
            HttpOnly = true,
            Secure = !env.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

}
