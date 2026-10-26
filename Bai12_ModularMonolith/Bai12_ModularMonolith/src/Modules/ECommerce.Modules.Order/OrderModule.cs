using ECommerce.Modules.Order.Application;
using ECommerce.Modules.Order.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Modules.Order;

public static class OrderModule
{
    public static IServiceCollection AddOrderModule(this IServiceCollection services)
    {
        services.AddSingleton<OrderStore>();              // "database" riêng của module
        services.AddScoped<IOrderService, OrderService>();
        return services;
    }
}
