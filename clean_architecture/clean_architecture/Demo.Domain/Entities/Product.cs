namespace Demo.Domain.Entities;

/// <summary>
/// Entity đại diện cho một Sản phẩm trong hệ thống.
/// Đây là layer Domain — không phụ thuộc vào bất kỳ framework hay thư viện bên ngoài nào.
/// </summary>
public class Product
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public decimal Price { get; private set; }

    public int StockQuantity { get; private set; }

    public string Category { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; private set; }

    private Product()
    {
    }

    public static Product Create(
        string name,
        string description,
        decimal price,
        int stockQuantity,
        string category)
    {
        Validate(name, price, stockQuantity);

        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description.Trim(),
            Price = price,
            StockQuantity = stockQuantity,
            Category = category.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        string description,
        decimal price,
        int stockQuantity,
        string category)
    {
        Validate(name, price, stockQuantity);

        Name = name.Trim();
        Description = description.Trim();
        Price = price;
        StockQuantity = stockQuantity;
        Category = category.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(string name, decimal price, int stockQuantity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên sản phẩm không được để trống.", nameof(name));
        if (price <= 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Giá sản phẩm phải lớn hơn 0.");
        if (stockQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(stockQuantity), "Tồn kho không được âm.");
    }
}
