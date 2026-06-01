using MediatR;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.System;
using MusicShop.Domain.Errors;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Application.UseCases.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler(
    IRepository<User> userRepository,
    IPasswordHasher passwordHasher,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : IRequestHandler<ChangePasswordCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || string.IsNullOrEmpty(currentUserService.UserId))
        {
            return Result<Unit>.Failure(AuthErrors.Unauthorized);
        }

        if (!Guid.TryParse(currentUserService.UserId, out Guid userId))
        {
            return Result<Unit>.Failure(AuthErrors.InvalidUserId);
        }

        User? user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return Result<Unit>.Failure(AuthErrors.UserNotFound);
        }

        if (user.PasswordHash == null || !passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return Result<Unit>.Failure(AuthErrors.InvalidCredentials);
        }

        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}

