using Demo.Application.Products.DTOs;
using Demo.Domain.Entities;
using Demo.Domain.Interfaces;
using MediatR;

namespace Demo.Application.Products.Commands;

// =====================================================
// COMMAND: Đại diện cho hành động "Tạo mới sản phẩm"
// Đây là phần "C" (Command) trong CQRS pattern.
// Command = thay đổi state (ghi dữ liệu).
// =====================================================

/// <summary>
/// Command chứa dữ liệu đầu vào cần thiết để tạo Product.
/// Implement IRequest<T> của MediatR để tự động route đến Handler tương ứng.
/// </summary>
public record CreateProductCommand : IRequest<ProductDto>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public string Category { get; init; } = string.Empty;
}

/// <summary>
/// Handler xử lý logic nghiệp vụ cho CreateProductCommand.
/// MediatR sẽ tự động gọi Handle() khi nhận được Command tương ứng.
/// </summary>
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Map từ Command sang Domain Entity
        var product = Product.Create(
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity,
            request.Category);

        var created = await _repository.AddAsync(product, cancellationToken);

        // Map từ Domain Entity sang DTO để trả về
        return new ProductDto
        {
            Id = created.Id,
            Name = created.Name,
            Description = created.Description,
            Price = created.Price,
            StockQuantity = created.StockQuantity,
            Category = created.Category,
            CreatedAt = created.CreatedAt,
            UpdatedAt = created.UpdatedAt
        };
    }
}
