using ShopMini.Models;
using ShopMini.Ocp;

namespace ShopMini.Srp;

// ✅ AP DUNG SRP - chi DIEU PHOI dat hang: uy thac luu cho ShopData,
// tinh phi cho ShippingFeeService, gui mail cho EmailService.
public class OrderService
{
    private readonly ShopData _data;
    private readonly ShippingFeeService _shipping;
    private readonly EmailService _email;

    public OrderService(ShopData data, ShippingFeeService shipping, EmailService email)
    {
        _data = data;
        _shipping = shipping;
        _email = email;
    }

    public Order PlaceOrder(PlaceOrderRequest req)
    {
        var product = _data.FindProduct(req.ProductId)
            ?? throw new KeyNotFoundException($"Khong tim thay san pham co Id = {req.ProductId}");

        if (product.Stock < req.Quantity)
            throw new InvalidOperationException(
                $"San pham '{product.Name}' khong du ton kho (con {product.Stock}, can {req.Quantity}).");

        var itemsAmount = product.Price * req.Quantity;
        var fee = _shipping.CalculateFee(req.ShippingMethod, itemsAmount);   // OCP

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

        product.Stock -= req.Quantity;                 // tru ton kho
        _email.SendOrderConfirmation(req.Email, order); // gui mail
        return order;
    }
}
