using Microsoft.AspNetCore.Mvc;
using ShopMini;
using ShopMini.Models;
using ShopMini.Ocp;
using ShopMini.Srp;

namespace ShopMini.Controllers;

[ApiController]
[Route("api")]
public class ShopController : ControllerBase
{
    private readonly ShopData _data;
    private readonly ShippingFeeService _shipping;
    private readonly LegacyShippingCalculator _legacy;
    private readonly OrderService _orderService;
    private readonly BadOrderManager _badOrderManager;

    public ShopController(
        ShopData data,
        ShippingFeeService shipping,
        LegacyShippingCalculator legacy,
        OrderService orderService,
        BadOrderManager badOrderManager)
    {
        _data = data;
        _shipping = shipping;
        _legacy = legacy;
        _orderService = orderService;
        _badOrderManager = badOrderManager;
    }

    // ---------- Du lieu ----------
    [HttpGet("products")]
    public IActionResult GetProducts() => Ok(_data.Products);

    [HttpGet("orders")]
    public IActionResult GetOrders() => Ok(_data.Orders);

    // ---------- OCP ----------
    [HttpGet("shipping/methods")]
    public IActionResult Methods() => Ok(Enum.GetNames<ShippingMethod>());

    // ✅ AP DUNG OCP (Strategy)
    [HttpGet("shipping/fee-preview")]
    public IActionResult FeePreview([FromQuery] ShippingMethod method, [FromQuery] decimal itemsAmount)
    {
        try
        {
            var fee = _shipping.CalculateFee(method, itemsAmount);
            return Ok(new { method = method.ToString(), itemsAmount, shippingFee = fee,
                note = "Strategy Pattern (tuan thu OCP) - them loai moi KHONG sua code cu" });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // ❌ VI PHAM OCP (switch-case)
    [HttpGet("shipping/legacy-fee-preview")]
    public IActionResult LegacyFeePreview([FromQuery] ShippingMethod method, [FromQuery] decimal itemsAmount)
    {
        try
        {
            var fee = _legacy.CalculateFee(method, itemsAmount);
            return Ok(new { method = method.ToString(), itemsAmount, shippingFee = fee,
                note = "switch-case (vi pham OCP) - them loai moi PHAI sua class nay" });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // ---------- SRP ----------
    // ✅ AP DUNG SRP
    [HttpPost("orders")]
    public IActionResult PlaceOrder([FromBody] PlaceOrderRequest req)
    {
        try
        {
            var order = _orderService.PlaceOrder(req);
            return Ok(new { message = "Dat hang thanh cong (SRP: tach Data + Shipping + Email)", order });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        { return BadRequest(new { error = ex.Message }); }
    }

    // ❌ VI PHAM SRP (God class)
    [HttpPost("orders/bad")]
    public IActionResult PlaceOrderBad([FromBody] PlaceOrderRequest req)
    {
        try { return Ok(_badOrderManager.PlaceOrderBad(req)); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        { return BadRequest(new { error = ex.Message }); }
    }
}
