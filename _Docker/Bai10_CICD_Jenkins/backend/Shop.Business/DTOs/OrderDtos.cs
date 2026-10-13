using System.ComponentModel.DataAnnotations;
using Shop.Business.Enums;

namespace Shop.Business.DTOs;

public class PlaceOrderRequestDto
{
    [Required(ErrorMessage = "Ten khach hang khong duoc de trong")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Ten khach hang phai tu 2 den 100 ky tu")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email khong duoc de trong")]
    [EmailAddress(ErrorMessage = "Email khong hop le")]
    public string Email { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "ProductId phai lon hon 0")]
    public int ProductId { get; set; }

    [Range(1, 1000, ErrorMessage = "So luong phai tu 1 den 1000")]
    public int Quantity { get; set; } = 1;

    public ShippingMethod ShippingMethod { get; set; } = ShippingMethod.Standard;
}

public class OrderDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string ShippingMethod { get; set; } = string.Empty;
    public decimal ItemsAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
