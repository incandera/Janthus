using System.Collections.Generic;

namespace Janthus.Model
{
    public class Quality : JanthusObject, IAttributeModifier, ITradeValueModifier
    {
        public List<decimal> AttributeMultiplier { get; set; }
        public List<decimal> TradeValueMultiplier { get; set; }
    }
}
