using Shop.Business.Enums;
using Shop.Business.Interfaces;
using Shop.Business.Strategies;

namespace Shop.Business.Services;

// ✅ AP DUNG OCP - Service nay chi lam viec voi tap cac strategy (abstraction).
// No KHONG he co if/switch theo tung phuong thuc van chuyen.
// Them phuong thuc moi = them 1 class strategy moi, service nay KHONG doi.
public class ShippingFeeService : IShippingFeeService
{
    private readonly IEnumerable<IShippingFeeStrategy> _strategies;

    public ShippingFeeService(IEnumerable<IShippingFeeStrategy> strategies)
    {
        _strategies = strategies;
    }

    public decimal CalculateFee(ShippingMethod method, decimal itemsAmount)
    {
        var strategy = _strategies.FirstOrDefault(s => s.SupportedMethod == method);
        if (strategy is null)
            throw new InvalidOperationException($"Chua ho tro phuong thuc van chuyen: {method}");

        return strategy.CalculateFee(itemsAmount);
    }
}
