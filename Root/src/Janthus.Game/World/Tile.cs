using Microsoft.Xna.Framework;

namespace Janthus.Game.World;

public class Tile
{
    public int TileId { get; set; }
    public string Name { get; set; }
    public Color Color { get; set; }
    public bool IsWalkable { get; set; }

    public static readonly Tile Grass = new() { TileId = 0, Name = "Grass", Color = new Color(34, 139, 34), IsWalkable = true };
    public static readonly Tile Water = new() { TileId = 1, Name = "Water", Color = new Color(30, 90, 200), IsWalkable = false };
    public static readonly Tile Stone = new() { TileId = 2, Name = "Stone", Color = new Color(128, 128, 128), IsWalkable = true };
    public static readonly Tile Sand = new() { TileId = 3, Name = "Sand", Color = new Color(210, 180, 100), IsWalkable = true };
    public static readonly Tile DarkGrass = new() { TileId = 4, Name = "Dark Grass", Color = new Color(20, 100, 20), IsWalkable = true };
}
