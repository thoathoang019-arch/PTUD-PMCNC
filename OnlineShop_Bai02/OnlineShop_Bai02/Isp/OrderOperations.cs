namespace ShopMini.Isp;

public record OrderInfo(int Id, string Customer, string Status, decimal Total);

// ---------- ❌ VI PHAM ISP ----------
// Fat interface gom ca thao tac cua KHACH (doc) lan ADMIN (xoa/doi trang thai).
public interface IOrderServiceFat
{
    OrderInfo GetById(int id);
    IEnumerable<OrderInfo> GetMyOrders();
    void DeleteOrder(int id);                    // chi Admin
    OrderInfo UpdateStatus(int id, string status); // chi Admin
}

// Service cho KHACH bi ep implement ca ham cua Admin -> phai nem NotSupportedException.
public class CustomerOrderServiceBad : IOrderServiceFat
{
    private static readonly List<OrderInfo> _data = new()
    { new(1, "Le Van C", "Confirmed", 300000), new(2, "Tran Thi B", "Shipping", 890000) };

    public OrderInfo GetById(int id) => _data.First(o => o.Id == id);
    public IEnumerable<OrderInfo> GetMyOrders() => _data;
    public void DeleteOrder(int id) => throw new NotSupportedException("Khach hang khong duoc xoa don!");
    public OrderInfo UpdateStatus(int id, string status) => throw new NotSupportedException("Khach hang khong duoc doi trang thai!");
}

// ---------- ✅ AP DUNG ISP: tach 2 interface nho, chuyen biet ----------
public interface IOrderReadService     // cho KHACH
{
    OrderInfo GetById(int id);
    IEnumerable<OrderInfo> GetMyOrders();
}
public interface IOrderManageService   // cho ADMIN
{
    void DeleteOrder(int id);
    OrderInfo UpdateStatus(int id, string status);
}

// Kho don hang dung chung (mock data thuc te).
public class OrderStore
{
    public readonly List<OrderInfo> Orders = new()
    { new(1, "Le Van C", "Confirmed", 300000), new(2, "Tran Thi B", "Shipping", 890000) };
}

public class CustomerOrderService : IOrderReadService   // chi implement thao tac doc
{
    private readonly OrderStore _store;
    public CustomerOrderService(OrderStore store) => _store = store;
    public OrderInfo GetById(int id) =>
        _store.Orders.FirstOrDefault(o => o.Id == id)
        ?? throw new KeyNotFoundException($"Khong tim thay don #{id}");
    public IEnumerable<OrderInfo> GetMyOrders() => _store.Orders;
}

public class AdminOrderService : IOrderManageService    // chi implement thao tac quan ly
{
    private readonly OrderStore _store;
    public AdminOrderService(OrderStore store) => _store = store;
    public void DeleteOrder(int id) => _store.Orders.RemoveAll(o => o.Id == id);
    public OrderInfo UpdateStatus(int id, string status)
    {
        var idx = _store.Orders.FindIndex(o => o.Id == id);
        if (idx < 0) throw new KeyNotFoundException($"Khong tim thay don #{id}");
        _store.Orders[idx] = _store.Orders[idx] with { Status = status };
        return _store.Orders[idx];
    }
}
