using Demo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Demo.Tests.Infrastructure;

public class AppDbContextTests
{
    [Fact]
    public void EnsureCreated_WithSeedData_BuildsValidModel()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"test-{Guid.NewGuid()}")
            .Options;

        using var context = new AppDbContext(options);

        Assert.True(context.Database.EnsureCreated());
        Assert.Equal(3, context.Products.Count());
    }
}
