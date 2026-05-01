using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;

using MusicShop.Application.Behaviors;
using MusicShop.Application.UseCases.Shop.Orders.Commands.UpdateOrderStatus;
using MusicShop.Application.UseCases.Shop.Orders.Commands.UpdateOrderStatus.Actions;

namespace MusicShop.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // 2. Register FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // 3. Order Status Update Strategies
        services.AddScoped<IOrderStatusAction, ConfirmOrderAction>();
        services.AddScoped<IOrderStatusAction, CancelOrderAction>();
        services.AddScoped<IOrderStatusAction, FulfillmentAction>();

        return services;
    }
}
