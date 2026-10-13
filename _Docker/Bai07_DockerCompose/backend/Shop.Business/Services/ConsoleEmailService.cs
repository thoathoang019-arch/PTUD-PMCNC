using Shop.Business.Entities;
using Shop.Business.Interfaces;
using Microsoft.Extensions.Logging;

namespace Shop.Business.Services;

// ✅ SRP - Chi lo gui email. O demo ta "gui" bang cach ghi log (gia lap SMTP).
public class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;

    public ConsoleEmailService(ILogger<ConsoleEmailService> logger) => _logger = logger;

    public void SendOrderConfirmation(string toEmail, Order order)
    {
        _logger.LogInformation(
            "[EMAIL] Gui toi {Email}: Don hang #{OrderId} da xac nhan. Tong tien {Total:N0} VND.",
            toEmail, order.Id, order.Total);
    }
}
