using ShopMini.Models;

namespace ShopMini.Srp;

// ❌ VI PHAM SRP - 1 class om 5 trach nhiem:
// validate + switch-case tinh phi + luu du lieu + dinh dang hoa don + ghi log.
public class BadOrderManager
{
    private readonly ShopData _data;
    public BadOrderManager(ShopData data) => _data = data;

    public object PlaceOrderBad(PlaceOrderRequest req)
    {
        // 1) Validate
        if (string.IsNullOrWhiteSpace(req.CustomerName))
            throw new ArgumentException("[SRP] Ten khach hang khong duoc de trong.");

        var product = _data.FindProduct(req.ProductId)
            ?? throw new KeyNotFoundException($"[SRP] Khong tim thay san pham Id = {req.ProductId}");
        if (product.Stock < req.Quantity)
            throw new InvalidOperationException($"[SRP] San pham '{product.Name}' khong du ton kho.");

        var itemsAmount = product.Price * req.Quantity;

        // 2) Business logic - tu tinh phi bang switch-case
        decimal fee = req.ShippingMethod switch
        {
            ShippingMethod.Standard => itemsAmount >= 500_000m ? 0m : 15_000m,
            ShippingMethod.Express => 30_000m,
            ShippingMethod.SameDay => 50_000m,
            ShippingMethod.International => 200_000m,
            _ => throw new InvalidOperationException("[SRP] Phuong thuc van chuyen khong ho tro.")
        };

        // 3) Persistence
        var order = _data.AddOrder(new Order
        {
            CustomerName = req.CustomerName.Trim(),
            ProductName = product.Name,
            Quantity = req.Quantity,
            ShippingMethod = req.ShippingMethod,
            ItemsAmount = itemsAmount,
            ShippingFee = fee,
            Total = itemsAmount + fee,
            CreatedAt = DateTime.Now
        });
        product.Stock -= req.Quantity;

        // 4) Formatting - tu dinh dang hoa don
        string receipt =
            "=== HOA DON (VI PHAM SRP) ===\n" +
            $"Don #{order.Id} - {order.CustomerName}\n" +
            $"{order.ProductName} x{order.Quantity}\n" +
            $"Tien hang: {order.ItemsAmount:N0} | Ship: {order.ShippingFee:N0} | Tong: {order.Total:N0} VND";

        // 5) Logging
        Console.WriteLine($"[LOG BAD SRP] Tao don #{order.Id} luc {DateTime.Now:HH:mm:ss}");
        Console.WriteLine(receipt);

        return new
        {
            order.Id, order.CustomerName, order.ProductName, order.Total,
            Receipt = receipt,
            Method = "BadOrderManager (VI PHAM SRP)",
            Reason = "1 class om 5 trach nhiem: Validate + Switch-case + Luu + Dinh dang + Log"
        };
    }
}
