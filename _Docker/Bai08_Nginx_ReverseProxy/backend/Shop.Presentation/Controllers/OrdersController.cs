using Shop.Business.DTOs;
using Shop.Business.Interfaces;
using Shop.Business.Legacy;
using Microsoft.AspNetCore.Mvc;

namespace Shop.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderAppService _orderService;
    private readonly BadOrderManager _badOrderManager;

    public OrdersController(IOrderAppService orderService, BadOrderManager badOrderManager)
    {
        _orderService = orderService;
        _badOrderManager = badOrderManager;
    }

    /// <summary>Lay toan bo don hang.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
        => Ok(await _orderService.GetAllOrdersAsync());

    /// <summary>Dat hang - Chuan SRP (tach Repository, ShippingFee, Email ra cac lop rieng).</summary>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> PlaceOrder([FromBody] PlaceOrderRequestDto request)
    {
        var result = await _orderService.PlaceOrderAsync(request);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    /// <summary>Dat hang - Demo VI PHAM SRP (1 class BadOrderManager om 5 trach nhiem).</summary>
    [HttpPost("bad")]
    public async Task<IActionResult> PlaceOrderBad([FromBody] PlaceOrderRequestDto request)
        => Ok(await _badOrderManager.PlaceOrderBadSRPAsync(request));
}
