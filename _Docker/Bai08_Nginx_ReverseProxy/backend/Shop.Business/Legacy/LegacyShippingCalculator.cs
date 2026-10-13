using Shop.Business.Enums;

namespace Shop.Business.Legacy;

// ❌ VI PHAM OCP - Tinh phi ship bang switch-case theo tung phuong thuc.
// Moi khi them phuong thuc van chuyen moi phai MO class nay ra SUA,
// them mot nhanh case moi -> de lam hong logic cu dang chay on dinh.
public class LegacyShippingCalculator
{
    public decimal CalculateFee(ShippingMethod method, decimal itemsAmount)
    {
        switch (method)
        {
            case ShippingMethod.Standard:
                return itemsAmount >= 500_000m ? 0m : 15_000m;
            case ShippingMethod.Express:
                return 30_000m;
            case ShippingMethod.SameDay:
                return 50_000m;
            case ShippingMethod.International:
                return 200_000m;
            default:
                throw new InvalidOperationException($"Chua ho tro phuong thuc van chuyen: {method}");
        }
    }
}
