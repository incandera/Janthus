namespace Janthus.Game.World;

public class TileMap
{
    public MapLayer GroundLayer { get; set; }
    public int Width => GroundLayer.Width;
    public int Height => GroundLayer.Height;

    public TileMap(int width, int height)
    {
        GroundLayer = new MapLayer("Ground", width, height);
    }

    public bool IsWalkable(int x, int y)
    {
        var tile = GroundLayer.GetTile(x, y);
        return tile != null && tile.IsWalkable;
    }

    public static TileMap GenerateDefault(int width, int height)
    {
        var map = new TileMap(width, height);
        var random = new Random(42);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Edge border = water
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    map.GroundLayer.Tiles[x, y] = Tile.Water;
                    continue;
                }

                // Small pond in center area
                var cx = width / 2;
                var cy = height / 2;
                var dx = x - cx;
                var dy = y - cy;
                if (dx * dx + dy * dy < 9)
                {
                    map.GroundLayer.Tiles[x, y] = Tile.Water;
                    continue;
                }

                // Stone path across middle
                if (y == height / 2 && x > 3 && x < width - 4)
                {
                    map.GroundLayer.Tiles[x, y] = Tile.Stone;
                    continue;
                }

                // Random grass/dark grass/sand
                var roll = random.Next(100);
                if (roll < 60)
                    map.GroundLayer.Tiles[x, y] = Tile.Grass;
                else if (roll < 80)
                    map.GroundLayer.Tiles[x, y] = Tile.DarkGrass;
                else if (roll < 95)
                    map.GroundLayer.Tiles[x, y] = Tile.Sand;
                else
                    map.GroundLayer.Tiles[x, y] = Tile.Stone;
            }
        }

        return map;
    }
}
