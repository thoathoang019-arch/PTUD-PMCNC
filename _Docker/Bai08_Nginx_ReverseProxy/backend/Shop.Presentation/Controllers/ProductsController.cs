using Shop.Business.DTOs;
using Shop.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Shop.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductAppService _productService;
    public ProductsController(IProductAppService productService) => _productService = productService;

    /// <summary>Lay danh sach san pham.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        => Ok(await _productService.GetAllProductsAsync());

    /// <summary>
    /// Endpoint test cho demo client_max_body_size (Bai 8).
    /// Nhan bat ky body nao va tra ve kich thuoc da nhan.
    /// - Body nho hon client_max_body_size → NGINX cho qua → 200 OK
    /// - Body lon hon client_max_body_size → NGINX chan → 413 Request Entity Too Large
    /// </summary>
    [HttpPost("test-upload")]
    public IActionResult TestUpload()
    {
        var instanceId = Environment.GetEnvironmentVariable("HOSTNAME") ?? "unknown";
        return Ok(new
        {
            message = "Upload thanh cong! NGINX da cho request qua toi backend.",
            contentLength = Request.ContentLength,
            contentLengthMB = Request.ContentLength.HasValue
                ? $"{Request.ContentLength.Value / (1024.0 * 1024.0):F2} MB"
                : "unknown",
            servedBy = instanceId
        });
    }
}
