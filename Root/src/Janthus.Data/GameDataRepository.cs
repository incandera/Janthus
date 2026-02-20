using Microsoft.EntityFrameworkCore;
using Janthus.Model.Entities;
using Janthus.Model.Services;

namespace Janthus.Data;

public class GameDataRepository : IGameDataProvider
{
    private readonly JanthusDbContext _context;

    private List<ActorType> _actorTypes;
    private List<CharacterClass> _classes;
    private List<ActorLevel> _levels;
    private List<SkillType> _skillTypes;
    private List<SkillLevel> _skillLevels;
    private List<TileDefinition> _tileDefinitions;
    private List<ObjectDefinition> _objectDefinitions;

    public GameDataRepository(JanthusDbContext context)
    {
        _context = context;
    }

    public void EnsureCreated()
    {
        _context.Database.EnsureCreated();
    }

    public List<ActorType> GetActorTypes()
    {
        _actorTypes ??= _context.ActorTypes.OrderBy(x => x.Name).ToList();
        return _actorTypes;
    }

    public List<Actor> GetBestiary()
    {
        return new List<Actor>();
    }

    public List<CharacterClass> GetClasses()
    {
        _classes ??= _context.CharacterClasses.OrderBy(x => x.Name).ToList();
        return _classes;
    }

    public CharacterClass GetClass(string name)
    {
        return GetClasses().SingleOrDefault(x => x.Name == name);
    }

    public List<ActorLevel> GetLevels()
    {
        _levels ??= _context.ActorLevels.OrderBy(x => x.Number).ToList();
        return _levels;
    }

    public ActorLevel GetLevel(int number)
    {
        return GetLevels().SingleOrDefault(x => x.Number == number);
    }

    public ActorLevel CalculateLevel(int sumOfAttributes)
    {
        var levels = GetLevels();
        var levelIndex = levels.FindIndex(x => x.MinimumSumOfAttributes > sumOfAttributes);

        if (levelIndex <= 0)
            return levels.Last();

        return levels[levelIndex - 1];
    }

    public List<SkillLevel> GetSkillLevels()
    {
        _skillLevels ??= _context.SkillLevels.OrderBy(x => x.Name).ToList();
        return _skillLevels;
    }

    public List<SkillType> GetSkillTypes()
    {
        _skillTypes ??= _context.SkillTypes.OrderBy(x => x.Name).ToList();
        return _skillTypes;
    }

    public List<TileDefinition> GetTileDefinitions()
    {
        _tileDefinitions ??= _context.TileDefinitions.OrderBy(x => x.Id).ToList();
        return _tileDefinitions;
    }

    public TileDefinition GetTileDefinition(int id)
    {
        return GetTileDefinitions().SingleOrDefault(x => x.Id == id);
    }

    public WorldMap GetWorldMap(string name)
    {
        return _context.WorldMaps.SingleOrDefault(x => x.Name == name);
    }

    public List<MapChunk> GetChunksForWorld(int worldMapId)
    {
        return _context.MapChunks
            .Where(x => x.WorldMapId == worldMapId)
            .OrderBy(x => x.ChunkY).ThenBy(x => x.ChunkX)
            .ToList();
    }

    public MapChunk GetChunk(int worldMapId, int chunkX, int chunkY)
    {
        return _context.MapChunks
            .SingleOrDefault(x => x.WorldMapId == worldMapId && x.ChunkX == chunkX && x.ChunkY == chunkY);
    }

    public List<ObjectDefinition> GetObjectDefinitions()
    {
        _objectDefinitions ??= _context.ObjectDefinitions.OrderBy(x => x.Id).ToList();
        return _objectDefinitions;
    }

    public List<MapObject> GetObjectsForChunk(int mapChunkId)
    {
        return _context.MapObjects
            .Where(x => x.MapChunkId == mapChunkId)
            .ToList();
    }

    public void SaveChunk(MapChunk chunk)
    {
        if (chunk.Id == 0)
            _context.MapChunks.Add(chunk);
        else
            _context.MapChunks.Update(chunk);
        _context.SaveChanges();
    }

    public void SaveMapObject(MapObject mapObject)
    {
        if (mapObject.Id == 0)
            _context.MapObjects.Add(mapObject);
        else
            _context.MapObjects.Update(mapObject);
        _context.SaveChanges();
    }

    public void SaveWorldMap(WorldMap worldMap)
    {
        if (worldMap.Id == 0)
            _context.WorldMaps.Add(worldMap);
        else
            _context.WorldMaps.Update(worldMap);
        _context.SaveChanges();
    }
}
