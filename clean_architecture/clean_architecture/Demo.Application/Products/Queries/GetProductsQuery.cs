using Demo.Application.Products.DTOs;
using Demo.Domain.Interfaces;
using MediatR;

namespace Demo.Application.Products.Queries;

// =====================================================
// QUERY: Lấy danh sách tất cả sản phẩm.
// Đây là phần "Q" (Query) trong CQRS pattern.
// Query = chỉ đọc dữ liệu, không thay đổi state.
// =====================================================

public record GetProductsQuery : IRequest<IEnumerable<ProductDto>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IProductRepository _repository;

    public GetProductsQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _repository.GetAllAsync(cancellationToken);

        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            Category = p.Category,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        });
    }
}
