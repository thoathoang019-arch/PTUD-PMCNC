using ShopMini.Models;

namespace ShopMini.Ocp;

// ✅ AP DUNG OCP - chi lam viec voi tap strategy, KHONG co if/switch.
// Them phuong thuc moi = them 1 class strategy, service nay KHONG doi.
public class ShippingFeeService
{
    private readonly IEnumerable<IShippingFeeStrategy> _strategies;
    public ShippingFeeService(IEnumerable<IShippingFeeStrategy> strategies) => _strategies = strategies;

    public decimal CalculateFee(ShippingMethod method, decimal itemsAmount)
    {
        var strategy = _strategies.FirstOrDefault(s => s.Method == method);
        if (strategy is null)
            throw new InvalidOperationException($"Chua ho tro phuong thuc van chuyen: {method}");
        return strategy.CalculateFee(itemsAmount);
    }
}
