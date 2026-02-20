namespace Janthus.Model.Entities;

public class CraftOperation : Operation
{
    public List<Material> RequiredMaterials { get; set; } = new();
    public Item ResultItem { get; set; }
}
