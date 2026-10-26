using Demo.Application.Products.Commands;
using Demo.Domain.Entities;
using Demo.Tests.TestDoubles;

namespace Demo.Tests.Application;

public class UpdateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_UsesDomainUpdateRules()
    {
        var repository = new InMemoryProductRepository();
        var original = Product.Create("Old", "Old", 10m, 1, "Books");
        await repository.AddAsync(original);
        var handler = new UpdateProductCommandHandler(repository);

        var dto = await handler.Handle(new UpdateProductCommand
        {
            Id = original.Id,
            Name = "New",
            Description = "Updated",
            Price = 20m,
            StockQuantity = 2,
            Category = "Education"
        }, CancellationToken.None);

        Assert.Equal("New", dto.Name);
        Assert.NotNull(dto.UpdatedAt);
    }
}
