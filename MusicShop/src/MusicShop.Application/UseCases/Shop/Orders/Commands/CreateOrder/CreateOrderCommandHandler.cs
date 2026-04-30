using MediatR;
using MusicShop.Application.Common.Interfaces;
using MusicShop.Application.DTOs.Shop;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Orders;
using MusicShop.Domain.Enums;
using MusicShop.Domain.Errors;
using MusicShop.Domain.Interfaces;
using MusicShop.Domain.Entities.Shop;

namespace MusicShop.Application.UseCases.Shop.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler(
    ICartRepository cartRepository,
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IStripeService stripeService) : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponse>>
{
    public async Task<Result<CreateOrderResponse>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(currentUserService.UserId, out Guid userId))
        {
            return Result<CreateOrderResponse>.Failure(AuthErrors.Unauthorized);
        }

        // 1. Fetch cart with items
        Domain.Entities.Orders.Cart? cart = await cartRepository.GetByUserIdAsync(userId, cancellationToken);

        if (cart == null || cart.Items.Count == 0)
        {
            return Result<CreateOrderResponse>.Failure(OrderErrors.CartEmpty);
        }

        // 2. Fetch all products at once (Fix N+1 Query)
        List<Guid> productIds = cart.Items.Select(item => item.ProductId).ToList();
        IReadOnlyList<Product> products = await productRepository.GetByIdsAsync(productIds, cancellationToken);
        Dictionary<Guid, Product> productMap = products.ToDictionary(product => product.Id);

        // 3. Prepare Order
        Order order = new()
        {
            UserId = userId,
            RecipientName = request.RecipientName,
            Email = request.Email,
            Phone = request.Phone,
            ShippingAddress = request.ShippingAddress,
            Note = request.Note,
            Status = OrderStatus.Pending
        };

        decimal totalAmount = 0;

        foreach (CartItem cartItem in cart.Items)
        {
            if (!productMap.TryGetValue(cartItem.ProductId, out Product? product))
            {
                return Result<CreateOrderResponse>.Failure(ProductErrors.NotFound);
            }

            if (!product.IsPreorder && product.StockQty < cartItem.Quantity)
            {
                return Result<CreateOrderResponse>.Failure(ProductErrors.InsufficientStock);
            }

            // 4. Create OrderItem and Price Snapshot
            OrderItem orderItem = new()
            {
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                PriceSnapshot = product.Price,
                ProductNameSnapshot = product.Name
            };

            order.OrderItems.Add(orderItem);
            totalAmount += product.Price * cartItem.Quantity;
        }

        order.TotalAmount = totalAmount;

        // 5. Initialize Payment
        Payment payment = new()
        {
            Amount = totalAmount,
            Method = request.PaymentGateway,
            Status = PaymentStatus.Pending
        };

        order.Payment = payment;

        orderRepository.Add(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Create Stripe Session
        Result<StripeCheckoutDto> stripeResult = await stripeService.CreateCheckoutSessionAsync(
            order,
            request.SuccessUrl,
            request.CancelUrl,
            cancellationToken);

        if (!stripeResult.IsSuccess)
        {
            return Result<CreateOrderResponse>.Failure(stripeResult.Error);
        }

        string checkoutUrl = stripeResult.Value.Url;

        return Result<CreateOrderResponse>.Success(new CreateOrderResponse(
            new OrderSummaryDto(order.Id, order.Status, order.TotalAmount, order.CreatedAt),
            new PaymentSummaryDto(payment.Id, payment.Method, payment.Status, checkoutUrl)
        ));
    }
}
