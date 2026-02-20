namespace Janthus.Model.Entities;

public class Item : JanthusObject
{
    public ItemType Type { get; set; }
    public Quality Quality { get; set; }
    public Material Material { get; set; }
    public decimal TradeValue { get; set; }
    public decimal Durability { get; set; }

    public List<Effect> EffectList { get; set; } = new();
    public List<Item> CraftComponents { get; set; } = new();
}
