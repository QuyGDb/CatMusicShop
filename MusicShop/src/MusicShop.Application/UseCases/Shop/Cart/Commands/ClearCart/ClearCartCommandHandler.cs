using MediatR;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Common;

namespace MusicShop.Application.UseCases.Shop.Cart.Commands.ClearCart;

public sealed class ClearCartCommandHandler(
    ICartRepository cartRepository,
    ICurrentUserService currentUserService) : IRequestHandler<ClearCartCommand, Result>
{
    public async Task<Result> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        Guid userId = Guid.Parse(currentUserService.UserId!);
        await cartRepository.ClearCartAsync(userId, cancellationToken);
        
        return Result.Success();
    }
}

