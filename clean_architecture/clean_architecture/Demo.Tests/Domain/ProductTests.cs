using Demo.Domain.Entities;

namespace Demo.Tests.Domain;

public class ProductTests
{
    [Fact]
    public void Create_WithValidData_CreatesProduct()
    {
        var product = Product.Create("Clean Architecture", "Book", 250_000m, 5, "Books");

        Assert.Equal("Clean Architecture", product.Name);
        Assert.Equal(250_000m, product.Price);
        Assert.NotEqual(Guid.Empty, product.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositivePrice_Throws(decimal price)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Product.Create("Book", "Demo", price, 1, "Books"));
    }

    [Fact]
    public void Create_WithBlankName_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            Product.Create("   ", "Demo", 10m, 1, "Books"));
    }

    [Fact]
    public void Create_WithNegativeStock_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Product.Create("Book", "Demo", 10m, -1, "Books"));
    }

    [Fact]
    public void Update_WithValidData_ChangesFieldsAndTimestamp()
    {
        var product = Product.Create("Old", "Old", 10m, 1, "Books");

        product.Update("New", "Updated", 20m, 2, "Education");

        Assert.Equal("New", product.Name);
        Assert.Equal(20m, product.Price);
        Assert.NotNull(product.UpdatedAt);
    }
}
