using ECommerce.Modules.Inventory.Application;
using ECommerce.Modules.Inventory.Infrastructure;
using ECommerce.Shared.Events;
using ECommerce.Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Modules.Inventory;

public static class InventoryModule
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services)
    {
        services.AddSingleton<InventoryStore>();        // "database" riêng của module
        services.AddScoped<InventoryService>();         // dùng NỘI BỘ module (controller của chính Inventory)

        // Subscribe event OrderCreated -> trừ kho (consumer bất đồng bộ)
        services.AddIntegrationEventHandler<OrderCreatedEvent, InventoryOrderCreatedHandler>("inventory");
        return services;
    }
}

// Lưu ý: hợp đồng IInventoryService (đường cắt cho module KHÁC gọi) được nối ở host/Program.cs
//        — in-process (InventoryService) hoặc qua HTTP (InventoryHttpClient) tuỳ cấu hình.
