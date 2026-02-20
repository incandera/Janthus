using Janthus.Model.Entities;

namespace Janthus.Game.World;

public class LoadedChunk
{
    public int ChunkX { get; }
    public int ChunkY { get; }
    public int Size { get; }
    public int DbId { get; }

    private readonly GameTile[,] _tiles;
    private readonly byte[,] _heights;
    private readonly List<MapObject> _objects;

    public LoadedChunk(MapChunk chunk, TileRegistry registry, List<MapObject> objects)
    {
        ChunkX = chunk.ChunkX;
        ChunkY = chunk.ChunkY;
        DbId = chunk.Id;
        _objects = objects;

        // Determine chunk size from GroundData length (square root for row-major square)
        Size = (int)Math.Sqrt(chunk.GroundData.Length);
        _tiles = new GameTile[Size, Size];
        _heights = new byte[Size, Size];

        // Deserialize ground data (row-major: y * Size + x)
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                var tileId = chunk.GroundData[y * Size + x];
                _tiles[x, y] = registry.GetTile(tileId);
            }
        }

        // Deserialize height data if present
        if (chunk.HeightData != null)
        {
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    _heights[x, y] = chunk.HeightData[y * Size + x];
                }
            }
        }
    }

    public GameTile GetTile(int localX, int localY)
    {
        if (localX < 0 || localX >= Size || localY < 0 || localY >= Size)
            return null;
        return _tiles[localX, localY];
    }

    public byte GetElevation(int localX, int localY)
    {
        if (localX < 0 || localX >= Size || localY < 0 || localY >= Size)
            return 0;
        return _heights[localX, localY];
    }

    public MapObject GetObject(int localX, int localY)
    {
        foreach (var obj in _objects)
        {
            if (obj.LocalX == localX && obj.LocalY == localY)
                return obj;
        }
        return null;
    }

    public List<MapObject> Objects => _objects;
}
