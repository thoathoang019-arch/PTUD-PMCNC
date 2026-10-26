using Shop.Business.DTOs;
using Shop.Business.Enums;
using Shop.Business.Interfaces;
using Shop.Business.Legacy;

namespace Shop.Business.Services;

public class ShippingAppService : IShippingAppService
{
    private readonly IShippingFeeService _shippingFeeService;
    private readonly LegacyShippingCalculator _legacyCalculator;

    public ShippingAppService(
        IShippingFeeService shippingFeeService,
        LegacyShippingCalculator legacyCalculator)
    {
        _shippingFeeService = shippingFeeService;
        _legacyCalculator = legacyCalculator;
    }

    public IEnumerable<string> GetSupportedMethods()
        => Enum.GetNames(typeof(ShippingMethod));

    // ✅ Chuan OCP
    public ShippingFeePreviewDto PreviewFee(ShippingMethod method, decimal itemsAmount)
    {
        var fee = _shippingFeeService.CalculateFee(method, itemsAmount);
        return new ShippingFeePreviewDto
        {
            ShippingMethod = method.ToString(),
            ItemsAmount = itemsAmount,
            ShippingFee = fee,
            Method = "Strategy Pattern + DI (Tuan thu OCP)",
            Note = "Them phuong thuc van chuyen moi ma KHONG SUA code tinh phi hien tai!"
        };
    }

    // ❌ Vi pham OCP
    public ShippingFeePreviewDto PreviewLegacyFee(ShippingMethod method, decimal itemsAmount)
    {
        var fee = _legacyCalculator.CalculateFee(method, itemsAmount);
        return new ShippingFeePreviewDto
        {
            ShippingMethod = method.ToString(),
            ItemsAmount = itemsAmount,
            ShippingFee = fee,
            Method = "Switch-Case Legacy (Vi pham OCP)",
            Note = "Muon them phuong thuc moi buoc phai SUA switch-case trong LegacyShippingCalculator!"
        };
    }
}
