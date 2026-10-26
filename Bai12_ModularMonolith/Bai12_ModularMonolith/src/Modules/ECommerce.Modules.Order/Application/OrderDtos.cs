using ECommerce.Shared.Contracts;

namespace ECommerce.Modules.Order.Application;

public sealed record CreateOrderRequest
{
    public string CustomerName { get; init; } = "";
    public string Sku { get; init; } = "";
    public int Quantity { get; init; } = 1;
    public decimal UnitPrice { get; init; }
    public string? PromotionCode { get; init; }
}

public sealed record CreateOrderResult(bool Success, int? OrderId, decimal? Total, string Message, StockCheckResult Stock)
{
    public static CreateOrderResult Created(int id, decimal total, StockCheckResult stock)
        => new(true, id, total, "Tạo đơn thành công.", stock);

    public static CreateOrderResult Rejected(string message, StockCheckResult stock)
        => new(false, null, null, message, stock);
}
