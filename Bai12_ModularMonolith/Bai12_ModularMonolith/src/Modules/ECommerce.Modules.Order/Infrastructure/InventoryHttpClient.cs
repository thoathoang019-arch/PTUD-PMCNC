using System.Net.Http.Json;
using ECommerce.Shared.Contracts;

namespace ECommerce.Modules.Order.Infrastructure;

// ============================================================
// Biến thể GỌI QUA NETWORK của IInventoryService (REST).
// Khi Inventory tách thành microservice, chỉ cần đổi host nối IInventoryService
// từ InventoryService (in-process) sang InventoryHttpClient (HTTP) — OrderService
// KHÔNG đổi một dòng nào. Đây chính là "đường cắt" trong thực tế.
// ============================================================
public sealed class InventoryHttpClient : IInventoryService
{
    private readonly HttpClient _http;
    public InventoryHttpClient(HttpClient http) => _http = http;

    public async Task<StockCheckResult> CheckStockAsync(string sku, int quantity, CancellationToken ct = default)
    {
        var url = $"api/inventory/check?sku={Uri.EscapeDataString(sku)}&qty={quantity}";
        var result = await _http.GetFromJsonAsync<StockCheckResult>(url, ct);
        return result ?? StockCheckResult.NotEnough(sku, 0, quantity);
    }
}
