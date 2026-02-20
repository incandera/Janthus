using Janthus.Model.Entities;
using Janthus.Model.Enums;

namespace Janthus.Model.Services;

public static class CombatCalculator
{
    public static decimal CalculateAttackRating(LeveledActor actor, List<Skill> skills, IGameDataProvider dataProvider)
    {
        var baseRating = actor.EffectiveStrength * 1.5m + actor.TotalEquipmentAttackRating;
        var skillMod = GetCombatSkillModifier(skills, dataProvider);
        return baseRating * (1.0m + skillMod * 0.5m);
    }

    public static decimal CalculateArmorRating(LeveledActor actor, List<Skill> skills, IGameDataProvider dataProvider)
    {
        var baseRating = actor.EffectiveConstitution * 0.5m + actor.TotalEquipmentArmorRating;
        var skillMod = GetCombatSkillModifier(skills, dataProvider);
        return baseRating * (1.0m + skillMod * 0.3m);
    }

    public static int CalculateDamage(LeveledActor attacker, List<Skill> attackerSkills,
                                       LeveledActor defender, List<Skill> defenderSkills,
                                       IGameDataProvider dataProvider, Random rng)
    {
        var attackRating = CalculateAttackRating(attacker, attackerSkills, dataProvider);
        var armorRating = CalculateArmorRating(defender, defenderSkills, dataProvider);

        var rawDamage = Math.Max(1m, attackRating - armorRating * 0.5m);

        // Size ratio modifier
        var defenderSize = defender.SizeMultiplier == 0 ? 1m : defender.SizeMultiplier;
        var attackerSize = attacker.SizeMultiplier == 0 ? 1m : attacker.SizeMultiplier;
        var sizeRatio = Math.Clamp(attackerSize / defenderSize, 0.25m, 4.0m);

        decimal sizeMod;
        if (sizeRatio < 1m)
            sizeMod = sizeRatio;
        else
            sizeMod = 1m + (sizeRatio - 1m) * 0.5m;

        // Luck swing
        var netLuck = attacker.EffectiveLuck - defender.EffectiveLuck;
        var variance = 0.04m * Math.Abs(netLuck);
        var bias = 0.02m * netLuck;
        var luckRoll = (decimal)(rng.NextDouble() * 2 - 1); // -1 to 1
        var luckMod = Math.Clamp(1.0m + bias + luckRoll * variance, 0.5m, 2.0m);

        var finalDamage = Math.Max(1, (int)Math.Round(rawDamage * sizeMod * luckMod));
        return finalDamage;
    }

    public static bool RollHit(LeveledActor attacker, List<Skill> attackerSkills,
                                LeveledActor defender, IGameDataProvider dataProvider, Random rng)
    {
        var combatSkill = GetCombatSkillModifier(attackerSkills, dataProvider);
        var hitChance = 0.7m + (attacker.EffectiveDexterity - defender.EffectiveDexterity) * 0.03m + combatSkill * 0.2m;
        hitChance = Math.Clamp(hitChance, 0.1m, 0.95m);
        return (decimal)rng.NextDouble() < hitChance;
    }

    public static Item Equip(LeveledActor actor, List<InventoryItem> inventory, Item item)
    {
        if (item.Slot == EquipmentSlot.None) return null;

        Item previousItem = null;
        if (actor.Equipment.TryGetValue(item.Slot, out var existing))
        {
            previousItem = existing;
            Unequip(actor, inventory, item.Slot);
        }

        // Remove item from inventory
        var invItem = inventory.Find(i => i.Item.Id == item.Id);
        if (invItem != null)
        {
            invItem.Quantity--;
            if (invItem.Quantity <= 0)
                inventory.Remove(invItem);
        }

        actor.Equipment[item.Slot] = item;
        return previousItem;
    }

    public static string TryConsumeItem(LeveledActor actor, List<InventoryItem> inventory, Item item)
    {
        if (item.Slot != EquipmentSlot.None) return null;

        string message = null;
        switch (item.Name)
        {
            case "Health Potion":
                var hpRestore = (decimal)actor.MaximumHitPoints * 0.4m;
                actor.CurrentHitPoints = Math.Min(actor.CurrentHitPoints + hpRestore, (decimal)actor.MaximumHitPoints);
                message = $"Restored {hpRestore:F0} HP";
                break;

            case "Mana Potion":
                var mpRestore = (decimal)actor.MaximumMana * 0.4m;
                actor.CurrentMana = Math.Min(actor.CurrentMana + mpRestore, (decimal)actor.MaximumMana);
                message = $"Restored {mpRestore:F0} MP";
                break;

            case "Antidote":
                message = "Cured poison";
                break;

            default:
                return null;
        }

        // Remove one from inventory
        var invItem = inventory.Find(i => i.Item.Id == item.Id);
        if (invItem != null)
        {
            invItem.Quantity--;
            if (invItem.Quantity <= 0)
                inventory.Remove(invItem);
        }

        return message;
    }

    public static bool Unequip(LeveledActor actor, List<InventoryItem> inventory, EquipmentSlot slot)
    {
        if (!actor.Equipment.TryGetValue(slot, out var item))
            return false;

        actor.Equipment.Remove(slot);

        // Return to inventory
        var existing = inventory.Find(i => i.Item.Id == item.Id);
        if (existing != null)
            existing.Quantity++;
        else
            inventory.Add(new InventoryItem(item));

        return true;
    }

    private static decimal GetCombatSkillModifier(List<Skill> skills, IGameDataProvider dataProvider)
    {
        var combatSkill = skills.Find(s => s.Type != null && s.Type.Name == "Combat");
        if (combatSkill?.Level == null) return 0m;

        var skillLevels = dataProvider.GetSkillLevels();
        var level = skillLevels.Find(sl => sl.Name == combatSkill.Level.Name);
        if (level == null) return 0m;

        return (level.ConferredEffectivenessMinimum + level.ConferredEffectivenessMaximum) / 2.0m;
    }
}
