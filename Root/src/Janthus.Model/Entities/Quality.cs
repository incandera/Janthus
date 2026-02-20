using Janthus.Model.Interfaces;

namespace Janthus.Model.Entities;

public class Quality : JanthusObject, IAttributeModifier, ITradeValueModifier
{
    public Dictionary<string, decimal> AttributeMultipliers { get; set; } = new();
    public decimal TradeValueMultiplier { get; set; }
}
