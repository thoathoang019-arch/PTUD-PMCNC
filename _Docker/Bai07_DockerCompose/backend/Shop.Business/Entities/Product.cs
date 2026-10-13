namespace Shop.Business.Entities;

// Entity: chi mo ta du lieu san pham (SRP - mot trach nhiem: giu du lieu).
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}
