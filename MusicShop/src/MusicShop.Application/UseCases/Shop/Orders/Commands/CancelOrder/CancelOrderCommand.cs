using MediatR;

namespace MusicShop.Application.UseCases.Shop.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId) : IRequest<MusicShop.Domain.Common.Result>;
