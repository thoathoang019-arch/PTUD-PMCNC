using Demo.Domain.Entities;
using Demo.Domain.Interfaces;

namespace Demo.Tests.TestDoubles;

internal sealed class InMemoryProductRepository : IProductRepository
{
    private readonly Dictionary<Guid, Product> _products = [];

    public Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IEnumerable<Product>>(_products.Values);

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_products.GetValueOrDefault(id));

    public Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _products[product.Id] = product;
        return Task.FromResult(product);
    }

    public Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _products[product.Id] = product;
        return Task.FromResult(product);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_products.Remove(id));
    }
}
