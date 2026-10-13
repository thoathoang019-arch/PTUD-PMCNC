using Shop.Business.Entities;
using Shop.Business.Enums;

namespace Shop.DataAccess.Persistence;

// Kho du lieu in-memory dung chung toan ung dung (singleton).
// Seed san "du lieu that" de demo co du lieu ngay khi chay.
public class InMemoryDataStore
{
    private readonly object _lock = new();
    private readonly List<Product> _products = new();
    private readonly List<Order> _orders = new();
    private int _nextOrderId = 1;

    public InMemoryDataStore()
    {
        Seed();
    }

    private void Seed()
    {
        _products.AddRange(new[]
        {
            new Product { Id = 1, Name = "Ao thun cotton",        Price = 150_000m, Stock = 20 },
            new Product { Id = 2, Name = "Tai nghe Bluetooth",    Price = 450_000m, Stock = 10 },
            new Product { Id = 3, Name = "Ban phim co",           Price = 890_000m, Stock = 8  },
            new Product { Id = 4, Name = "Chuot khong day",       Price = 220_000m, Stock = 15 },
            new Product { Id = 5, Name = "Sach Clean Code (TV)",  Price = 180_000m, Stock = 30 },
            new Product { Id = 6, Name = "Balo laptop",           Price = 320_000m, Stock = 12 },
        });

        // Vai don hang mau (du lieu that de GET /api/orders co ket qua ngay)
        var now = DateTime.Now;
        _orders.Add(new Order
        {
            Id = _nextOrderId++, CustomerName = "Nguyen Van An", ProductId = 2, ProductName = "Tai nghe Bluetooth",
            Quantity = 1, ShippingMethod = ShippingMethod.Express, ItemsAmount = 450_000m,
            ShippingFee = 30_000m, Total = 480_000m, Status = "Confirmed", CreatedAt = now.AddDays(-2)
        });
        _orders.Add(new Order
        {
            Id = _nextOrderId++, CustomerName = "Tran Thi Binh", ProductId = 3, ProductName = "Ban phim co",
            Quantity = 1, ShippingMethod = ShippingMethod.Standard, ItemsAmount = 890_000m,
            ShippingFee = 0m, Total = 890_000m, Status = "Confirmed", CreatedAt = now.AddDays(-1)
        });
    }

    public IEnumerable<Product> GetProducts()
    {
        lock (_lock) { return _products.ToList(); }
    }

    public Product? GetProduct(int id)
    {
        lock (_lock) { return _products.FirstOrDefault(p => p.Id == id); }
    }

    public IEnumerable<Order> GetOrders()
    {
        lock (_lock) { return _orders.ToList(); }
    }

    public Order? GetOrder(int id)
    {
        lock (_lock) { return _orders.FirstOrDefault(o => o.Id == id); }
    }

    public void AddOrder(Order order)
    {
        lock (_lock)
        {
            order.Id = _nextOrderId++;
            _orders.Add(order);
        }
    }
}
