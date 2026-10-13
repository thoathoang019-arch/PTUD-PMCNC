using Shop.Business.DTOs;

namespace Shop.Business.Interfaces;

public interface IProductAppService
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
}
