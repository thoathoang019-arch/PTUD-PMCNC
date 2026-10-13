using Shop.Business.Entities;

namespace Shop.Business.Interfaces;

// Trach nhiem: truy xuat du lieu san pham (tang persistence).
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task UpdateAsync(Product product);
}
