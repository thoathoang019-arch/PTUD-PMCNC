using Shop.Business.Entities;
using Shop.Business.Interfaces;
using Shop.DataAccess.Persistence;

namespace Shop.DataAccess.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly InMemoryDataStore _store;
    public ProductRepository(InMemoryDataStore store) => _store = store;

    public Task<IEnumerable<Product>> GetAllAsync() => Task.FromResult(_store.GetProducts());
    public Task<Product?> GetByIdAsync(int id) => Task.FromResult(_store.GetProduct(id));

    // In-memory: doi tuong tra ve la tham chieu that trong kho nen sua truc tiep da co hieu luc.
    public Task UpdateAsync(Product product) => Task.CompletedTask;
}
