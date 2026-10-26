using Shop.Business.Enums;

namespace Shop.Business.Entities;

// Entity: chi mo ta du lieu don hang.
public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public ShippingMethod ShippingMethod { get; set; }
    public decimal ItemsAmount { get; set; }   // tien hang = don gia * so luong
    public decimal ShippingFee { get; set; }    // phi van chuyen
    public decimal Total { get; set; }          // tong = tien hang + phi ship
    public string Status { get; set; } = "Confirmed";
    public DateTime CreatedAt { get; set; }
}
