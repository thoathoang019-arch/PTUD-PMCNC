using Demo.Domain.Entities;
using Demo.Domain.Exceptions;
using Demo.Domain.Interfaces;
using MediatR;

namespace Demo.Application.Products.Commands;

// =====================================================
// COMMAND: Xóa sản phẩm theo ID
// =====================================================

public record DeleteProductCommand(Guid Id) : IRequest<bool>;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _repository;

    public DeleteProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        return await _repository.DeleteAsync(request.Id, cancellationToken);
    }
}
