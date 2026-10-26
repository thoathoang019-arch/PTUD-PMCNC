using ECommerce.Shared.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Infrastructure.Messaging;

// ============================================================
// Chọn bus theo cấu hình "Messaging:Mode":
//   - "InProcess" (mặc định): chạy ngay, không cần broker. Hỗ trợ pause/resume để demo.
//   - "RabbitMq"            : dùng RabbitMQ thật (cần broker, xem docker-compose.yml).
// ============================================================
public static class MessagingRegistration
{
    public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration config)
    {
        var mode = config["Messaging:Mode"] ?? "InProcess";

        if (string.Equals(mode, "RabbitMq", StringComparison.OrdinalIgnoreCase))
        {
            services.Configure<RabbitMqOptions>(config.GetSection("Messaging:RabbitMq"));
            services.AddSingleton<RabbitMqEventBus>();
            services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<RabbitMqEventBus>());
            services.AddHostedService(sp => sp.GetRequiredService<RabbitMqEventBus>());
        }
        else
        {
            services.AddSingleton<InProcessEventBus>();
            services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<InProcessEventBus>());
            services.AddSingleton<IConsumerControl>(sp => sp.GetRequiredService<InProcessEventBus>());
            services.AddHostedService(sp => sp.GetRequiredService<InProcessEventBus>());
        }
        return services;
    }
}
