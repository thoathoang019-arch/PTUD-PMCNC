// ============================================================
// Unit Test — ProductAppService
// ============================================================
// Demo thêm 1 class test khác để chứng minh test coverage đa dạng.
// Sử dụng "fake" (manual mock) thay vì library phức tạp.
//
// Trong thực tế, nên dùng Moq hoặc NSubstitute để mock.
// Ở đây dùng fake class đơn giản để tập trung vào KHÁI NIỆM Unit Test.
// ============================================================

using Shop.Business.DTOs;
using Shop.Business.Entities;
using Shop.Business.Interfaces;
using Shop.Business.Services;

namespace Shop.Tests;

/// <summary>
/// Fake repository — thay thế database thật bằng data trong bộ nhớ.
/// Đây là cách đơn giản nhất để test ĐỘC LẬP (không cần database).
/// </summary>
internal class FakeProductRepository : IProductRepository
{
    private readonly List<Product> _products;

    public FakeProductRepository(List<Product> products)
    {
        _products = products;
    }

    public Task<IEnumerable<Product>> GetAllAsync()
        => Task.FromResult<IEnumerable<Product>>(_products);

    public Task<Product?> GetByIdAsync(int id)
        => Task.FromResult(_products.FirstOrDefault(p => p.Id == id));

    public Task UpdateStockAsync(int id, int newStock)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product != null) product.Stock = newStock;
        return Task.CompletedTask;
    }
}

public class ProductAppServiceTests
{
    // ── Test 1: Trả về danh sách sản phẩm đúng ─────────────────────
    [Fact]
    public async Task GetAllProducts_ReturnsCorrectCount()
    {
        // Arrange: Fake repository với 3 sản phẩm
        var fakeProducts = new List<Product>
        {
            new() { Id = 1, Name = "Laptop", Price = 15_000_000m, Stock = 10 },
            new() { Id = 2, Name = "Phone",  Price = 8_000_000m,  Stock = 25 },
            new() { Id = 3, Name = "Tablet", Price = 12_000_000m, Stock = 5 }
        };
        var service = new ProductAppService(new FakeProductRepository(fakeProducts));

        // Act
        var products = await service.GetAllProductsAsync();

        // Assert
        Assert.Equal(3, products.Count());
    }

    // ── Test 2: Mapping đúng từ Entity → DTO ────────────────────────
    [Fact]
    public async Task GetAllProducts_MapsEntityToDto()
    {
        // Arrange
        var fakeProducts = new List<Product>
        {
            new() { Id = 1, Name = "Laptop ASUS", Price = 20_000_000m, Stock = 7 }
        };
        var service = new ProductAppService(new FakeProductRepository(fakeProducts));

        // Act
        var products = (await service.GetAllProductsAsync()).ToList();

        // Assert: kiểm tra mapping từng field
        Assert.Single(products);
        Assert.Equal(1, products[0].Id);
        Assert.Equal("Laptop ASUS", products[0].Name);
        Assert.Equal(20_000_000m, products[0].Price);
        Assert.Equal(7, products[0].Stock);
    }

    // ── Test 3: Repository rỗng → trả về danh sách rỗng ────────────
    [Fact]
    public async Task GetAllProducts_ReturnsEmpty_WhenNoProducts()
    {
        // Arrange: repository không có sản phẩm
        var service = new ProductAppService(new FakeProductRepository(new List<Product>()));

        // Act
        var products = await service.GetAllProductsAsync();

        // Assert
        Assert.Empty(products);
    }
}
