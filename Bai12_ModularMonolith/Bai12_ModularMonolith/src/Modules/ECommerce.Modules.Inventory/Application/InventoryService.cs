using ECommerce.Modules.Inventory.Infrastructure;
using ECommerce.Shared.Contracts;

namespace ECommerce.Modules.Inventory.Application;

// Cài đặt hợp đồng IInventoryService (đường cắt đồng bộ mà Order gọi vào).
public sealed class InventoryService : IInventoryService
{
    private readonly InventoryStore _store;
    public InventoryService(InventoryStore store) => _store = store;

    public Task<StockCheckResult> CheckStockAsync(string sku, int quantity, CancellationToken ct = default)
    {
        var available = _store.Get(sku)?.Available ?? 0;
        var result = available >= quantity
            ? StockCheckResult.Ok(sku, available, quantity)
            : StockCheckResult.NotEnough(sku, available, quantity);
        return Task.FromResult(result);
    }
}
