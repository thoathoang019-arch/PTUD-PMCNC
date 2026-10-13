using ShopMini.Models;

namespace ShopMini.Ocp;

// ✅ OCP - "hanh vi chung": moi phuong thuc van chuyen la 1 class rieng.
public interface IShippingFeeStrategy
{
    ShippingMethod Method { get; }
    decimal CalculateFee(decimal itemsAmount);
}

public class StandardShipping : IShippingFeeStrategy
{
    public ShippingMethod Method => ShippingMethod.Standard;
    // 15.000d, mien phi neu don >= 500.000d
    public decimal CalculateFee(decimal itemsAmount) => itemsAmount >= 500_000m ? 0m : 15_000m;
}

public class ExpressShipping : IShippingFeeStrategy
{
    public ShippingMethod Method => ShippingMethod.Express;
    public decimal CalculateFee(decimal itemsAmount) => 30_000m;
}

public class SameDayShipping : IShippingFeeStrategy
{
    public ShippingMethod Method => ShippingMethod.SameDay;
    public decimal CalculateFee(decimal itemsAmount) => 50_000m;
}

// Loai MO RONG: chi them class nay + 1 dong dang ky DI, KHONG sua code cu.
public class InternationalShipping : IShippingFeeStrategy
{
    public ShippingMethod Method => ShippingMethod.International;
    public decimal CalculateFee(decimal itemsAmount) => 200_000m;
}
