using Shop.Business.Enums;

namespace Shop.Business.Interfaces;

// Trach nhiem DUY NHAT: tinh phi van chuyen (dung Strategy - OCP).
public interface IShippingFeeService
{
    decimal CalculateFee(ShippingMethod method, decimal itemsAmount);
}
