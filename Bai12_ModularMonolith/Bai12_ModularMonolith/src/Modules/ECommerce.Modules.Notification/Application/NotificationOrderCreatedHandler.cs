using ECommerce.Shared.Events;
using ECommerce.Shared.Messaging;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Notification.Application;

// Consumer BẤT ĐỒNG BỘ thứ 2: nghe OrderCreated -> gửi email xác nhận (giả lập bằng log).
public sealed class NotificationOrderCreatedHandler : IIntegrationEventHandler<OrderCreatedEvent>
{
    private readonly ILogger<NotificationOrderCreatedHandler> _log;
    public NotificationOrderCreatedHandler(ILogger<NotificationOrderCreatedHandler> log) => _log = log;

    public Task HandleAsync(OrderCreatedEvent e, CancellationToken ct = default)
    {
        _log.LogInformation("[notification] Gửi email xác nhận đơn #{Order} tới {Customer} (tổng {Total:n0} đ)",
            e.OrderId, e.CustomerName, e.Total);
        return Task.CompletedTask;
    }
}
