namespace Janthus.Model.Entities;

public class Material : JanthusObject
{
    public decimal Hardness { get; set; }
    public decimal WeightMultiplier { get; set; }
    public decimal TradeValueMultiplier { get; set; }
}
