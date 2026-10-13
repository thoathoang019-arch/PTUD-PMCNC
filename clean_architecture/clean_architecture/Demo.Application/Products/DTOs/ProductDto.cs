namespace Demo.Application.Products.DTOs;

/// <summary>
/// Data Transfer Object cho Product.
/// Dùng để truyền dữ liệu giữa Application layer và Presentation layer,
/// tránh expose trực tiếp Domain Entity ra bên ngoài.
/// </summary>
public record ProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public string Category { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
