using Demo.Application.Products.Commands;
using Demo.Application.Products.DTOs;
using Demo.Application.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Demo.API.Controllers;

/// <summary>
/// API Controller cho Product — điểm vào nhận HTTP request.
/// 
/// Controller CHỈ làm 2 việc:
///   1. Nhận request, tạo Command/Query tương ứng
///   2. Gửi cho MediatR xử lý và trả về response
///   
/// KHÔNG chứa bất kỳ business logic nào — tuân thủ Thin Controller principle.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh sách tất cả sản phẩm
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var products = await _mediator.Send(new GetProductsQuery());
        return Ok(products);
    }

    /// <summary>
    /// Lấy chi tiết sản phẩm theo ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _mediator.Send(new GetProductByIdQuery(id));
        return Ok(product);
    }

    /// <summary>
    /// Tạo mới một sản phẩm
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var product = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>
    /// Cập nhật thông tin sản phẩm
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID trong URL không khớp với ID trong body.");

        var product = await _mediator.Send(command);
        return Ok(product);
    }

    /// <summary>
    /// Xóa sản phẩm theo ID
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteProductCommand(id));
        return NoContent();
    }
}
