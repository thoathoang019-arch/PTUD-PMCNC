using Demo.Domain.Entities;
using Demo.Domain.Interfaces;
using Demo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Demo.Infrastructure.Repositories;

/// <summary>
/// Triển khai cụ thể (concrete implementation) của IProductRepository.
/// 
/// Đây là ví dụ điển hình của Dependency Inversion Principle:
///   - Interface IProductRepository được định nghĩa ở Domain layer (high-level)
///   - Implementation ProductRepository nằm ở Infrastructure layer (low-level)
///   - Domain layer KHÔNG biết gì về EF Core, SQL, hay bất kỳ ORM nào
///   
/// Lợi ích: Có thể dễ dàng thay thế bằng implementation khác (Dapper, MongoDB, v.v.)
///           mà không ảnh hưởng đến Domain và Application layer.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products.ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products.FindAsync([id], cancellationToken);
    }

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FindAsync([id], cancellationToken);
        if (product == null) return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
