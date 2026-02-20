using Microsoft.Xna.Framework;

namespace Janthus.Game.World;

public class GameTile
{
    public int TileDefinitionId { get; }
    public string Name { get; }
    public Color Color { get; }
    public bool IsWalkable { get; }
    public float BaseMovementCost { get; }

    public GameTile(int tileDefinitionId, string name, Color color, bool isWalkable, float baseMovementCost)
    {
        TileDefinitionId = tileDefinitionId;
        Name = name;
        Color = color;
        IsWalkable = isWalkable;
        BaseMovementCost = baseMovementCost;
    }
}
