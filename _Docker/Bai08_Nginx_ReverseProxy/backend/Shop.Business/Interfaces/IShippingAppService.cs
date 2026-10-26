using Shop.Business.DTOs;
using Shop.Business.Enums;

namespace Shop.Business.Interfaces;

public interface IShippingAppService
{
    IEnumerable<string> GetSupportedMethods();
    ShippingFeePreviewDto PreviewFee(ShippingMethod method, decimal itemsAmount);        // OCP
    ShippingFeePreviewDto PreviewLegacyFee(ShippingMethod method, decimal itemsAmount);  // Vi pham OCP
}
