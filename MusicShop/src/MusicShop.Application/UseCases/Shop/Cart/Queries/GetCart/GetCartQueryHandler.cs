using MediatR;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.Common.Mappings;
using MusicShop.Application.DTOs.Shop;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Orders;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Application.UseCases.Shop.Cart.Queries.GetCart;

/// <summary>
/// Handler for GetCartQuery.
/// Retrieves or creates a shopping cart for the specified user.
/// </summary>
public sealed class GetCartQueryHandler(ICartRepository cartRepository, IUnitOfWork unitOfWork) : IRequestHandler<GetCartQuery, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.Orders.Cart? cart = await cartRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (cart == null)
        {
            // If cart doesn't exist, create an empty one for the user
            Domain.Entities.Orders.Cart newCart = new Domain.Entities.Orders.Cart
            {
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            };

            newCart.UpdatedAt = DateTime.UtcNow;

            cartRepository.Add(newCart);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<CartDto>.Success(newCart.ToDto());
        }

        return Result<CartDto>.Success(cart.ToDto());
    }
}

