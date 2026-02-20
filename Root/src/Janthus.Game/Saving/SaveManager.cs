using System.Text.Json;
using Microsoft.Xna.Framework;
using Janthus.Model.Entities;
using Janthus.Model.Enums;
using Janthus.Model.Services;
using Janthus.Game.Actors;
using Janthus.Game.GameState;
using Janthus.Game.World;

namespace Janthus.Game.Saving;

public static class SaveManager
{
    public const int MaxSlots = 3;

    private static readonly string SaveDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Janthus", "saves");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private static string GetSlotPath(int slot) =>
        Path.Combine(SaveDirectory, $"save_{slot}.json");

    public static GameSaveData CaptureState(PlayingState state, IGameDataProvider dataProvider)
    {
        var saveData = new GameSaveData
        {
            SaveName = state.PlayerController.Sprite.Label,
            SaveTime = DateTime.UtcNow,
            Player = BuildActorSaveData(state.PlayerController.Sprite),
            Camera = new CameraSaveData
            {
                X = state.Camera.Position.X,
                Y = state.Camera.Position.Y,
                Zoom = state.Camera.Zoom
            }
        };

        foreach (var npc in state.NpcControllers)
        {
            saveData.Npcs.Add(BuildActorSaveData(npc.Sprite));
        }

        foreach (var flag in dataProvider.GetGameFlags())
        {
            saveData.GameFlags.Add(new FlagSaveData { Name = flag.Name, Value = flag.Value });
        }

        return saveData;
    }

    public static void SaveToSlot(int slot, GameSaveData data)
    {
        try
        {
            Directory.CreateDirectory(SaveDirectory);
            var json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(GetSlotPath(slot), json);
        }
        catch
        {
            // Non-critical — silently ignore write failures
        }
    }

    public static GameSaveData LoadFromSlot(int slot)
    {
        try
        {
            var path = GetSlotPath(slot);
            if (!File.Exists(path)) return null;
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<GameSaveData>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public static SaveSlotInfo[] GetSlotSummaries()
    {
        var summaries = new SaveSlotInfo[MaxSlots];
        for (int i = 0; i < MaxSlots; i++)
        {
            var slot = i + 1;
            summaries[i] = new SaveSlotInfo { Slot = slot };

            try
            {
                var path = GetSlotPath(slot);
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    var data = JsonSerializer.Deserialize<GameSaveData>(json, JsonOptions);
                    if (data != null)
                    {
                        summaries[i].Exists = true;
                        summaries[i].SaveName = data.SaveName;
                        summaries[i].SaveTime = data.SaveTime;
                    }
                }
            }
            catch
            {
                // Corrupt file — treat as empty
            }
        }

        return summaries;
    }

    public static GameSaveData LoadMostRecent()
    {
        var summaries = GetSlotSummaries();
        SaveSlotInfo best = null;
        foreach (var s in summaries)
        {
            if (s.Exists && (best == null || s.SaveTime > best.SaveTime))
                best = s;
        }

        if (best == null) return null;
        return LoadFromSlot(best.Slot);
    }

    public static bool AnySavesExist()
    {
        var summaries = GetSlotSummaries();
        foreach (var s in summaries)
        {
            if (s.Exists) return true;
        }
        return false;
    }

    private static ActorSaveData BuildActorSaveData(ActorSprite sprite)
    {
        var actor = sprite.DomainActor;
        var leveled = actor as LeveledActor;
        var data = new ActorSaveData
        {
            Name = sprite.Label,
            CurrentHitPoints = actor.CurrentHitPoints,
            CurrentMana = actor.CurrentMana,
            Status = actor.Status.ToString(),
            TileX = sprite.TileX,
            TileY = sprite.TileY,
            IsAdversary = sprite.IsAdversary,
            Color = sprite.Color.PackedValue
        };

        if (leveled != null)
        {
            data.Constitution = leveled.Constitution.Value;
            data.Dexterity = leveled.Dexterity.Value;
            data.Intelligence = leveled.Intelligence.Value;
            data.Luck = leveled.Luck.Value;
            data.Attunement = leveled.Attunement.Value;
            data.Strength = leveled.Strength.Value;
            data.Willpower = leveled.Willpower.Value;

            foreach (var kvp in leveled.Equipment)
            {
                data.Equipment.Add(new EquipmentSaveData
                {
                    Slot = kvp.Key.ToString(),
                    ItemId = kvp.Value.Id
                });
            }
        }

        // Gold, inventory, skills — check for player or NPC
        if (actor is PlayerCharacter pc)
        {
            data.Gold = pc.Gold;
            data.Lawfulness = pc.Alignment.Lawfulness.ToString();
            data.Disposition = pc.Alignment.Disposition.ToString();

            foreach (var inv in pc.Inventory)
            {
                data.Inventory.Add(new InventorySaveData { ItemId = inv.Item.Id, Quantity = inv.Quantity });
            }

            foreach (var skill in pc.Skills)
            {
                data.Skills.Add(new SkillSaveData { SkillTypeId = skill.Type.Id, SkillLevelId = skill.Level.Id });
            }
        }
        else if (actor is NonPlayerCharacter npc)
        {
            data.Gold = npc.Gold;
            data.Lawfulness = npc.Alignment.Lawfulness.ToString();
            data.Disposition = npc.Alignment.Disposition.ToString();

            foreach (var inv in npc.Inventory)
            {
                data.Inventory.Add(new InventorySaveData { ItemId = inv.Item.Id, Quantity = inv.Quantity });
            }

            foreach (var skill in npc.Skills)
            {
                data.Skills.Add(new SkillSaveData { SkillTypeId = skill.Type.Id, SkillLevelId = skill.Level.Id });
            }
        }

        return data;
    }
}
