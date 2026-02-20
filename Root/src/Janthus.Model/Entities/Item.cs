using Janthus.Model.Enums;

namespace Janthus.Model.Entities;

public class Item : JanthusObject
{
    public ItemType Type { get; set; }
    public Quality Quality { get; set; }
    public Material Material { get; set; }
    public decimal TradeValue { get; set; }
    public decimal Durability { get; set; }

    public EquipmentSlot Slot { get; set; }
    public decimal AttackRating { get; set; }
    public decimal ArmorRating { get; set; }
    public int LuckBonus { get; set; }
    public int StrengthBonus { get; set; }
    public int DexterityBonus { get; set; }
    public int ConstitutionBonus { get; set; }

    public List<Effect> EffectList { get; set; } = new();
    public List<Item> CraftComponents { get; set; } = new();
}
