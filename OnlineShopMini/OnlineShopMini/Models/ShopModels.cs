using System.ComponentModel.DataAnnotations;

namespace ShopMini.Models;

// Phuong thuc van chuyen. International = loai MO RONG (minh hoa OCP).
public enum ShippingMethod
{
    Standard = 1,       // Giao thuong
    Express = 2,        // Giao nhanh
    SameDay = 3,        // Giao trong ngay
    International = 4    // Giao quoc te (loai mo rong)
}

// SRP: entity chi giu du lieu.
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = "";
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public ShippingMethod ShippingMethod { get; set; }
    public decimal ItemsAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Du lieu client gui len khi dat hang (co rang buoc de xu ly loi co ban).
public class PlaceOrderRequest
{
    [Required, StringLength(100, MinimumLength = 2, ErrorMessage = "Ten khach hang phai tu 2 den 100 ky tu")]
    public string CustomerName { get; set; } = "";

    [Required, EmailAddress(ErrorMessage = "Email khong hop le")]
    public string Email { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "ProductId phai lon hon 0")]
    public int ProductId { get; set; }

    [Range(1, 1000, ErrorMessage = "So luong phai tu 1 den 1000")]
    public int Quantity { get; set; } = 1;

    public ShippingMethod ShippingMethod { get; set; } = ShippingMethod.Standard;
}
