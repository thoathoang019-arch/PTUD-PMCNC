using ECommerce.Modules.Inventory.Infrastructure;
using ECommerce.Shared.Events;
using ECommerce.Shared.Messaging;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Inventory.Application;

// Consumer BẤT ĐỒNG BỘ: nghe OrderCreated -> trừ tồn kho.
public sealed class InventoryOrderCreatedHandler : IIntegrationEventHandler<OrderCreatedEvent>
{
    private readonly InventoryStore _store;
    private readonly ILogger<InventoryOrderCreatedHandler> _log;

    public InventoryOrderCreatedHandler(InventoryStore store, ILogger<InventoryOrderCreatedHandler> log)
    {
        _store = store;
        _log = log;
    }

    public Task HandleAsync(OrderCreatedEvent e, CancellationToken ct = default)
    {
        var ok = _store.TryDeduct(e.Sku, e.Quantity);
        var left = _store.Get(e.Sku)?.Available ?? 0;
        _log.LogInformation("[inventory] OrderCreated #{Order}: trừ {Qty} {Sku} -> {Result} (còn {Left})",
            e.OrderId, e.Quantity, e.Sku, ok ? "OK" : "THIẾU", left);
        return Task.CompletedTask;
    }
}
