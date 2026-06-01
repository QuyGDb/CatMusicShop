using MediatR;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Orders;
using MusicShop.Domain.Enums;
using MusicShop.Domain.Errors;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Application.UseCases.Shop.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusCommandHandler(
    IOrderRepository orderRepository,
    IEnumerable<IOrderStatusAction> actions,
    IEmailService emailService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateOrderStatusCommand, Result>
{
    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetByIdWithDetailsAsync(request.OrderId, cancellationToken);

        if (order == null)
        {
            return Result.Failure(OrderErrors.NotFound);
        }

        OrderStatus fromStatus = order.Status;
        OrderStatus targetStatus = request.Status;

        if (fromStatus == targetStatus)
        {
            return Result.Success();
        }

        // 1. Identify and execute specialized transition actions
        IOrderStatusAction? action = actions.FirstOrDefault(a => a.CanHandle(fromStatus, targetStatus));
        if (action != null)
        {
            await action.ExecuteAsync(order, request, cancellationToken);
        }

        // 2. Validate and perform the state transition in Domain
        Result transitionResult = order.TransitionTo(targetStatus);
        if (transitionResult.IsFailure)
        {
            return transitionResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 3. Send Email Notification
        try
        {
            await SendStatusUpdateEmailAsync(order, cancellationToken);
        }
        catch (Exception)
        {
            // Suppress notification failures to prevent breaking the core flow
        }

        return Result.Success();
    }

    private async Task SendStatusUpdateEmailAsync(Order order, CancellationToken cancellationToken)
    {
        string subject = $"Order #{order.Id.ToString().ToUpper()[..8]} Status Update";
        string statusText = order.Status.ToString();
        string body = $@"
            <div style='font-family: sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                <h2 style='color: #333;'>Hello {order.RecipientName},</h2>
                <p>Your order <strong>#{order.Id}</strong> has been updated to: <span style='color: #4f46e5; font-weight: bold;'>{statusText}</span></p>
                
                {(order.Status == OrderStatus.Shipped && !string.IsNullOrEmpty(order.TrackingNumber)
                    ? $"<p>Your tracking number is: <strong>{order.TrackingNumber}</strong></p>"
                    : "")}
                
                <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;' />
                <p style='font-size: 12px; color: #666;'>Thank you for shopping with MusicShop!</p>
            </div>";

        await emailService.SendEmailAsync(order.Email, subject, body);
    }
}

