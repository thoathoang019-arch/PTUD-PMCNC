using Microsoft.AspNetCore.Mvc;
using ShopMini.Dip;

namespace ShopMini.Controllers;

[ApiController]
[Route("api/dip")]
public class DipController : ControllerBase
{
    private readonly OrderPlacementService _placement;   // duoc DI tiem IOrderStore + INotificationSender
    public DipController(OrderPlacementService placement) => _placement = placement;

    // ❌ Tu new EmailSender + dung GetType().Name -> phu thuoc chi tiet, doi kenh phai sua code.
    [HttpGet("violation")]
    public IActionResult Violation([FromQuery] string customer = "Le Van C",
                                   [FromQuery] string contact = "c@shop.vn",
                                   [FromQuery] decimal total = 300000)
    {
        var result = new BadOrderPlacementService().PlaceOrder(customer, contact, total);
        return Ok(new { result, note = "Hardcode new EmailSender + GetType().Name. VI PHAM DIP." });
    }

    // ✅ Inject abstraction qua DI. Doi Email <-> SMS chi sua 1 dong trong Program.cs, khong sua service.
    [HttpGet("solution")]
    public IActionResult Solution([FromQuery] string customer = "Le Van C",
                                  [FromQuery] string contact = "0900123456",
                                  [FromQuery] decimal total = 300000)
    {
        var result = _placement.PlaceOrder(customer, contact, total);
        return Ok(new { result, note = "OrderPlacementService phu thuoc IOrderStore + INotificationSender (inject qua DI). TUAN THU DIP." });
    }
}
