using Shop.Business.Enums;

namespace Shop.Business.Strategies;

// OCP - "hanh vi chung" cua moi cach tinh phi ship.
// Moi phuong thuc van chuyen la 1 class rieng implement interface nay.
public interface IShippingFeeStrategy
{
    ShippingMethod SupportedMethod { get; }
    decimal CalculateFee(decimal itemsAmount);
}
