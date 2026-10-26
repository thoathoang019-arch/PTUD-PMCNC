namespace ECommerce.Shared.Contracts;

// ============================================================
// HỢP ĐỒNG module Inventory expose cho module khác (ĐƯỜNG CẮT đồng bộ).
// Order gọi IInventoryService để kiểm tra tồn kho — KHÔNG đụng vào store/DbContext của Inventory.
//
// Khi tách Inventory thành microservice: giữ nguyên interface này, chỉ thay phần CÀI ĐẶT
// bằng một client gọi REST/gRPC qua network (xem InventoryHttpClient trong module Order).
// ============================================================
public interface IInventoryService
{
    Task<StockCheckResult> CheckStockAsync(string sku, int quantity, CancellationToken ct = default);
}

public sealed record StockCheckResult(string Sku, bool IsAvailable, int Available, int Requested)
{
    public static StockCheckResult Ok(string sku, int available, int requested) => new(sku, true, available, requested);
    public static StockCheckResult NotEnough(string sku, int available, int requested) => new(sku, false, available, requested);
}
