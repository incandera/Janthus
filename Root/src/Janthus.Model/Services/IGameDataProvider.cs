using Janthus.Model.Entities;

namespace Janthus.Model.Services;

public interface IGameDataProvider
{
    List<ActorType> GetActorTypes();
    List<Actor> GetBestiary();
    List<CharacterClass> GetClasses();
    CharacterClass GetClass(string name);
    List<ActorLevel> GetLevels();
    ActorLevel GetLevel(int number);
    ActorLevel CalculateLevel(int sumOfAttributes);
    List<SkillLevel> GetSkillLevels();
    List<SkillType> GetSkillTypes();

    // Tile definitions
    List<TileDefinition> GetTileDefinitions();
    TileDefinition GetTileDefinition(int id);

    // World maps
    WorldMap GetWorldMap(string name);
    List<MapChunk> GetChunksForWorld(int worldMapId);
    MapChunk GetChunk(int worldMapId, int chunkX, int chunkY);

    // Object definitions
    List<ObjectDefinition> GetObjectDefinitions();
    List<MapObject> GetObjectsForChunk(int mapChunkId);

    // Write operations for world generation
    void SaveChunk(MapChunk chunk);
    void SaveMapObject(MapObject mapObject);
    void SaveWorldMap(WorldMap worldMap);
}
