using MediatR;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Common;
using MusicShop.Domain.Errors;
using MusicShop.Domain.Entities.Shop;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Application.UseCases.Shop.Cart.Commands.UpdateCartItem;

/// <summary>
/// Handler for UpdateCartItemCommand.
/// Validates stock before updating quantity.
/// </summary>
public sealed class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, Result>
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCartItemCommandHandler(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        // 1. Get tracked cart
        Domain.Entities.Orders.Cart? cart = await _cartRepository.GetByUserIdForUpdateAsync(request.UserId, cancellationToken);
        if (cart == null)
        {
            return Result.Failure(CartErrors.NotFound);
        }

        // 2. Find item
        Domain.Entities.Orders.CartItem? cartItem = cart.Items.FirstOrDefault(item => item.Id == request.CartItemId);
        if (cartItem == null)
        {
            return Result.Failure(CartErrors.ItemNotFound);
        }

        // 3. Handle deletion if quantity is 0 or less
        if (request.NewQuantity <= 0)
        {
            cart.Items.Remove(cartItem);
        }
        else
        {
            // 4. Validate Stock
            Product? product = await _productRepository.FirstOrDefaultAsync(p => p.Id == cartItem.ProductId, cancellationToken);
            if (product == null)
            {
                return Result.Failure(ProductErrors.NotFound);
            }

            if (!product.IsAvailable || product.StockQty < request.NewQuantity)
            {
                return Result.Failure(CartErrors.InsufficientStock);
            }

            cartItem.Quantity = request.NewQuantity;
        }

        cart.UpdatedAt = DateTime.UtcNow;

        // 5. Save Changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

