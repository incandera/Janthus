using Janthus.Model.Entities;
using Janthus.Model.Services;

namespace Janthus.Game.World;

public class ChunkManager
{
    private readonly Dictionary<(int, int), LoadedChunk> _chunks = new();
    private readonly TileRegistry _tileRegistry;
    private readonly IGameDataProvider _provider;
    private readonly WorldMap _worldMap;
    private readonly Dictionary<int, ObjectDefinition> _objectDefinitions = new();

    private const int ChunkShift = 5; // log2(32)
    private const int ChunkMask = 31; // 32 - 1

    public int WorldWidth => _worldMap.ChunkCountX * _worldMap.ChunkSize;
    public int WorldHeight => _worldMap.ChunkCountY * _worldMap.ChunkSize;
    public int ChunkSize => _worldMap.ChunkSize;
    public TileRegistry TileRegistry => _tileRegistry;
    public WorldMap WorldMap => _worldMap;

    public ChunkManager(WorldMap worldMap, TileRegistry tileRegistry, IGameDataProvider provider)
    {
        _worldMap = worldMap;
        _tileRegistry = tileRegistry;
        _provider = provider;

        foreach (var def in provider.GetObjectDefinitions())
            _objectDefinitions[def.Id] = def;
    }

    public ObjectDefinition GetObjectDefinition(int id)
    {
        _objectDefinitions.TryGetValue(id, out var def);
        return def;
    }

    public void LoadChunk(int chunkX, int chunkY)
    {
        var key = (chunkX, chunkY);
        if (_chunks.ContainsKey(key))
            return;

        var chunk = _provider.GetChunk(_worldMap.Id, chunkX, chunkY);
        if (chunk == null)
            return;

        var objects = _provider.GetObjectsForChunk(chunk.Id);
        _chunks[key] = new LoadedChunk(chunk, _tileRegistry, objects);
    }

    public void UnloadChunk(int chunkX, int chunkY)
    {
        _chunks.Remove((chunkX, chunkY));
    }

    public LoadedChunk GetLoadedChunk(int chunkX, int chunkY)
    {
        _chunks.TryGetValue((chunkX, chunkY), out var chunk);
        return chunk;
    }

    public GameTile GetTile(int worldX, int worldY)
    {
        var chunkX = worldX >> ChunkShift;
        var chunkY = worldY >> ChunkShift;
        var localX = worldX & ChunkMask;
        var localY = worldY & ChunkMask;

        var chunk = GetLoadedChunk(chunkX, chunkY);
        return chunk?.GetTile(localX, localY);
    }

    public bool IsWalkable(int worldX, int worldY)
    {
        if (!IsInBounds(worldX, worldY))
            return false;

        var tile = GetTile(worldX, worldY);
        if (tile == null || !tile.IsWalkable)
            return false;

        // Check if object at this tile is impassable
        var chunkX = worldX >> ChunkShift;
        var chunkY = worldY >> ChunkShift;
        var localX = worldX & ChunkMask;
        var localY = worldY & ChunkMask;
        var chunk = GetLoadedChunk(chunkX, chunkY);
        if (chunk != null)
        {
            var obj = chunk.GetObject(localX, localY);
            if (obj != null)
            {
                var def = GetObjectDefinition(obj.ObjectDefinitionId);
                if (def != null && !def.IsPassable)
                    return false;
            }
        }

        return true;
    }

    public float GetMovementCost(int worldX, int worldY)
    {
        var tile = GetTile(worldX, worldY);
        return tile?.BaseMovementCost ?? 1.0f;
    }

    public byte GetElevation(int worldX, int worldY)
    {
        var chunkX = worldX >> ChunkShift;
        var chunkY = worldY >> ChunkShift;
        var localX = worldX & ChunkMask;
        var localY = worldY & ChunkMask;

        var chunk = GetLoadedChunk(chunkX, chunkY);
        return chunk?.GetElevation(localX, localY) ?? 0;
    }

    public bool IsInBounds(int worldX, int worldY)
    {
        return worldX >= 0 && worldX < WorldWidth && worldY >= 0 && worldY < WorldHeight;
    }

    public ObjectDefinition GetObjectAt(int worldX, int worldY)
    {
        var chunkX = worldX >> ChunkShift;
        var chunkY = worldY >> ChunkShift;
        var localX = worldX & ChunkMask;
        var localY = worldY & ChunkMask;
        var chunk = GetLoadedChunk(chunkX, chunkY);
        var obj = chunk?.GetObject(localX, localY);
        if (obj == null) return null;
        return GetObjectDefinition(obj.ObjectDefinitionId);
    }

    public void UpdatePlayerPosition(int worldX, int worldY)
    {
        var centerChunkX = worldX >> ChunkShift;
        var centerChunkY = worldY >> ChunkShift;

        // Determine which chunks should be loaded (3x3 neighborhood)
        var needed = new HashSet<(int, int)>();
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                var cx = centerChunkX + dx;
                var cy = centerChunkY + dy;
                if (cx >= 0 && cx < _worldMap.ChunkCountX && cy >= 0 && cy < _worldMap.ChunkCountY)
                    needed.Add((cx, cy));
            }
        }

        // Unload chunks no longer needed
        var toRemove = new List<(int, int)>();
        foreach (var key in _chunks.Keys)
        {
            if (!needed.Contains(key))
                toRemove.Add(key);
        }
        foreach (var key in toRemove)
            _chunks.Remove(key);

        // Load chunks that aren't loaded yet
        foreach (var (cx, cy) in needed)
        {
            LoadChunk(cx, cy);
        }
    }

    public IEnumerable<LoadedChunk> LoadedChunks => _chunks.Values;
}
