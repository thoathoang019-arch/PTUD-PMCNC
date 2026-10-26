using Shop.Business.DTOs;
using Shop.Business.Interfaces;

namespace Shop.Business.Services;

public class ProductAppService : IProductAppService
{
    private readonly IProductRepository _productRepository;

    public ProductAppService(IProductRepository productRepository)
        => _productRepository = productRepository;

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Stock = p.Stock
        });
    }
}
