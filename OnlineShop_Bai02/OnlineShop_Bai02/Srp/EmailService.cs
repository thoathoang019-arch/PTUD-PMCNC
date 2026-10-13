using ShopMini.Models;

namespace ShopMini.Srp;

// ✅ SRP - chi lo gui email (gia lap bang ghi log).
public class EmailService
{
    private readonly ILogger<EmailService> _logger;
    public EmailService(ILogger<EmailService> logger) => _logger = logger;

    public void SendOrderConfirmation(string email, Order order) =>
        _logger.LogInformation("[EMAIL] Gui toi {Email}: Don #{Id} da xac nhan, tong {Total:N0} VND.",
            email, order.Id, order.Total);
}
