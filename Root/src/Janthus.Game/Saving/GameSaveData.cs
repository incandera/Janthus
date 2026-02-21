namespace Janthus.Game.Saving;

public class GameSaveData
{
    public string SaveName { get; set; } = string.Empty;
    public DateTime SaveTime { get; set; }
    public ActorSaveData Player { get; set; }
    public List<ActorSaveData> Npcs { get; set; } = new();
    public CameraSaveData Camera { get; set; }
    public List<FlagSaveData> GameFlags { get; set; } = new();
    public Dictionary<string, byte[]> ChunkVisibility { get; set; } = new();
    public float TimeOfDay { get; set; } = 10f;
}

public class ActorSaveData
{
    public string Name { get; set; } = string.Empty;
    public int Constitution { get; set; }
    public int Dexterity { get; set; }
    public int Intelligence { get; set; }
    public int Luck { get; set; }
    public int Attunement { get; set; }
    public int Strength { get; set; }
    public int Willpower { get; set; }
    public decimal CurrentHitPoints { get; set; }
    public decimal CurrentMana { get; set; }
    public decimal Gold { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Lawfulness { get; set; } = string.Empty;
    public string Disposition { get; set; } = string.Empty;
    public int TileX { get; set; }
    public int TileY { get; set; }
    public bool IsAdversary { get; set; }
    public int Facing { get; set; }
    public uint Color { get; set; }
    public List<InventorySaveData> Inventory { get; set; } = new();
    public List<EquipmentSaveData> Equipment { get; set; } = new();
    public List<SkillSaveData> Skills { get; set; } = new();
}

public class InventorySaveData
{
    public int ItemId { get; set; }
    public int Quantity { get; set; }
}

public class EquipmentSaveData
{
    public string Slot { get; set; } = string.Empty;
    public int ItemId { get; set; }
}

public class SkillSaveData
{
    public int SkillTypeId { get; set; }
    public int SkillLevelId { get; set; }
}

public class CameraSaveData
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Zoom { get; set; }
}

public class FlagSaveData
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class SaveSlotInfo
{
    public int Slot { get; set; }
    public string SaveName { get; set; } = string.Empty;
    public DateTime SaveTime { get; set; }
    public bool Exists { get; set; }
}
