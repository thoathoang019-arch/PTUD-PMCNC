using Shop.Business.Entities;

namespace Shop.Business.Interfaces;

// Trach nhiem DUY NHAT: gui email. Khong biet gi ve database.
public interface IEmailService
{
    void SendOrderConfirmation(string toEmail, Order order);
}
