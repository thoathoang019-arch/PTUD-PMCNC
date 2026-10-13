namespace ShopMini.Dip;

// ===== Abstraction =====
public interface INotificationSender
{
    string Channel { get; }                       // ten kenh dat qua PROPERTY (khong dung GetType().Name)
    string Send(string to, string message);
}
public interface IOrderStore
{
    int Save(string customer, decimal total);     // gia lap luu CSDL, tra ve Id don
}

// ===== Chi tiet (implementations) =====
public class EmailSender : INotificationSender
{
    public string Channel => "Email";
    public string Send(string to, string message) => $"[EMAIL] -> {to}: {message}";
}
public class SmsSender : INotificationSender
{
    public string Channel => "SMS";
    public string Send(string to, string message) => $"[SMS] -> {to}: {message}";
}
public class InMemoryOrderStore : IOrderStore
{
    private int _seq;
    public int Save(string customer, decimal total) => ++_seq;
}

// ✅ AP DUNG DIP — module cap cao phu thuoc ABSTRACTION, nhan qua constructor (DI).
// Khong new class cu the, khong dung GetType().Name.
public class OrderPlacementService
{
    private readonly IOrderStore _store;
    private readonly INotificationSender _notifier;
    public OrderPlacementService(IOrderStore store, INotificationSender notifier)
    { _store = store; _notifier = notifier; }

    public object PlaceOrder(string customer, string contact, decimal total)
    {
        var id = _store.Save(customer, total);
        var sent = _notifier.Send(contact, $"Don #{id} tong {total:N0}d da duoc tao.");
        return new { orderId = id, channel = _notifier.Channel, notification = sent };
    }
}

// ❌ VI PHAM DIP — tu new class cu the + dung GetType().Name (cach lam khong tot).
public class BadOrderPlacementService
{
    private readonly EmailSender _email = new();          // hardcode chi tiet
    private readonly InMemoryOrderStore _store = new();   // hardcode chi tiet

    public object PlaceOrder(string customer, string contact, decimal total)
    {
        var id = _store.Save(customer, total);
        var sent = _email.Send(contact, $"Don #{id} tong {total:N0}d da duoc tao.");
        return new
        {
            orderId = id,
            channel = _email.GetType().Name,   // <-- cach cu: lay ten class con -> ket noi chat
            notification = sent
        };
    }
}
