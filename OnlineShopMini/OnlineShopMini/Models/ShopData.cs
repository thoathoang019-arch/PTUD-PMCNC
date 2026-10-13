using ShopMini.Models;

namespace ShopMini;

// Kho du lieu in-memory dung chung (Singleton), seed san du lieu that.
// Thay cho ca mot tang DataAccess + Repository -> gon cho demo.
public class ShopData
{
    public List<Product> Products { get; } = new();
    public List<Order> Orders { get; } = new();
    private int _nextOrderId = 1;

    public ShopData()
    {
        Products.AddRange(new[]
        {
            new Product { Id = 1, Name = "Ao thun cotton",       Price = 150_000m, Stock = 20 },
            new Product { Id = 2, Name = "Tai nghe Bluetooth",   Price = 450_000m, Stock = 10 },
            new Product { Id = 3, Name = "Ban phim co",          Price = 890_000m, Stock = 8  },
            new Product { Id = 4, Name = "Chuot khong day",      Price = 220_000m, Stock = 15 },
            new Product { Id = 5, Name = "Sach Clean Code (TV)", Price = 180_000m, Stock = 30 },
        });
    }

    public Product? FindProduct(int id) => Products.FirstOrDefault(p => p.Id == id);

    public Order AddOrder(Order order)
    {
        order.Id = _nextOrderId++;
        Orders.Add(order);
        return order;
    }
}
