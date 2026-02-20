namespace Janthus.Model.Entities;

public class ObjectDefinition : JanthusObject
{
    public bool IsPassable { get; set; }
    public float MovementCostModifier { get; set; }
    public bool BlocksLineOfSight { get; set; }
}
