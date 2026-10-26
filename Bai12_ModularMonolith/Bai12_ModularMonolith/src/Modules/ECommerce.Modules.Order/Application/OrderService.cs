using ECommerce.Modules.Order.Infrastructure;
using ECommerce.Shared.Contracts;
using ECommerce.Shared.Events;
using ECommerce.Shared.Messaging;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Order.Application;

public interface IOrderService
{
    Task<CreateOrderResult> CreateAsync(CreateOrderRequest request, CancellationToken ct = default);
}

// ============================================================
// Luồng đặt hàng — KẾT HỢP đồng bộ + bất đồng bộ:
//   1) ĐỒNG BỘ: hỏi Inventory "còn hàng không" (cần biết ngay để quyết định).
//   2) Tạo đơn trong store riêng của Order.
//   3) BẤT ĐỒNG BỘ: publish OrderCreated cho các module khác tự xử lý (không chờ).
//
// Order chỉ biết IInventoryService (đường cắt) — không biết Inventory cài đặt thế nào,
// cũng không biết in-process hay gọi qua HTTP. Đó là lý do tách microservice sau này dễ.
// ============================================================
public sealed class OrderService : IOrderService
{
    private readonly IInventoryService _inventory;
    private readonly IEventBus _eventBus;
    private readonly OrderStore _store;
    private readonly ILogger<OrderService> _log;

    public OrderService(IInventoryService inventory, IEventBus eventBus, OrderStore store, ILogger<OrderService> log)
    {
        _inventory = inventory;
        _eventBus = eventBus;
        _store = store;
        _log = log;
    }

    public async Task<CreateOrderResult> CreateAsync(CreateOrderRequest req, CancellationToken ct = default)
    {
        // 1) ĐỒNG BỘ: kiểm tra tồn kho trước khi tạo đơn
        var stock = await _inventory.CheckStockAsync(req.Sku, req.Quantity, ct);
        if (!stock.IsAvailable)
        {
            _log.LogWarning("[order] từ chối: {Sku} còn {Have}, cần {Need}", req.Sku, stock.Available, req.Quantity);
            return CreateOrderResult.Rejected(
                $"Hết hàng: {req.Sku} còn {stock.Available}, cần {req.Quantity}.", stock);
        }

        // 2) Tạo đơn trong store riêng
        var order = _store.Create(req.CustomerName, req.Sku, req.Quantity, req.UnitPrice, req.PromotionCode);
        _log.LogInformation("[order] tạo đơn #{Id} cho {Customer}", order.Id, order.CustomerName);

        // 3) BẤT ĐỒNG BỘ: báo cho các module khác (Inventory trừ kho, Notification gửi email...)
        await _eventBus.PublishAsync(new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerName = order.CustomerName,
            Sku = order.Sku,
            Quantity = order.Quantity,
            Total = order.Total,
            PromotionCode = order.PromotionCode
        }, ct);

        return CreateOrderResult.Created(order.Id, order.Total, stock);
    }
}
