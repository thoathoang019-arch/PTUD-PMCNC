using ECommerce.Shared.Messaging;

namespace ECommerce.Shared.Events;

// ============================================================
// Event phát ra khi Order tạo đơn thành công.
// Nhiều module subscribe event này (fanout): Inventory trừ kho, Notification gửi email,
// Promotion cập nhật lượt dùng mã... mỗi module xử lý độc lập.
// ============================================================
public record OrderCreatedEvent : IntegrationEvent
{
    public override string EventName => "OrderCreated";

    public int OrderId { get; init; }
    public string CustomerName { get; init; } = "";
    public string Sku { get; init; } = "";
    public int Quantity { get; init; }
    public decimal Total { get; init; }
    public string? PromotionCode { get; init; }
}
