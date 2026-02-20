namespace Janthus.Game.World;

public class MapLayer
{
    public string Name { get; set; }
    public Tile[,] Tiles { get; set; }

    public int Width => Tiles.GetLength(0);
    public int Height => Tiles.GetLength(1);

    public MapLayer(string name, int width, int height)
    {
        Name = name;
        Tiles = new Tile[width, height];
    }

    public Tile GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return null;
        return Tiles[x, y];
    }
}
