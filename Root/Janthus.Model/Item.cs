using System.Collections.Generic;

namespace Janthus.Model
{
    public class Item : JanthusObject
    {
        public ItemType Type { get; set; }
        public Quality Quality { get; set; }
        public Material Material { get; set; }
        public decimal TradeValue { get; set; }
        public decimal Durability { get; set; }

        public List<Effect> EffectList { get; set; }

        // The various (sub-)items required to build this item using the crafting system
        public List<Item> CraftComponents { get; set; }
    }
}
