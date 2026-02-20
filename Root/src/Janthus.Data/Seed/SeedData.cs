using Microsoft.EntityFrameworkCore;
using Janthus.Model.Entities;

namespace Janthus.Data.Seed;

public static class SeedData
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        SeedCharacterClasses(modelBuilder);
        SeedActorLevels(modelBuilder);
        SeedActorTypes(modelBuilder);
        SeedSkillTypes(modelBuilder);
        SeedSkillLevels(modelBuilder);
        SeedTileDefinitions(modelBuilder);
        SeedObjectDefinitions(modelBuilder);
        SeedWorldMaps(modelBuilder);
    }

    private static void SeedCharacterClasses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CharacterClass>().HasData(
            new CharacterClass
            {
                Id = 1, Name = "Soldier", Description = "A disciplined warrior favoring strength and constitution.",
                ConstitutionRollWeight = 0.20, DexterityRollWeight = 0.10, IntelligenceRollWeight = 0.05,
                LuckRollWeight = 0.10, AttunementRollWeight = 0.05, StrengthRollWeight = 0.30, WillpowerRollWeight = 0.20
            },
            new CharacterClass
            {
                Id = 2, Name = "Mage", Description = "A wielder of arcane power, strong in attunement and intelligence.",
                ConstitutionRollWeight = 0.05, DexterityRollWeight = 0.05, IntelligenceRollWeight = 0.25,
                LuckRollWeight = 0.10, AttunementRollWeight = 0.30, StrengthRollWeight = 0.05, WillpowerRollWeight = 0.20
            },
            new CharacterClass
            {
                Id = 3, Name = "Rogue", Description = "A cunning operator favoring dexterity and luck.",
                ConstitutionRollWeight = 0.10, DexterityRollWeight = 0.30, IntelligenceRollWeight = 0.10,
                LuckRollWeight = 0.20, AttunementRollWeight = 0.05, StrengthRollWeight = 0.10, WillpowerRollWeight = 0.15
            },
            new CharacterClass
            {
                Id = 4, Name = "Cleric", Description = "A divine servant balancing willpower and attunement.",
                ConstitutionRollWeight = 0.15, DexterityRollWeight = 0.05, IntelligenceRollWeight = 0.15,
                LuckRollWeight = 0.10, AttunementRollWeight = 0.20, StrengthRollWeight = 0.10, WillpowerRollWeight = 0.25
            },
            new CharacterClass
            {
                Id = 5, Name = "Ranger", Description = "A versatile wilderness fighter with balanced attributes.",
                ConstitutionRollWeight = 0.15, DexterityRollWeight = 0.25, IntelligenceRollWeight = 0.10,
                LuckRollWeight = 0.10, AttunementRollWeight = 0.10, StrengthRollWeight = 0.15, WillpowerRollWeight = 0.15
            }
        );
    }

    private static void SeedActorLevels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActorLevel>().HasData(
            new ActorLevel { Id = 1, Number = 1, MinimumSumOfAttributes = 7, LevelRankGroupName = "Novice" },
            new ActorLevel { Id = 2, Number = 2, MinimumSumOfAttributes = 14, LevelRankGroupName = "Novice" },
            new ActorLevel { Id = 3, Number = 3, MinimumSumOfAttributes = 21, LevelRankGroupName = "Novice" },
            new ActorLevel { Id = 4, Number = 4, MinimumSumOfAttributes = 28, LevelRankGroupName = "Novice" },
            new ActorLevel { Id = 5, Number = 5, MinimumSumOfAttributes = 35, LevelRankGroupName = "Apprentice" },
            new ActorLevel { Id = 6, Number = 6, MinimumSumOfAttributes = 42, LevelRankGroupName = "Apprentice" },
            new ActorLevel { Id = 7, Number = 7, MinimumSumOfAttributes = 49, LevelRankGroupName = "Apprentice" },
            new ActorLevel { Id = 8, Number = 8, MinimumSumOfAttributes = 56, LevelRankGroupName = "Apprentice" },
            new ActorLevel { Id = 9, Number = 9, MinimumSumOfAttributes = 63, LevelRankGroupName = "Journeyman" },
            new ActorLevel { Id = 10, Number = 10, MinimumSumOfAttributes = 70, LevelRankGroupName = "Journeyman" },
            new ActorLevel { Id = 11, Number = 11, MinimumSumOfAttributes = 77, LevelRankGroupName = "Journeyman" },
            new ActorLevel { Id = 12, Number = 12, MinimumSumOfAttributes = 84, LevelRankGroupName = "Journeyman" },
            new ActorLevel { Id = 13, Number = 13, MinimumSumOfAttributes = 91, LevelRankGroupName = "Expert" },
            new ActorLevel { Id = 14, Number = 14, MinimumSumOfAttributes = 98, LevelRankGroupName = "Expert" },
            new ActorLevel { Id = 15, Number = 15, MinimumSumOfAttributes = 105, LevelRankGroupName = "Expert" },
            new ActorLevel { Id = 16, Number = 16, MinimumSumOfAttributes = 112, LevelRankGroupName = "Expert" },
            new ActorLevel { Id = 17, Number = 17, MinimumSumOfAttributes = 119, LevelRankGroupName = "Master" },
            new ActorLevel { Id = 18, Number = 18, MinimumSumOfAttributes = 126, LevelRankGroupName = "Master" },
            new ActorLevel { Id = 19, Number = 19, MinimumSumOfAttributes = 133, LevelRankGroupName = "Master" },
            new ActorLevel { Id = 20, Number = 20, MinimumSumOfAttributes = 140, LevelRankGroupName = "Master" }
        );
    }

    private static void SeedActorTypes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActorType>().HasData(
            new ActorType { Id = 1, Name = "Humanoid", Description = "Human-like creatures including humans, elves, and dwarves." },
            new ActorType { Id = 2, Name = "Beast", Description = "Natural animals and monstrous fauna." },
            new ActorType { Id = 3, Name = "Undead", Description = "Reanimated or cursed creatures." },
            new ActorType { Id = 4, Name = "Elemental", Description = "Beings of pure elemental energy." },
            new ActorType { Id = 5, Name = "Construct", Description = "Magically or mechanically animated objects." }
        );
    }

    private static void SeedSkillTypes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SkillType>().HasData(
            new SkillType { Id = 1, Name = "Combat", Description = "Proficiency in martial weapons and fighting techniques." },
            new SkillType { Id = 2, Name = "Magic", Description = "Mastery of arcane and divine spellcasting." },
            new SkillType { Id = 3, Name = "Stealth", Description = "Ability to move unseen and act covertly." },
            new SkillType { Id = 4, Name = "Crafting", Description = "Knowledge of item creation and material working." },
            new SkillType { Id = 5, Name = "Diplomacy", Description = "Skill in persuasion, negotiation, and social interaction." }
        );
    }

    private static void SeedSkillLevels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SkillLevel>().HasData(
            new SkillLevel { Id = 1, Name = "Novice", Description = "Basic understanding.", ConferredEffectivenessMinimum = 0.0m, ConferredEffectivenessMaximum = 0.2m },
            new SkillLevel { Id = 2, Name = "Apprentice", Description = "Developing proficiency.", ConferredEffectivenessMinimum = 0.2m, ConferredEffectivenessMaximum = 0.4m },
            new SkillLevel { Id = 3, Name = "Journeyman", Description = "Competent practitioner.", ConferredEffectivenessMinimum = 0.4m, ConferredEffectivenessMaximum = 0.6m },
            new SkillLevel { Id = 4, Name = "Expert", Description = "Highly skilled specialist.", ConferredEffectivenessMinimum = 0.6m, ConferredEffectivenessMaximum = 0.8m },
            new SkillLevel { Id = 5, Name = "Master", Description = "Unmatched mastery.", ConferredEffectivenessMinimum = 0.8m, ConferredEffectivenessMaximum = 1.0m }
        );
    }

    private static void SeedTileDefinitions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TileDefinition>().HasData(
            new TileDefinition { Id = 1, Name = "Grass", Description = "Lush green grass.", ColorHex = "#228B22", IsWalkable = true, BaseMovementCost = 1.0f },
            new TileDefinition { Id = 2, Name = "Water", Description = "Deep water.", ColorHex = "#1E5AC8", IsWalkable = false, BaseMovementCost = 0f },
            new TileDefinition { Id = 3, Name = "Stone", Description = "Solid stone ground.", ColorHex = "#808080", IsWalkable = true, BaseMovementCost = 0.8f },
            new TileDefinition { Id = 4, Name = "Sand", Description = "Loose sandy ground.", ColorHex = "#D2B464", IsWalkable = true, BaseMovementCost = 1.3f },
            new TileDefinition { Id = 5, Name = "Dark Grass", Description = "Dense dark vegetation.", ColorHex = "#146414", IsWalkable = true, BaseMovementCost = 1.1f }
        );
    }

    private static void SeedObjectDefinitions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ObjectDefinition>().HasData(
            new ObjectDefinition { Id = 1, Name = "Tree", Description = "A tall tree.", IsPassable = false, MovementCostModifier = 0f, BlocksLineOfSight = true },
            new ObjectDefinition { Id = 2, Name = "Boulder", Description = "A large boulder.", IsPassable = false, MovementCostModifier = 0f, BlocksLineOfSight = true },
            new ObjectDefinition { Id = 3, Name = "Wall", Description = "A stone wall.", IsPassable = false, MovementCostModifier = 0f, BlocksLineOfSight = true }
        );
    }

    private static void SeedWorldMaps(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorldMap>().HasData(
            new WorldMap { Id = 1, Name = "Default", Seed = 42, ChunkSize = 32, ChunkCountX = 3, ChunkCountY = 3 }
        );
    }
}
