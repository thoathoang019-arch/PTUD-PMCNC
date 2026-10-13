using Demo.Domain.Entities;

namespace Demo.Domain.Interfaces;

/// <summary>
/// Interface Repository cho Product.
/// Định nghĩa ở Domain layer, triển khai cụ thể ở Infrastructure layer.
/// Đây là ví dụ của Dependency Inversion Principle (chữ D trong SOLID).
/// </summary>
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default);
    Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
