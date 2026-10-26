using Shop.Business.Enums;

namespace Shop.Business.Strategies;

public class StandardShippingStrategy : IShippingFeeStrategy
{
    public ShippingMethod SupportedMethod => ShippingMethod.Standard;

    // Giao thuong: 15.000d, mien phi neu don hang tu 500.000d tro len.
    public decimal CalculateFee(decimal itemsAmount) => itemsAmount >= 500_000m ? 0m : 15_000m;
}
