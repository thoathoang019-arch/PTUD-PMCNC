using Shop.Business.DTOs;
using Shop.Business.Enums;
using Shop.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Shop.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShippingController : ControllerBase
{
    private readonly IShippingAppService _shippingService;
    public ShippingController(IShippingAppService shippingService) => _shippingService = shippingService;

    /// <summary>Cac phuong thuc van chuyen dang ho tro.</summary>
    [HttpGet("methods")]
    public ActionResult<IEnumerable<string>> GetMethods() => Ok(_shippingService.GetSupportedMethods());

    /// <summary>Tinh thu phi van chuyen - Chuan OCP (Strategy Pattern).</summary>
    [HttpGet("fee-preview")]
    public ActionResult<ShippingFeePreviewDto> PreviewFee([FromQuery] ShippingMethod method, [FromQuery] decimal itemsAmount)
        => Ok(_shippingService.PreviewFee(method, itemsAmount));

    /// <summary>Tinh thu phi van chuyen - Legacy (Vi pham OCP).</summary>
    [HttpGet("legacy-fee-preview")]
    public ActionResult<ShippingFeePreviewDto> PreviewLegacyFee([FromQuery] ShippingMethod method, [FromQuery] decimal itemsAmount)
        => Ok(_shippingService.PreviewLegacyFee(method, itemsAmount));
}
