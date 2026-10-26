using Shop.Business.Entities;
using Shop.Business.Interfaces;
using Shop.DataAccess.Persistence;

namespace Shop.DataAccess.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly InMemoryDataStore _store;
    public OrderRepository(InMemoryDataStore store) => _store = store;

    public Task<IEnumerable<Order>> GetAllAsync() => Task.FromResult(_store.GetOrders());
    public Task<Order?> GetByIdAsync(int id) => Task.FromResult(_store.GetOrder(id));
    public Task AddAsync(Order order) { _store.AddOrder(order); return Task.CompletedTask; }
}
