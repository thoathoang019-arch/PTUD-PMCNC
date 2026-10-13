using Shop.Business.Enums;

namespace Shop.Business.Strategies;

public class SameDayShippingStrategy : IShippingFeeStrategy
{
    public ShippingMethod SupportedMethod => ShippingMethod.SameDay;

    // Giao trong ngay: 50.000d.
    public decimal CalculateFee(decimal itemsAmount) => 50_000m;
}
