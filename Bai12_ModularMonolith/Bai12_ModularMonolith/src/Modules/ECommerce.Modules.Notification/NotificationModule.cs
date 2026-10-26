using ECommerce.Modules.Notification.Application;
using ECommerce.Shared.Events;
using ECommerce.Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Modules.Notification;

public static class NotificationModule
{
    public static IServiceCollection AddNotificationModule(this IServiceCollection services)
    {
        // Subscribe event OrderCreated -> gửi email (consumer độc lập với Inventory)
        services.AddIntegrationEventHandler<OrderCreatedEvent, NotificationOrderCreatedHandler>("notification");
        return services;
    }
}
