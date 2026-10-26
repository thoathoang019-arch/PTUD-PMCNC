using ECommerce.Modules.Inventory.Application;
using ECommerce.Modules.Inventory.Infrastructure;
using ECommerce.Shared.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Modules.Inventory.Api;

// ============================================================
// Endpoint REST của Inventory. Chính là "đường cắt đồng bộ" khi tách microservice:
// Order có thể gọi IInventoryService in-process, HOẶC gọi GET /api/inventory/check qua HTTP.
// Controller NỘI BỘ của Inventory dùng thẳng InventoryService (concrete) của module mình.
// ============================================================
[ApiController]
[Route("api/inventory")]
public sealed class InventoryController : ControllerBase
{
    private readonly InventoryService _service;
    private readonly InventoryStore _store;

    public InventoryController(InventoryService service, InventoryStore store)
    {
        _service = service;
        _store = store;
    }

    [HttpGet]
    public IActionResult List() => Ok(_store.All());

    [HttpGet("check")]
    public async Task<ActionResult<StockCheckResult>> Check([FromQuery] string sku, [FromQuery] int qty)
        => Ok(await _service.CheckStockAsync(sku, qty));
}
