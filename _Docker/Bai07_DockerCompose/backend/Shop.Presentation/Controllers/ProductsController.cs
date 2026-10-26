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
}
