using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Shared.Messaging;

// ============================================================
// Helper để mỗi module ĐĂNG KÝ consumer của mình một cách gọn gàng.
//   services.AddIntegrationEventHandler<OrderCreatedEvent, InventoryOrderCreatedHandler>("inventory");
// Đăng ký này được bus (in-process / RabbitMQ) đọc lại lúc khởi động để tạo subscription.
// ============================================================
public static class EventBusRegistration
{
    public static IServiceCollection AddIntegrationEventHandler<TEvent, THandler>(
        this IServiceCollection services, string consumerName)
        where TEvent : IntegrationEvent
        where THandler : class, IIntegrationEventHandler<TEvent>
    {
        services.AddScoped<THandler>();
        services.AddSingleton(new EventSubscription(consumerName, typeof(TEvent), typeof(THandler)));
        return services;
    }
}
