using Microsoft.EntityFrameworkCore;
using Janthus.Model.Entities;
using Janthus.Model.Enums;

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
        SeedConversations(modelBuilder);
        SeedItemTypes(modelBuilder);
        SeedItems(modelBuilder);
        SeedMerchantStock(modelBuilder);
        SeedInspectDescriptions(modelBuilder);
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
            new TileDefinition { Id = 3, Name = "Sand", Description = "Loose sandy ground.", ColorHex = "#D2B464", IsWalkable = true, BaseMovementCost = 1.3f },
            new TileDefinition { Id = 4, Name = "Stone", Description = "Solid stone ground.", ColorHex = "#808080", IsWalkable = true, BaseMovementCost = 0.8f },
            new TileDefinition { Id = 5, Name = "Dirt", Description = "Packed earth and dried mud.", ColorHex = "#8B7355", IsWalkable = true, BaseMovementCost = 1.1f }
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

    private static void SeedConversations(ModelBuilder modelBuilder)
    {
        // ========== CONVERSATIONS ==========
        modelBuilder.Entity<Conversation>().HasData(
            // Guard conversations
            new Conversation { Id = 1, NpcName = "Guard", Title = "Guard - Greeting", Priority = 0, EntryNodeId = 1, IsRepeatable = true },
            new Conversation { Id = 2, NpcName = "Guard", Title = "Guard - Quest Complete", Priority = 10, EntryNodeId = 5, IsRepeatable = false },
            // Merchant conversations
            new Conversation { Id = 3, NpcName = "Merchant", Title = "Merchant - Default Greeting", Priority = 0, EntryNodeId = 10, IsRepeatable = true },
            // Mage conversations
            new Conversation { Id = 4, NpcName = "Mage", Title = "Mage - Wounded Intro", Priority = 10, EntryNodeId = 20, IsRepeatable = false },
            new Conversation { Id = 5, NpcName = "Mage", Title = "Mage - Health Potion", Priority = 5, EntryNodeId = 25, IsRepeatable = false },
            new Conversation { Id = 6, NpcName = "Mage", Title = "Mage - Quest Active", Priority = 3, EntryNodeId = 30, IsRepeatable = true },
            new Conversation { Id = 7, NpcName = "Mage", Title = "Mage - Quest Complete", Priority = 15, EntryNodeId = 32, IsRepeatable = false },
            new Conversation { Id = 8, NpcName = "Mage", Title = "Mage - Join Party", Priority = 20, EntryNodeId = 35, IsRepeatable = false }
        );

        // ========== NODES ==========
        modelBuilder.Entity<ConversationNode>().HasData(
            // --- Guard - Greeting (Conv 1) ---
            new ConversationNode { Id = 1, ConversationId = 1, SpeakerName = "Guard",
                Text = "Hail, traveler. These roads have grown dangerous of late. Keep your wits about you." },
            new ConversationNode { Id = 2, ConversationId = 1, SpeakerName = "Guard",
                Text = "Aye, bandits and mercenaries have been prowling the outskirts. A Mage was attacked not far from here. If you're looking to survive, the Merchant has a rare Dunedain Amulet that brings great fortune." },
            new ConversationNode { Id = 3, ConversationId = 1, SpeakerName = "Guard",
                Text = "Stay safe out there.", IsEndNode = true },
            new ConversationNode { Id = 4, ConversationId = 1, SpeakerName = "Guard",
                Text = "You might also want to check on the Mage to the east. Last I heard, he was in bad shape.", IsEndNode = true },

            // --- Guard - Quest Complete (Conv 2) ---
            new ConversationNode { Id = 5, ConversationId = 2, SpeakerName = "Guard",
                Text = "I heard you retrieved the Key of Stratholme! The Mage will be most grateful. You've done a great service to this region." },
            new ConversationNode { Id = 6, ConversationId = 2, SpeakerName = "Guard",
                Text = "Perhaps there will be peace in these parts again. Fare well, hero.", IsEndNode = true },

            // --- Merchant - Default Greeting (Conv 3) ---
            new ConversationNode { Id = 10, ConversationId = 3, SpeakerName = "Merchant",
                Text = "Welcome, traveler! I have fine wares for sale. What brings you to my shop?" },
            new ConversationNode { Id = 11, ConversationId = 3, SpeakerName = "Merchant",
                Text = "Come back anytime! I always have something new.", IsEndNode = true },
            new ConversationNode { Id = 12, ConversationId = 3, SpeakerName = "Merchant",
                Text = "Ah, the guard sent you! Yes, I have the Dunedain Amulet. An ancient relic of immense fortune. Not something I part with lightly." },
            new ConversationNode { Id = 13, ConversationId = 3, SpeakerName = "Merchant",
                Text = "Three hundred gold pieces. The fortune it brings is worth ten times that." },
            new ConversationNode { Id = 14, ConversationId = 3, SpeakerName = "Merchant",
                Text = "A wise purchase! The Dunedain Amulet has turned the tide for many a warrior. May fortune smile upon you.",
                IsEndNode = true },
            new ConversationNode { Id = 15, ConversationId = 3, SpeakerName = "Merchant",
                Text = "Now, now, let's not cause trouble. Perhaps I can offer you a small discount?" },
            new ConversationNode { Id = 16, ConversationId = 3, SpeakerName = "Merchant",
                Text = "You can feel it too? I have a few items of arcane interest. Not for everyone." },

            // --- Mage - Wounded Intro (Conv 4) ---
            new ConversationNode { Id = 20, ConversationId = 4, SpeakerName = "Mage",
                Text = "Please... help me. I was ambushed by mercenaries on the road. They took the Key of Stratholme -- my most precious artifact." },
            new ConversationNode { Id = 21, ConversationId = 4, SpeakerName = "Mage",
                Text = "The Key opens an ancient vault to the north. In the wrong hands... it could be disastrous. I'm too wounded to pursue them myself." },
            new ConversationNode { Id = 22, ConversationId = 4, SpeakerName = "Mage",
                Text = "Please, if you can defeat the mercenaries and retrieve the Key, I would be forever in your debt. They are formidable fighters -- do not engage them unprepared.",
                IsEndNode = true },
            new ConversationNode { Id = 23, ConversationId = 4, SpeakerName = "Mage",
                Text = "I understand. Be careful out there.", IsEndNode = true },

            // --- Mage - Health Potion (Conv 5) ---
            new ConversationNode { Id = 25, ConversationId = 5, SpeakerName = "Mage",
                Text = "My wounds still trouble me greatly. If you happen to have a Health Potion, I would be most grateful." },
            new ConversationNode { Id = 26, ConversationId = 5, SpeakerName = "Mage",
                Text = "Bless you! I can already feel my strength returning. You are a true friend.",
                IsEndNode = true },
            new ConversationNode { Id = 27, ConversationId = 5, SpeakerName = "Mage",
                Text = "I understand. Perhaps the Merchant sells them. Please hurry -- I grow weaker by the hour.",
                IsEndNode = true },

            // --- Mage - Quest Active (Conv 6) ---
            new ConversationNode { Id = 30, ConversationId = 6, SpeakerName = "Mage",
                Text = "The mercenaries still have my Key. Please, be careful when you face them. They are dangerous fighters." },
            new ConversationNode { Id = 31, ConversationId = 6, SpeakerName = "Mage",
                Text = "I believe in you. Good luck.", IsEndNode = true },

            // --- Mage - Quest Complete (Conv 7) ---
            new ConversationNode { Id = 32, ConversationId = 7, SpeakerName = "Mage",
                Text = "You have it! The Key of Stratholme! I cannot thank you enough. You have saved not only my life's work, but perhaps the entire region." },
            new ConversationNode { Id = 33, ConversationId = 7, SpeakerName = "Mage",
                Text = "Please, take this gold as a reward. You have more than earned it. May we meet again under better circumstances.",
                IsEndNode = true },

            // --- Mage - Join Party (Conv 8) ---
            new ConversationNode { Id = 35, ConversationId = 8, SpeakerName = "Mage",
                Text = "I have been reflecting on what you did for me. Retrieving the Key, tending to my wounds... I owe you a great debt." },
            new ConversationNode { Id = 36, ConversationId = 8, SpeakerName = "Mage",
                Text = "The road ahead will only grow more dangerous. I may not be a warrior, but my arcane knowledge could prove useful. Would you allow me to travel with you?" },
            new ConversationNode { Id = 37, ConversationId = 8, SpeakerName = "Mage",
                Text = "Then it is settled! Together we shall face whatever lies ahead. Lead on, friend.",
                IsEndNode = true },
            new ConversationNode { Id = 38, ConversationId = 8, SpeakerName = "Mage",
                Text = "I understand. Should you change your mind, you know where to find me.",
                IsEndNode = true }
        );

        // ========== RESPONSES ==========
        modelBuilder.Entity<ConversationResponse>().HasData(
            // --- Guard - Greeting (Conv 1) ---
            // Node 1 responses
            new ConversationResponse { Id = 1, NodeId = 1, Text = "What dangers lie ahead?", NextNodeId = 2, SortOrder = 1 },
            new ConversationResponse { Id = 2, NodeId = 1, Text = "Farewell.", NextNodeId = 3, SortOrder = 2 },
            // Node 2 responses
            new ConversationResponse { Id = 3, NodeId = 2, Text = "Thanks for the tip. I'll visit the Merchant.", NextNodeId = 4, SortOrder = 1 },
            new ConversationResponse { Id = 4, NodeId = 2, Text = "I can handle myself. Farewell.", NextNodeId = 3, SortOrder = 2 },

            // --- Guard - Quest Complete (Conv 2) ---
            // Node 5 responses
            new ConversationResponse { Id = 5, NodeId = 5, Text = "It was the right thing to do.", NextNodeId = 6, SortOrder = 1 },
            new ConversationResponse { Id = 6, NodeId = 5, Text = "Those mercenaries had it coming.", NextNodeId = 6, SortOrder = 2 },

            // --- Merchant - Default Greeting (Conv 3) ---
            // Node 10 responses
            new ConversationResponse { Id = 10, NodeId = 10, Text = "Just browsing, thanks.", NextNodeId = 11, SortOrder = 1 },
            new ConversationResponse { Id = 11, NodeId = 10, Text = "I heard you have a Dunedain Amulet.", NextNodeId = 12, SortOrder = 2 },
            new ConversationResponse { Id = 12, NodeId = 10, Text = "Your prices are unfair!", NextNodeId = 15, SortOrder = 3 },
            new ConversationResponse { Id = 13, NodeId = 10, Text = "I sense arcane energy here...", NextNodeId = 16, SortOrder = 4 },
            // Node 12 responses
            new ConversationResponse { Id = 14, NodeId = 12, Text = "How much?", NextNodeId = 13, SortOrder = 1 },
            new ConversationResponse { Id = 15, NodeId = 12, Text = "Never mind.", NextNodeId = 11, SortOrder = 2 },
            // Node 13 responses
            new ConversationResponse { Id = 16, NodeId = 13, Text = "Deal. Here's three hundred gold.", NextNodeId = 14, SortOrder = 1 },
            new ConversationResponse { Id = 17, NodeId = 13, Text = "Too rich for me.", NextNodeId = 11, SortOrder = 2 },
            // Node 15 responses
            new ConversationResponse { Id = 18, NodeId = 15, Text = "Fine, show me what you have.", NextNodeId = 11, SortOrder = 1 },
            new ConversationResponse { Id = 19, NodeId = 15, Text = "I'll remember this.", NextNodeId = 11, SortOrder = 2 },
            // Node 16 responses
            new ConversationResponse { Id = 20, NodeId = 16, Text = "Show me the arcane wares.", NextNodeId = 11, SortOrder = 1 },
            new ConversationResponse { Id = 21, NodeId = 16, Text = "Interesting. Perhaps later.", NextNodeId = 11, SortOrder = 2 },

            // --- Mage - Wounded Intro (Conv 4) ---
            // Node 20 responses
            new ConversationResponse { Id = 25, NodeId = 20, Text = "What happened to you?", NextNodeId = 21, SortOrder = 1 },
            new ConversationResponse { Id = 26, NodeId = 20, Text = "I can't help right now.", NextNodeId = 23, SortOrder = 2 },
            // Node 21 responses
            new ConversationResponse { Id = 27, NodeId = 21, Text = "I'll get the Key back for you.", NextNodeId = 22, SortOrder = 1 },
            new ConversationResponse { Id = 28, NodeId = 21, Text = "That sounds too dangerous.", NextNodeId = 23, SortOrder = 2 },

            // --- Mage - Health Potion (Conv 5) ---
            // Node 25 responses
            new ConversationResponse { Id = 30, NodeId = 25, Text = "Here, take this Health Potion.", NextNodeId = 26, SortOrder = 1 },
            new ConversationResponse { Id = 31, NodeId = 25, Text = "I don't have one right now.", NextNodeId = 27, SortOrder = 2 },

            // --- Mage - Quest Active (Conv 6) ---
            // Node 30 responses
            new ConversationResponse { Id = 35, NodeId = 30, Text = "I'll bring it back soon.", NextNodeId = 31, SortOrder = 1 },
            new ConversationResponse { Id = 36, NodeId = 30, Text = "Any tips for fighting them?", NextNodeId = 31, SortOrder = 2 },

            // --- Mage - Quest Complete (Conv 7) ---
            // Node 32 responses
            new ConversationResponse { Id = 40, NodeId = 32, Text = "I'm glad I could help.", NextNodeId = 33, SortOrder = 1 },
            new ConversationResponse { Id = 41, NodeId = 32, Text = "It was a tough fight.", NextNodeId = 33, SortOrder = 2 },

            // --- Mage - Join Party (Conv 8) ---
            // Node 35 responses
            new ConversationResponse { Id = 45, NodeId = 35, Text = "What do you have in mind?", NextNodeId = 36, SortOrder = 1 },
            new ConversationResponse { Id = 46, NodeId = 35, Text = "You owe me nothing.", NextNodeId = 36, SortOrder = 2 },
            // Node 36 responses
            new ConversationResponse { Id = 47, NodeId = 36, Text = "I would welcome your company.", NextNodeId = 37, SortOrder = 1 },
            new ConversationResponse { Id = 48, NodeId = 36, Text = "I prefer to travel alone.", NextNodeId = 38, SortOrder = 2 }
        );

        // ========== CONDITIONS ==========
        modelBuilder.Entity<ConversationCondition>().HasData(
            // Guard - Quest Complete (Conv 2): requires key_retrieved flag
            new ConversationCondition { Id = 1, ConversationId = 2, ResponseId = 0,
                ConditionType = ConditionType.FlagSet, Value = "key_retrieved" },

            // Merchant response 11 (ask about Dunedain Amulet): requires talked_to_guard flag
            new ConversationCondition { Id = 2, ConversationId = 0, ResponseId = 11,
                ConditionType = ConditionType.FlagSet, Value = "talked_to_guard" },
            // Merchant response 12 (prices unfair): requires Chaotic lawfulness
            new ConversationCondition { Id = 3, ConversationId = 0, ResponseId = 12,
                ConditionType = ConditionType.PlayerLawfulness, Value = "Chaotic" },
            // Merchant response 13 (arcane energy): requires Mage class
            new ConversationCondition { Id = 4, ConversationId = 0, ResponseId = 13,
                ConditionType = ConditionType.PlayerClass, Value = "Mage" },
            // Merchant response 16 (buy Dunedain Amulet): requires 300+ gold
            new ConversationCondition { Id = 5, ConversationId = 0, ResponseId = 16,
                ConditionType = ConditionType.MinGold, Value = "300" },

            // Mage - Wounded Intro (Conv 4): requires talked_to_mage NOT set
            new ConversationCondition { Id = 6, ConversationId = 4, ResponseId = 0,
                ConditionType = ConditionType.FlagNotSet, Value = "talked_to_mage" },

            // Mage - Health Potion (Conv 5): requires talked_to_mage AND mage NOT healed
            new ConversationCondition { Id = 7, ConversationId = 5, ResponseId = 0,
                ConditionType = ConditionType.FlagSet, Value = "talked_to_mage" },
            new ConversationCondition { Id = 8, ConversationId = 5, ResponseId = 0,
                ConditionType = ConditionType.FlagNotSet, Value = "mage_healed" },

            // Mage - Health Potion response 30 (give potion): requires player has Health Potion (min 1 in inventory)
            // We'll use a flag check — the TakeItem action will handle the actual removal
            // Actually, we need to check the player has a Health Potion. We don't have a HasItem condition type,
            // so we'll leave this unconditional and handle it narratively (player can choose to give even without one,
            // but the action will only work if they have one)

            // Mage - Health Potion response 30 (give potion): requires player has Health Potion
            new ConversationCondition { Id = 13, ConversationId = 0, ResponseId = 30,
                ConditionType = ConditionType.HasItem, Value = "Health Potion" },

            // Mage - Quest Active (Conv 6): requires quest active, key not yet retrieved
            new ConversationCondition { Id = 9, ConversationId = 6, ResponseId = 0,
                ConditionType = ConditionType.FlagSet, Value = "quest_active_retrieve_key" },
            new ConversationCondition { Id = 10, ConversationId = 6, ResponseId = 0,
                ConditionType = ConditionType.FlagNotSet, Value = "key_retrieved" },

            // Mage - Quest Complete (Conv 7): requires key_retrieved flag
            new ConversationCondition { Id = 11, ConversationId = 7, ResponseId = 0,
                ConditionType = ConditionType.FlagSet, Value = "key_retrieved" },
            new ConversationCondition { Id = 12, ConversationId = 7, ResponseId = 0,
                ConditionType = ConditionType.FlagNotSet, Value = "quest_done_retrieve_key" },

            // Mage - Join Party (Conv 8): requires quest complete AND not already in party
            new ConversationCondition { Id = 14, ConversationId = 8, ResponseId = 0,
                ConditionType = ConditionType.FlagSet, Value = "quest_done_retrieve_key" },
            new ConversationCondition { Id = 15, ConversationId = 8, ResponseId = 0,
                ConditionType = ConditionType.FlagNotSet, Value = "mage_in_party" }
        );

        // ========== ACTIONS ==========
        modelBuilder.Entity<ConversationAction>().HasData(
            // Guard response 3 (thanks for tip): set talked_to_guard flag
            new ConversationAction { Id = 1, ResponseId = 3,
                ActionType = ConversationActionType.SetFlag, Value = "talked_to_guard" },
            // Guard response 4 (can handle myself): also set talked_to_guard
            new ConversationAction { Id = 2, ResponseId = 4,
                ActionType = ConversationActionType.SetFlag, Value = "talked_to_guard" },

            // Merchant response 16 (buy Dunedain Amulet): take gold + give item + set flag
            new ConversationAction { Id = 3, ResponseId = 16,
                ActionType = ConversationActionType.TakeGold, Value = "300" },
            new ConversationAction { Id = 4, ResponseId = 16,
                ActionType = ConversationActionType.GiveItem, Value = "Dunedain Amulet" },
            new ConversationAction { Id = 5, ResponseId = 16,
                ActionType = ConversationActionType.SetFlag, Value = "bought_amulet" },

            // Merchant response 18 (accept discount): set flag
            new ConversationAction { Id = 6, ResponseId = 18,
                ActionType = ConversationActionType.SetFlag, Value = "merchant_discount" },
            // Merchant response 20 (arcane access): set flag
            new ConversationAction { Id = 7, ResponseId = 20,
                ActionType = ConversationActionType.SetFlag, Value = "merchant_arcane_access" },

            // Mage response 25 (what happened): set talked_to_mage
            new ConversationAction { Id = 8, ResponseId = 25,
                ActionType = ConversationActionType.SetFlag, Value = "talked_to_mage" },
            // Mage response 26 (can't help): also set talked_to_mage
            new ConversationAction { Id = 9, ResponseId = 26,
                ActionType = ConversationActionType.SetFlag, Value = "talked_to_mage" },

            // Mage response 27 (I'll get the Key): start quest
            new ConversationAction { Id = 10, ResponseId = 27,
                ActionType = ConversationActionType.StartQuest, Value = "retrieve_key" },

            // Mage response 30 (give Health Potion): heal mage, take potion
            new ConversationAction { Id = 11, ResponseId = 30,
                ActionType = ConversationActionType.SetFlag, Value = "mage_healed" },
            new ConversationAction { Id = 18, ResponseId = 30,
                ActionType = ConversationActionType.TakeItem, Value = "Health Potion" },

            // Mage response 40 (glad I could help): complete quest, give gold reward
            new ConversationAction { Id = 12, ResponseId = 40,
                ActionType = ConversationActionType.CompleteQuest, Value = "retrieve_key" },
            new ConversationAction { Id = 13, ResponseId = 40,
                ActionType = ConversationActionType.GiveGold, Value = "200" },
            new ConversationAction { Id = 14, ResponseId = 40,
                ActionType = ConversationActionType.GiveExperience, Value = "50" },
            // Mage response 41 (tough fight): same rewards
            new ConversationAction { Id = 15, ResponseId = 41,
                ActionType = ConversationActionType.CompleteQuest, Value = "retrieve_key" },
            new ConversationAction { Id = 16, ResponseId = 41,
                ActionType = ConversationActionType.GiveGold, Value = "200" },
            new ConversationAction { Id = 17, ResponseId = 41,
                ActionType = ConversationActionType.GiveExperience, Value = "50" },

            // Mage response 47 (welcome your company): recruit follower + set flag
            new ConversationAction { Id = 19, ResponseId = 47,
                ActionType = ConversationActionType.RecruitFollower, Value = "Mage" },
            new ConversationAction { Id = 20, ResponseId = 47,
                ActionType = ConversationActionType.SetFlag, Value = "mage_in_party" }
        );
    }

    private static void SeedItemTypes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ItemType>().HasData(
            new ItemType { Id = 1, Name = "Weapon", Description = "Weapons for combat.", BaseWeight = 5.0m, BaseTradeValue = 50m },
            new ItemType { Id = 2, Name = "Armor", Description = "Protective gear.", BaseWeight = 8.0m, BaseTradeValue = 60m },
            new ItemType { Id = 3, Name = "Consumable", Description = "Single-use items.", BaseWeight = 0.5m, BaseTradeValue = 20m },
            new ItemType { Id = 4, Name = "Accessory", Description = "Wearable accessories.", BaseWeight = 1.0m, BaseTradeValue = 100m },
            new ItemType { Id = 5, Name = "Material", Description = "Crafting materials.", BaseWeight = 2.0m, BaseTradeValue = 10m },
            new ItemType { Id = 6, Name = "Quest", Description = "Quest-related items.", BaseWeight = 0.1m, BaseTradeValue = 0m }
        );
    }

    private static void SeedItems(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>().HasData(
            // Existing items (updated with equipment properties)
            new { Id = 1, Name = "Health Potion", Description = "Restores a moderate amount of health.", TradeValue = 25m, Durability = 1m, ItemTypeId = 3, Slot = EquipmentSlot.None, AttackRating = 0m, ArmorRating = 0m, LuckBonus = 0, StrengthBonus = 0, DexterityBonus = 0, ConstitutionBonus = 0 },
            new { Id = 2, Name = "Mana Potion", Description = "Restores a moderate amount of mana.", TradeValue = 30m, Durability = 1m, ItemTypeId = 3, Slot = EquipmentSlot.None, AttackRating = 0m, ArmorRating = 0m, LuckBonus = 0, StrengthBonus = 0, DexterityBonus = 0, ConstitutionBonus = 0 },
            new { Id = 3, Name = "Short Sword", Description = "A simple but reliable blade.", TradeValue = 60m, Durability = 100m, ItemTypeId = 1, Slot = EquipmentSlot.Weapon, AttackRating = 8m, ArmorRating = 0m, LuckBonus = 0, StrengthBonus = 0, DexterityBonus = 0, ConstitutionBonus = 0 },
            new { Id = 4, Name = "Leather Armor", Description = "Light armor offering basic protection.", TradeValue = 80m, Durability = 100m, ItemTypeId = 2, Slot = EquipmentSlot.Cuirass, AttackRating = 0m, ArmorRating = 6m, LuckBonus = 0, StrengthBonus = 0, DexterityBonus = 1, ConstitutionBonus = 0 },
            new { Id = 5, Name = "Enchanted Amulet", Description = "A glowing amulet pulsing with arcane energy.", TradeValue = 200m, Durability = 999m, ItemTypeId = 4, Slot = EquipmentSlot.Accessory, AttackRating = 0m, ArmorRating = 0m, LuckBonus = 3, StrengthBonus = 0, DexterityBonus = 0, ConstitutionBonus = 0 },
            new { Id = 6, Name = "Iron Ore", Description = "Raw iron ore for smelting.", TradeValue = 12m, Durability = 999m, ItemTypeId = 5, Slot = EquipmentSlot.None, AttackRating = 0m, ArmorRating = 0m, LuckBonus = 0, StrengthBonus = 0, DexterityBonus = 0, ConstitutionBonus = 0 },
            new { Id = 7, Name = "Antidote", Description = "Cures common poisons.", TradeValue = 20m, Durability = 1m, ItemTypeId = 3, Slot = EquipmentSlot.None, AttackRating = 0m, ArmorRating = 0m, LuckBonus = 0, StrengthBonus = 0, DexterityBonus = 0, ConstitutionBonus = 0 },
            new { Id = 8, Name = "Wooden Shield", Description = "A sturdy wooden shield.", TradeValue = 45m, Durability = 80m, ItemTypeId = 2, Slot = EquipmentSlot.Accessory, AttackRating = 0m, ArmorRating = 3m, LuckBonus = 0, StrengthBonus = 0, DexterityBonus = 0, ConstitutionBonus = 0 },
            // New equipment items
            new { Id = 9, Name = "Dagger", Description = "A small, quick blade favored by rogues.", TradeValue = 30m, Durability = 80m, ItemTypeId = 1, Slot = EquipmentSlot.Weapon, AttackRating = 5m, ArmorRating = 0m, LuckBonus = 0, StrengthBonus = 0, DexterityBonus = 1, ConstitutionBonus = 0 },
            new { Id = 10, Name = "Greatsword", Description = "A massive two-handed blade of tremendous power.", TradeValue = 150m, Durability = 120m, ItemTypeId = 1, Slot = EquipmentSlot.Weapon, AttackRating = 15m, ArmorRating = 0m, LuckBonus = 0, StrengthBonus = 2, DexterityBonus = 0, ConstitutionBonus = 0 },
            new { Id = 11, Name = "Mace", Description = "A heavy blunt weapon effective against armor.", TradeValue = 75m, Durability = 110m, ItemTypeId = 1, Slot = EquipmentSlot.Weapon, AttackRating = 10m, ArmorRating = 0m, LuckBonus = 0, StrengthBonus = 1, DexterityBonus = 0, ConstitutionBonus = 0 },
            new { Id = 12, Name = "Leather Helmet", Description = "A simple leather cap for head protection.", TradeValue = 35m, Durability = 80m, ItemTypeId = 2, Slot = EquipmentSlot.Helmet, AttackRating = 0m, ArmorRating = 2m, LuckBonus = 0, StrengthBonus = 0, DexterityBonus = 0, ConstitutionBonus = 0 },
            new { Id = 13, Name = "Leather Gauntlets", Description = "Supple leather gloves that maintain dexterity.", TradeValue = 25m, Durability = 70m, ItemTypeId = 2, Slot = EquipmentSlot.Gauntlets, AttackRating = 0m, ArmorRating = 1m, LuckBonus = 0, StrengthBonus = 0, DexterityBonus = 1, ConstitutionBonus = 0 },
            new { Id = 14, Name = "Leather Greaves", Description = "Leather leg guards for basic protection.", TradeValue = 30m, Durability = 80m, ItemTypeId = 2, Slot = EquipmentSlot.Greaves, AttackRating = 0m, ArmorRating = 2m, LuckBonus = 0, StrengthBonus = 0, DexterityBonus = 0, ConstitutionBonus = 0 },
            new { Id = 15, Name = "Leather Boots", Description = "Comfortable leather boots for long journeys.", TradeValue = 25m, Durability = 70m, ItemTypeId = 2, Slot = EquipmentSlot.Boots, AttackRating = 0m, ArmorRating = 1m, LuckBonus = 0, StrengthBonus = 0, DexterityBonus = 1, ConstitutionBonus = 0 },
            new { Id = 16, Name = "Iron Helmet", Description = "A sturdy iron helm providing solid protection.", TradeValue = 70m, Durability = 120m, ItemTypeId = 2, Slot = EquipmentSlot.Helmet, AttackRating = 0m, ArmorRating = 4m, LuckBonus = 0, StrengthBonus = 0, DexterityBonus = 0, ConstitutionBonus = 1 },
            new { Id = 17, Name = "Iron Cuirass", Description = "Heavy iron chest armor for maximum protection.", TradeValue = 160m, Durability = 150m, ItemTypeId = 2, Slot = EquipmentSlot.Cuirass, AttackRating = 0m, ArmorRating = 10m, LuckBonus = 0, StrengthBonus = 0, DexterityBonus = 0, ConstitutionBonus = 2 },
            new { Id = 18, Name = "Iron Gauntlets", Description = "Heavy iron gauntlets that add striking power.", TradeValue = 55m, Durability = 110m, ItemTypeId = 2, Slot = EquipmentSlot.Gauntlets, AttackRating = 0m, ArmorRating = 3m, LuckBonus = 0, StrengthBonus = 1, DexterityBonus = 0, ConstitutionBonus = 0 },
            new { Id = 19, Name = "Iron Greaves", Description = "Solid iron leg guards for heavy duty.", TradeValue = 65m, Durability = 120m, ItemTypeId = 2, Slot = EquipmentSlot.Greaves, AttackRating = 0m, ArmorRating = 4m, LuckBonus = 0, StrengthBonus = 0, DexterityBonus = 0, ConstitutionBonus = 1 },
            new { Id = 20, Name = "Iron Boots", Description = "Heavy iron boots offering solid foot protection.", TradeValue = 55m, Durability = 110m, ItemTypeId = 2, Slot = EquipmentSlot.Boots, AttackRating = 0m, ArmorRating = 3m, LuckBonus = 0, StrengthBonus = 0, DexterityBonus = 0, ConstitutionBonus = 0 },
            // Scenario items
            new { Id = 21, Name = "Dunedain Amulet", Description = "An ancient amulet of the Dunedain, radiating powerful fortune.", TradeValue = 350m, Durability = 999m, ItemTypeId = 4, Slot = EquipmentSlot.Accessory, AttackRating = 0m, ArmorRating = 0m, LuckBonus = 5, StrengthBonus = 0, DexterityBonus = 0, ConstitutionBonus = 0 },
            new { Id = 22, Name = "Key of Stratholme", Description = "An ornate key belonging to the Mage of Stratholme.", TradeValue = 0m, Durability = 999m, ItemTypeId = 6, Slot = EquipmentSlot.None, AttackRating = 0m, ArmorRating = 0m, LuckBonus = 0, StrengthBonus = 0, DexterityBonus = 0, ConstitutionBonus = 0 }
        );
    }

    private static void SeedMerchantStock(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MerchantStock>().HasData(
            new MerchantStock { Id = 1, NpcName = "Merchant", ItemId = 1, Quantity = 10, PriceMultiplier = 1.0m },
            new MerchantStock { Id = 2, NpcName = "Merchant", ItemId = 2, Quantity = 5, PriceMultiplier = 1.0m },
            new MerchantStock { Id = 3, NpcName = "Merchant", ItemId = 3, Quantity = 2, PriceMultiplier = 1.1m },
            new MerchantStock { Id = 4, NpcName = "Merchant", ItemId = 4, Quantity = 2, PriceMultiplier = 1.0m },
            new MerchantStock { Id = 5, NpcName = "Merchant", ItemId = 5, Quantity = 1, PriceMultiplier = 1.0m },
            new MerchantStock { Id = 6, NpcName = "Merchant", ItemId = 7, Quantity = 5, PriceMultiplier = 0.9m },
            new MerchantStock { Id = 7, NpcName = "Merchant", ItemId = 8, Quantity = 3, PriceMultiplier = 1.0m },
            // New equipment stock
            new MerchantStock { Id = 8, NpcName = "Merchant", ItemId = 9, Quantity = 3, PriceMultiplier = 1.0m },   // Dagger
            new MerchantStock { Id = 9, NpcName = "Merchant", ItemId = 10, Quantity = 1, PriceMultiplier = 1.2m },  // Greatsword
            new MerchantStock { Id = 10, NpcName = "Merchant", ItemId = 11, Quantity = 2, PriceMultiplier = 1.0m }, // Mace
            new MerchantStock { Id = 11, NpcName = "Merchant", ItemId = 12, Quantity = 2, PriceMultiplier = 1.0m }, // Leather Helmet
            new MerchantStock { Id = 12, NpcName = "Merchant", ItemId = 13, Quantity = 2, PriceMultiplier = 1.0m }, // Leather Gauntlets
            new MerchantStock { Id = 13, NpcName = "Merchant", ItemId = 14, Quantity = 2, PriceMultiplier = 1.0m }, // Leather Greaves
            new MerchantStock { Id = 14, NpcName = "Merchant", ItemId = 15, Quantity = 2, PriceMultiplier = 1.0m }, // Leather Boots
            new MerchantStock { Id = 15, NpcName = "Merchant", ItemId = 16, Quantity = 1, PriceMultiplier = 1.1m }, // Iron Helmet
            new MerchantStock { Id = 16, NpcName = "Merchant", ItemId = 17, Quantity = 1, PriceMultiplier = 1.1m }, // Iron Cuirass
            new MerchantStock { Id = 17, NpcName = "Merchant", ItemId = 18, Quantity = 1, PriceMultiplier = 1.1m }, // Iron Gauntlets
            new MerchantStock { Id = 18, NpcName = "Merchant", ItemId = 19, Quantity = 1, PriceMultiplier = 1.1m }, // Iron Greaves
            new MerchantStock { Id = 19, NpcName = "Merchant", ItemId = 20, Quantity = 1, PriceMultiplier = 1.1m },  // Iron Boots
            new MerchantStock { Id = 20, NpcName = "Merchant", ItemId = 21, Quantity = 1, PriceMultiplier = 1.0m }   // Dunedain Amulet
        );
    }

    private static void SeedInspectDescriptions(ModelBuilder modelBuilder)
    {
        // ========== INSPECT DESCRIPTIONS ==========
        modelBuilder.Entity<InspectDescription>().HasData(
            // --- Tiles ---
            new InspectDescription { Id = 1, TargetType = "Tile", TargetKey = "1", Priority = 0,
                Text = "A carpet of wild grass stretches before you, bending gently in the breeze. Small wildflowers dot the green expanse — white clover, yellow buttercup, the occasional purple thistle. The earth beneath feels soft and yielding, well-watered by the rains that sweep through this valley." },
            new InspectDescription { Id = 2, TargetType = "Tile", TargetKey = "2", Priority = 0,
                Text = "Dark water stretches before you, its surface broken by slow, lazy ripples. The depth is impossible to gauge — it could be ankle-deep or bottomless. Reeds cluster along the edges, and you catch the silver flash of small fish darting beneath the surface." },
            new InspectDescription { Id = 3, TargetType = "Tile", TargetKey = "3", Priority = 0,
                Text = "Fine sand shifts beneath your feet, golden-brown and warm. The grains are surprisingly uniform — this was once a riverbed, perhaps, or the shore of some long-vanished lake. Walking here is slow going, each step sinking slightly before finding purchase." },
            new InspectDescription { Id = 4, TargetType = "Tile", TargetKey = "4", Priority = 0,
                Text = "Bare stone lies exposed here, worn smooth by centuries of wind and rain. Thin cracks spider across the surface, and in places you can see where ancient geological forces thrust this bedrock upward. Your boots ring hollow against the solid ground." },
            new InspectDescription { Id = 5, TargetType = "Tile", TargetKey = "5", Priority = 0,
                Text = "Hard-packed earth stretches underfoot, cracked and dry. Boot prints and cart ruts score the surface — this ground has seen heavy use. A thin layer of dust rises with each step, carrying the dry mineral smell of well-trodden soil." },

            // --- Objects ---
            new InspectDescription { Id = 10, TargetType = "Object", TargetKey = "1", Priority = 0,
                Text = "A gnarled tree rises from the earth, its trunk thick with age and its branches spread wide overhead. Rough bark is patterned with lichens and moss, and the roots grip the soil like an old man's fingers. Birds have nested in the upper branches — you can hear their faint calls." },
            new InspectDescription { Id = 11, TargetType = "Object", TargetKey = "2", Priority = 0,
                Text = "A massive boulder squats on the ground, half-buried in the earth. Its surface is weathered and pitted, streaked with veins of darker mineral. It would take a team of oxen to move this stone — or perhaps it was placed here deliberately, long ago, for reasons lost to time." },
            new InspectDescription { Id = 12, TargetType = "Object", TargetKey = "3", Priority = 0,
                Text = "A wall of fitted stone blocks your path, each block carefully cut and mortared into place. The craftsmanship speaks of an older age — these stones have stood for generations. In places the mortar has crumbled, but the wall remains solid and unyielding." },

            // --- Guard ---
            new InspectDescription { Id = 20, TargetType = "Npc", TargetKey = "Guard", Priority = 0,
                Text = "A tall figure in worn but well-maintained chainmail stands at attention, one hand resting on the pommel of a sheathed longsword. His face is weathered by years of outdoor duty, eyes constantly scanning the horizon. A faded tabard marks him as part of the local garrison — one of the few who still take their post seriously." },
            new InspectDescription { Id = 21, TargetType = "Npc", TargetKey = "Guard", Priority = 5,
                Text = "The guard nods in recognition as you approach, his posture relaxing slightly. He still keeps one hand near his weapon — old habits — but his expression is warmer now. \"Back again?\" his stance seems to say. You've earned a measure of trust from this watchful soldier." },
            new InspectDescription { Id = 22, TargetType = "Npc", TargetKey = "Guard", Priority = 10,
                Text = "The guard straightens when he sees you, and you catch something new in his expression — respect. Word of your deeds has reached him. He inclines his head slightly, a gesture that says more than words. In the quiet hierarchy of soldiers, you have been recognized." },

            // --- Merchant ---
            new InspectDescription { Id = 25, TargetType = "Npc", TargetKey = "Merchant", Priority = 0,
                Text = "A shrewd-eyed trader stands behind an array of carefully displayed wares, fingers absently counting coins in a leather pouch. His clothing is finer than you'd expect for this outpost — silk-lined cuffs, polished brass buttons. He watches every passerby with the calculating gaze of someone who knows the price of everything." },
            new InspectDescription { Id = 26, TargetType = "Npc", TargetKey = "Merchant", Priority = 5,
                Text = "The merchant's face brightens as he recognizes a paying customer. His manner is warmer now, more solicitous — you've proven yourself someone worth dealing with. He subtly angles his best merchandise in your direction, a small smile playing at the corners of his mouth." },
            new InspectDescription { Id = 27, TargetType = "Npc", TargetKey = "Merchant", Priority = 5,
                Text = "Something has shifted in the merchant's bearing. He catches your eye and gives an almost imperceptible nod, as if acknowledging a shared secret. Behind his usual wares, you notice a locked cabinet you hadn't seen before — its wood dark with age, its brass fittings inscribed with faint symbols." },

            // --- Mage ---
            new InspectDescription { Id = 30, TargetType = "Npc", TargetKey = "Mage", Priority = 0,
                Text = "A robed figure leans heavily against a gnarled staff, face drawn with pain. His traveling cloak is torn and bloodstained, and you can see where crude bandages have been hastily wrapped around his midsection. Despite his injuries, his eyes burn with an unsettling intelligence — this is no ordinary wanderer." },
            new InspectDescription { Id = 31, TargetType = "Npc", TargetKey = "Mage", Priority = 5,
                Text = "The mage looks up as you approach, recognition flickering across his pain-creased features. He seems relieved to see a familiar face in this dangerous place. His wounds are still grievous, but there is hope in his eyes now — hope that was not there before you spoke with him." },
            new InspectDescription { Id = 32, TargetType = "Npc", TargetKey = "Mage", Priority = 10,
                Text = "Color has returned to the mage's cheeks, and he stands a little straighter than before. The healing potion has done its work — the worst of his wounds have closed, though he still moves gingerly. His eyes are clearer now, sharp with renewed purpose. He clutches his staff with growing strength." },
            new InspectDescription { Id = 33, TargetType = "Npc", TargetKey = "Mage", Priority = 15,
                Text = "The mage stands tall and whole, his injuries fully healed and his power restored. Arcane energy crackles faintly along his staff, and his robes — once torn and bloodied — have been mended with visible skill. He radiates quiet authority, and when he speaks, the air itself seems to listen." },

            // --- Mercenary (alive) ---
            new InspectDescription { Id = 40, TargetType = "Npc", TargetKey = "Mercenary", Priority = 0,
                Text = "A broad-shouldered warrior watches your approach with cold, calculating eyes. Iron plates are riveted over scarred leather, and a heavy mace hangs at his belt with the ease of long familiarity. Everything about his bearing suggests violence held in check — barely. This is someone who kills for coin and feels nothing about it." },

            // --- Mercenary (dead) ---
            new InspectDescription { Id = 41, TargetType = "Npc", TargetKey = "Mercenary (Dead)", Priority = 0,
                Text = "The mercenary lies where he fell, his heavy armor doing nothing to cushion his final collapse. His eyes stare at nothing, and the mace has rolled from his slack fingers. Among his belongings, something glints — an ornate key, far too fine for a common sellsword to possess." },

            // --- Bandit (alive) ---
            new InspectDescription { Id = 45, TargetType = "Npc", TargetKey = "Bandit", Priority = 0,
                Text = "A lean, wiry figure lurks at the edge of the path, hand never far from a notched blade. His clothes are a patchwork of stolen garments, and a crude mask of dark cloth hides the lower half of his face. He watches you with the hungry patience of a predator sizing up its prey." },

            // --- Bandit (dead) ---
            new InspectDescription { Id = 46, TargetType = "Npc", TargetKey = "Bandit (Dead)", Priority = 0,
                Text = "The bandit lies crumpled in the dirt, his stolen finery splayed around him like the plumage of a fallen bird. The mask has slipped, revealing a surprisingly young face beneath — barely old enough to grow a beard. Whatever drove him to this life, it ended here." },

            // --- Mage (party member) ---
            new InspectDescription { Id = 50, TargetType = "Npc", TargetKey = "Mage", Priority = 20,
                Text = "The mage walks at your side, his staff tapping a steady rhythm against the earth. His injuries are long healed, and arcane energy crackles faintly about his fingers as he surveys the road ahead. There is a quiet confidence in his bearing now — the confidence of a man who has found purpose again. He is your ally, and the bond forged through hardship runs deep." }
        );

        // ========== INSPECT CONDITIONS ==========
        modelBuilder.Entity<InspectCondition>().HasData(
            // Guard - after talking (Id 21): requires talked_to_guard
            new InspectCondition { Id = 1, InspectDescriptionId = 21,
                ConditionType = ConditionType.FlagSet, Value = "talked_to_guard" },
            // Guard - quest complete (Id 22): requires quest_done_retrieve_key
            new InspectCondition { Id = 2, InspectDescriptionId = 22,
                ConditionType = ConditionType.FlagSet, Value = "quest_done_retrieve_key" },

            // Merchant - after buying amulet (Id 26): requires bought_amulet
            new InspectCondition { Id = 3, InspectDescriptionId = 26,
                ConditionType = ConditionType.FlagSet, Value = "bought_amulet" },
            // Merchant - arcane access (Id 27): requires merchant_arcane_access
            new InspectCondition { Id = 4, InspectDescriptionId = 27,
                ConditionType = ConditionType.FlagSet, Value = "merchant_arcane_access" },

            // Mage - after talking (Id 31): requires talked_to_mage
            new InspectCondition { Id = 5, InspectDescriptionId = 31,
                ConditionType = ConditionType.FlagSet, Value = "talked_to_mage" },
            // Mage - after healing (Id 32): requires mage_healed
            new InspectCondition { Id = 6, InspectDescriptionId = 32,
                ConditionType = ConditionType.FlagSet, Value = "mage_healed" },
            // Mage - quest complete (Id 33): requires quest_done_retrieve_key
            new InspectCondition { Id = 7, InspectDescriptionId = 33,
                ConditionType = ConditionType.FlagSet, Value = "quest_done_retrieve_key" },
            // Mage - party member (Id 50): requires mage_in_party
            new InspectCondition { Id = 8, InspectDescriptionId = 50,
                ConditionType = ConditionType.FlagSet, Value = "mage_in_party" }
        );
    }
}
