using MediatR;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.Common.Mappings;
using MusicShop.Application.DTOs.Auth;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.System;
using MusicShop.Domain.Errors;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Application.UseCases.Auth.Queries.GetMe;

public class GetMeQueryHandler(
    IRepository<User> userRepository,
    ICurrentUserService currentUserService) : IRequestHandler<GetMeQuery, Result<UserResponse>>
{
    public async Task<Result<UserResponse>> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || string.IsNullOrEmpty(currentUserService.UserId))
        {
            return Result<UserResponse>.Failure(AuthErrors.Unauthorized);
        }

        if (!Guid.TryParse(currentUserService.UserId, out Guid userId))
        {
            return Result<UserResponse>.Failure(AuthErrors.InvalidUserId);
        }

        User? user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return Result<UserResponse>.Failure(AuthErrors.UserNotFound);
        }

        return Result<UserResponse>.Success(user.ToResponse());
    }
}

