using System.Collections.Concurrent;
using ECommerce.Modules.Inventory.Domain;

namespace ECommerce.Modules.Inventory.Infrastructure;

// ============================================================
// "Schema"/kho dữ liệu RIÊNG của module Inventory (ở đây dùng in-memory cho gọn).
// Module khác TUYỆT ĐỐI không được chạm vào store này — chỉ gọi qua IInventoryService.
// Khi lên microservice: thay bằng DbContext trỏ tới database riêng của Inventory.
// ============================================================
public sealed class InventoryStore
{
    private readonly ConcurrentDictionary<string, InventoryItem> _items = new(StringComparer.OrdinalIgnoreCase);

    public InventoryStore()
    {
        Seed("SKU-LAPTOP", "Laptop", 5);
        Seed("SKU-PHONE", "Phone", 20);
        Seed("SKU-TABLET", "Tablet", 0);   // hết hàng -> dùng để demo tình huống từ chối đơn
    }

    private void Seed(string sku, string name, int qty)
        => _items[sku] = new InventoryItem { Sku = sku, Name = name, Available = qty };

    public InventoryItem? Get(string sku) => _items.TryGetValue(sku, out var it) ? it : null;

    public IReadOnlyCollection<InventoryItem> All() => _items.Values.ToList();

    /// <summary>Trừ kho an toàn. Trả về true nếu đủ hàng để trừ.</summary>
    public bool TryDeduct(string sku, int qty)
    {
        if (!_items.TryGetValue(sku, out var it)) return false;
        lock (it)
        {
            if (it.Available < qty) return false;
            it.Available -= qty;
            return true;
        }
    }
}
