using ECommerce.Modules.Order.Application;
using ECommerce.Modules.Order.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Modules.Order.Api;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;
    private readonly OrderStore _store;

    public OrdersController(IOrderService orders, OrderStore store)
    {
        _orders = orders;
        _store = store;
    }

    [HttpGet]
    public IActionResult List() => Ok(_store.All());

    // POST /api/orders — tạo đơn (đồng bộ check kho -> tạo -> publish event)
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest req)
    {
        var result = await _orders.CreateAsync(req);
        return result.Success
            ? Ok(result)
            : UnprocessableEntity(result);   // 422 khi hết hàng
    }
}
