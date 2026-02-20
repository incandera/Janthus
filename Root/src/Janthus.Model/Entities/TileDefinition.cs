namespace Janthus.Model.Entities;

public class TileDefinition : JanthusObject
{
    public string ColorHex { get; set; } = "#FFFFFF";
    public bool IsWalkable { get; set; }
    public float BaseMovementCost { get; set; } = 1.0f;
}
