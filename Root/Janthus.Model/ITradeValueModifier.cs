using System.Collections.Generic;

namespace Janthus.Model
{
    public interface ITradeValueModifier
    {
        List<decimal> TradeValueMultiplier { get; set; }
    }
}
