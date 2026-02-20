namespace Janthus.Model.Entities;

public class MerchantStock
{
    public int Id { get; set; }
    public string NpcName { get; set; }
    public int ItemId { get; set; }
    public int Quantity { get; set; }
    public decimal PriceMultiplier { get; set; } = 1.0m;
}
