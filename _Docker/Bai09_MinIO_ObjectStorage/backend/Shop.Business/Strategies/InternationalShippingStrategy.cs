using Shop.Business.Enums;

namespace Shop.Business.Strategies;

// Loai MO RONG: chi can them class nay + dang ky DI, KHONG sua code cu -> OCP.
public class InternationalShippingStrategy : IShippingFeeStrategy
{
    public ShippingMethod SupportedMethod => ShippingMethod.International;

    // Giao quoc te: 200.000d.
    public decimal CalculateFee(decimal itemsAmount) => 200_000m;
}
