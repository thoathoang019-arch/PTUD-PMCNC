using Shop.Business.Enums;

namespace Shop.Business.Strategies;

public class ExpressShippingStrategy : IShippingFeeStrategy
{
    public ShippingMethod SupportedMethod => ShippingMethod.Express;

    // Giao nhanh: 30.000d.
    public decimal CalculateFee(decimal itemsAmount) => 30_000m;
}
