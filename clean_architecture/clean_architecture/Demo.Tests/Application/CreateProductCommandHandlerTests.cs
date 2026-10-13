using Demo.Application.Products.Commands;
using Demo.Tests.TestDoubles;

namespace Demo.Tests.Application;

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_PersistsAndReturnsDto()
    {
        var repository = new InMemoryProductRepository();
        var handler = new CreateProductCommandHandler(repository);

        var dto = await handler.Handle(new CreateProductCommand
        {
            Name = "Keyboard",
            Description = "Mechanical",
            Price = 900_000m,
            StockQuantity = 10,
            Category = "Accessories"
        }, CancellationToken.None);

        Assert.Equal("Keyboard", dto.Name);
        Assert.NotNull(await repository.GetByIdAsync(dto.Id));
    }

    [Fact]
    public async Task Handle_CancelledToken_StopsBeforePersistence()
    {
        var repository = new InMemoryProductRepository();
        var handler = new CreateProductCommandHandler(repository);
        using var source = new CancellationTokenSource();
        source.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            handler.Handle(new CreateProductCommand
            {
                Name = "Keyboard", Price = 1m, StockQuantity = 1, Category = "Accessories"
            }, source.Token));
    }
}
