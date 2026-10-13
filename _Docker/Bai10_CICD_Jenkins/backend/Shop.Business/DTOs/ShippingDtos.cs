namespace Shop.Business.DTOs;

public class ShippingFeePreviewDto
{
    public string ShippingMethod { get; set; } = string.Empty;
    public decimal ItemsAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public string Method { get; set; } = string.Empty;   // cach tinh (OCP hay Legacy)
    public string Note { get; set; } = string.Empty;
}
