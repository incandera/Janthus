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
    void SaveMapObjects(List<MapObject> mapObjects);
    void SaveWorldMap(WorldMap worldMap);

    // Conversations
    List<Conversation> GetConversationsForNpc(string npcName);
    ConversationNode GetConversationNode(int nodeId);
    List<ConversationResponse> GetResponsesForNode(int nodeId);
    List<ConversationCondition> GetConditionsForConversation(int conversationId);
    List<ConversationCondition> GetConditionsForResponse(int responseId);
    List<ConversationAction> GetActionsForResponse(int responseId);

    // Items and trade
    List<ItemType> GetItemTypes();
    List<Item> GetItems();
    Item GetItem(int id);
    Item GetItemByName(string name);
    List<MerchantStock> GetMerchantStock(string npcName);

    // Inspect descriptions
    List<InspectDescription> GetInspectDescriptions(string targetType, string targetKey);

    // Game flags
    List<GameFlag> GetGameFlags();
    GameFlag GetGameFlag(string name);
    void SetGameFlag(string name, string value);
    void ClearGameFlag(string name);
    void ClearAllGameFlags();
}
