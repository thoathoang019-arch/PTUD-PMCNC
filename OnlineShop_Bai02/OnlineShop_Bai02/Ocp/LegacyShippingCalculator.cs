using ShopMini.Models;

namespace ShopMini.Ocp;

// ❌ VI PHAM OCP - switch-case. Them phuong thuc moi phai MO class nay ra SUA.
public class LegacyShippingCalculator
{
    public decimal CalculateFee(ShippingMethod method, decimal itemsAmount) => method switch
    {
        ShippingMethod.Standard => itemsAmount >= 500_000m ? 0m : 15_000m,
        ShippingMethod.Express => 30_000m,
        ShippingMethod.SameDay => 50_000m,
        ShippingMethod.International => 200_000m,
        _ => throw new InvalidOperationException($"Chua ho tro phuong thuc van chuyen: {method}")
    };
}
