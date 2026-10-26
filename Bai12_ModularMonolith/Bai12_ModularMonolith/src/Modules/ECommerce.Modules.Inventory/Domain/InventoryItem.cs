namespace ECommerce.Modules.Inventory.Domain;

public sealed class InventoryItem
{
    public required string Sku { get; set; }
    public required string Name { get; set; }
    public int Available { get; set; }
}
