namespace Janthus.Model.Entities;

public class InventoryItem
{
    public Item Item { get; set; }
    public int Quantity { get; set; }

    public InventoryItem(Item item, int quantity = 1)
    {
        Item = item;
        Quantity = quantity;
    }
}
