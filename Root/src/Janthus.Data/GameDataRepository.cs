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
    private List<Conversation> _conversations;
    private List<ConversationNode> _conversationNodes;
    private List<ConversationResponse> _conversationResponses;
    private List<ConversationCondition> _conversationConditions;
    private List<ConversationAction> _conversationActions;
    private List<ItemType> _itemTypes;
    private List<Item> _items;
    private List<MerchantStock> _merchantStock;
    private List<InspectDescription> _inspectDescriptions;
    private List<InspectCondition> _inspectConditions;

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

    public void SaveMapObjects(List<MapObject> mapObjects)
    {
        _context.MapObjects.AddRange(mapObjects);
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

    public List<Conversation> GetConversationsForNpc(string npcName)
    {
        _conversations ??= _context.Conversations.ToList();
        _conversationConditions ??= _context.ConversationConditions.ToList();

        var conversations = _conversations
            .Where(c => c.NpcName == npcName)
            .OrderByDescending(c => c.Priority)
            .ToList();

        foreach (var conv in conversations)
        {
            conv.Conditions = _conversationConditions
                .Where(c => c.ConversationId == conv.Id).ToList();
        }

        return conversations;
    }

    public ConversationNode GetConversationNode(int nodeId)
    {
        _conversationNodes ??= _context.ConversationNodes.ToList();
        return _conversationNodes.SingleOrDefault(n => n.Id == nodeId);
    }

    public List<ConversationResponse> GetResponsesForNode(int nodeId)
    {
        _conversationResponses ??= _context.ConversationResponses.ToList();
        _conversationConditions ??= _context.ConversationConditions.ToList();
        _conversationActions ??= _context.ConversationActions.ToList();

        var responses = _conversationResponses
            .Where(r => r.NodeId == nodeId)
            .OrderBy(r => r.SortOrder)
            .ToList();

        foreach (var resp in responses)
        {
            resp.Conditions = _conversationConditions
                .Where(c => c.ResponseId == resp.Id).ToList();
            resp.Actions = _conversationActions
                .Where(a => a.ResponseId == resp.Id).ToList();
        }

        return responses;
    }

    public List<ConversationCondition> GetConditionsForConversation(int conversationId)
    {
        _conversationConditions ??= _context.ConversationConditions.ToList();
        return _conversationConditions.Where(c => c.ConversationId == conversationId).ToList();
    }

    public List<ConversationCondition> GetConditionsForResponse(int responseId)
    {
        _conversationConditions ??= _context.ConversationConditions.ToList();
        return _conversationConditions.Where(c => c.ResponseId == responseId).ToList();
    }

    public List<ConversationAction> GetActionsForResponse(int responseId)
    {
        _conversationActions ??= _context.ConversationActions.ToList();
        return _conversationActions.Where(a => a.ResponseId == responseId).ToList();
    }

    public List<ItemType> GetItemTypes()
    {
        _itemTypes ??= _context.ItemTypes.OrderBy(x => x.Id).ToList();
        return _itemTypes;
    }

    public List<Item> GetItems()
    {
        if (_items == null)
        {
            var itemTypes = GetItemTypes();
            _items = _context.Items.OrderBy(x => x.Id).ToList();
            foreach (var item in _items)
            {
                var typeId = _context.Entry(item).Property<int>("ItemTypeId").CurrentValue;
                item.Type = itemTypes.SingleOrDefault(t => t.Id == typeId);
            }
        }
        return _items;
    }

    public Item GetItem(int id)
    {
        return GetItems().SingleOrDefault(x => x.Id == id);
    }

    public Item GetItemByName(string name)
    {
        return GetItems().SingleOrDefault(x =>
            string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public List<MerchantStock> GetMerchantStock(string npcName)
    {
        _merchantStock ??= _context.MerchantStocks.ToList();
        return _merchantStock.Where(s => s.NpcName == npcName).ToList();
    }

    public List<InspectDescription> GetInspectDescriptions(string targetType, string targetKey)
    {
        _inspectDescriptions ??= _context.InspectDescriptions.ToList();
        _inspectConditions ??= _context.InspectConditions.ToList();

        var descriptions = _inspectDescriptions
            .Where(d => d.TargetType == targetType && d.TargetKey == targetKey)
            .OrderByDescending(d => d.Priority)
            .ToList();

        foreach (var desc in descriptions)
        {
            desc.Conditions = _inspectConditions
                .Where(c => c.InspectDescriptionId == desc.Id).ToList();
        }

        return descriptions;
    }

    public List<GameFlag> GetGameFlags()
    {
        return _context.GameFlags.ToList();
    }

    public GameFlag GetGameFlag(string name)
    {
        return _context.GameFlags.SingleOrDefault(f => f.Name == name);
    }

    public void SetGameFlag(string name, string value)
    {
        var flag = _context.GameFlags.SingleOrDefault(f => f.Name == name);
        if (flag == null)
        {
            flag = new GameFlag { Name = name, Value = value };
            _context.GameFlags.Add(flag);
        }
        else
        {
            flag.Value = value;
        }
        _context.SaveChanges();
    }

    public void ClearGameFlag(string name)
    {
        var flag = _context.GameFlags.SingleOrDefault(f => f.Name == name);
        if (flag != null)
        {
            _context.GameFlags.Remove(flag);
            _context.SaveChanges();
        }
    }

    public void ClearAllGameFlags()
    {
        var flags = _context.GameFlags.ToList();
        _context.GameFlags.RemoveRange(flags);
        _context.SaveChanges();
    }
}
