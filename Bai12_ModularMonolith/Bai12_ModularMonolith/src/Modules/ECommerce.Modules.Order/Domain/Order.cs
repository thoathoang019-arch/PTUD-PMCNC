namespace ECommerce.Modules.Order.Domain;

public sealed class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = "";
    public string Sku { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total => UnitPrice * Quantity;
    public string? PromotionCode { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
