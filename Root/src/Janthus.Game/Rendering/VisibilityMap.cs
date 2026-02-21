namespace Janthus.Game.Rendering;

public enum TileVisibility : byte
{
    Unexplored = 0,
    Explored = 1,
    Visible = 2
}

public class VisibilityMap
{
    private readonly TileVisibility[,] _tiles;
    public int Width { get; }
    public int Height { get; }

    public VisibilityMap(int width, int height)
    {
        Width = width;
        Height = height;
        _tiles = new TileVisibility[width, height];
    }

    public TileVisibility GetVisibility(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return TileVisibility.Unexplored;
        return _tiles[x, y];
    }

    public void SetVisible(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return;
        _tiles[x, y] = TileVisibility.Visible;
    }

    public void ClearVisible()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (_tiles[x, y] == TileVisibility.Visible)
                    _tiles[x, y] = TileVisibility.Explored;
            }
        }
    }

    public byte[] GetChunkVisibility(int chunkX, int chunkY, int chunkSize)
    {
        var data = new byte[chunkSize * chunkSize];
        var offsetX = chunkX * chunkSize;
        var offsetY = chunkY * chunkSize;

        for (int ly = 0; ly < chunkSize; ly++)
        {
            for (int lx = 0; lx < chunkSize; lx++)
            {
                var wx = offsetX + lx;
                var wy = offsetY + ly;
                if (wx < Width && wy < Height)
                    data[ly * chunkSize + lx] = (byte)_tiles[wx, wy];
            }
        }

        return data;
    }

    public void LoadChunkVisibility(int chunkX, int chunkY, int chunkSize, byte[] data)
    {
        if (data == null || data.Length != chunkSize * chunkSize) return;

        var offsetX = chunkX * chunkSize;
        var offsetY = chunkY * chunkSize;

        for (int ly = 0; ly < chunkSize; ly++)
        {
            for (int lx = 0; lx < chunkSize; lx++)
            {
                var wx = offsetX + lx;
                var wy = offsetY + ly;
                if (wx < Width && wy < Height)
                {
                    var val = data[ly * chunkSize + lx];
                    if (val <= 2)
                        _tiles[wx, wy] = (TileVisibility)val;
                }
            }
        }
    }
}
