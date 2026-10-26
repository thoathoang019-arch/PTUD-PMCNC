using System.Collections.Concurrent;
using ECommerce.Modules.Order.Domain;

namespace ECommerce.Modules.Order.Infrastructure;

// "Schema"/kho dữ liệu RIÊNG của module Order. Module khác không chạm vào đây.
public sealed class OrderStore
{
    private readonly ConcurrentDictionary<int, Order> _orders = new();
    private int _seq;

    public Order Create(string customer, string sku, int qty, decimal unitPrice, string? promo)
    {
        var id = Interlocked.Increment(ref _seq);
        var order = new Order
        {
            Id = id,
            CustomerName = customer,
            Sku = sku,
            Quantity = qty,
            UnitPrice = unitPrice,
            PromotionCode = promo
        };
        _orders[id] = order;
        return order;
    }

    public IReadOnlyCollection<Order> All() => _orders.Values.OrderBy(o => o.Id).ToList();
    public Order? Get(int id) => _orders.TryGetValue(id, out var o) ? o : null;
}
