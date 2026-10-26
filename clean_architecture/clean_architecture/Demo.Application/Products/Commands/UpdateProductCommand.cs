using Demo.Application.Products.DTOs;
using Demo.Domain.Entities;
using Demo.Domain.Exceptions;
using Demo.Domain.Interfaces;
using MediatR;

namespace Demo.Application.Products.Commands;

// =====================================================
// COMMAND: Cập nhật thông tin sản phẩm
// =====================================================

public record UpdateProductCommand : IRequest<ProductDto>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public string Category { get; init; } = string.Empty;
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IProductRepository _repository;

    public UpdateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        // Cập nhật các thuộc tính
        product.Update(
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity,
            request.Category);

        var updated = await _repository.UpdateAsync(product, cancellationToken);

        return new ProductDto
        {
            Id = updated.Id,
            Name = updated.Name,
            Description = updated.Description,
            Price = updated.Price,
            StockQuantity = updated.StockQuantity,
            Category = updated.Category,
            CreatedAt = updated.CreatedAt,
            UpdatedAt = updated.UpdatedAt
        };
    }
}
