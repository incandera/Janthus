using Microsoft.EntityFrameworkCore;
using Janthus.Model.Entities;
using Janthus.Data.Seed;

namespace Janthus.Data;

public class JanthusDbContext : DbContext
{
    public DbSet<ActorType> ActorTypes { get; set; }
    public DbSet<CharacterClass> CharacterClasses { get; set; }
    public DbSet<ActorLevel> ActorLevels { get; set; }
    public DbSet<SkillType> SkillTypes { get; set; }
    public DbSet<SkillLevel> SkillLevels { get; set; }
    public DbSet<TileDefinition> TileDefinitions { get; set; }
    public DbSet<WorldMap> WorldMaps { get; set; }
    public DbSet<MapChunk> MapChunks { get; set; }
    public DbSet<ObjectDefinition> ObjectDefinitions { get; set; }
    public DbSet<MapObject> MapObjects { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationNode> ConversationNodes { get; set; }
    public DbSet<ConversationResponse> ConversationResponses { get; set; }
    public DbSet<ConversationCondition> ConversationConditions { get; set; }
    public DbSet<ConversationAction> ConversationActions { get; set; }
    public DbSet<GameFlag> GameFlags { get; set; }
    public DbSet<ItemType> ItemTypes { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<Quality> Qualities { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<MerchantStock> MerchantStocks { get; set; }
    public DbSet<InspectDescription> InspectDescriptions { get; set; }
    public DbSet<InspectCondition> InspectConditions { get; set; }
    public DbSet<QuestDefinition> QuestDefinitions { get; set; }
    public DbSet<QuestGoal> QuestGoals { get; set; }
    public DbSet<Operation> Operations { get; set; }

    public JanthusDbContext(DbContextOptions<JanthusDbContext> options) : base(options) { }

    public JanthusDbContext() { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=janthus_data.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActorType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Ignore(e => e.InternalId);
        });

        modelBuilder.Entity<CharacterClass>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Ignore(e => e.InternalId);
        });

        modelBuilder.Entity<ActorLevel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LevelRankGroupName).HasMaxLength(100);
            entity.Ignore(e => e.ConferredEffectList);
        });

        modelBuilder.Entity<SkillType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Ignore(e => e.InternalId);
        });

        modelBuilder.Entity<SkillLevel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Ignore(e => e.InternalId);
        });

        modelBuilder.Entity<TileDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Ignore(e => e.InternalId);
        });

        modelBuilder.Entity<ObjectDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Ignore(e => e.InternalId);
        });

        modelBuilder.Entity<WorldMap>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<MapChunk>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.WorldMapId, e.ChunkX, e.ChunkY }).IsUnique();
        });

        modelBuilder.Entity<MapObject>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MapChunkId);
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NpcName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.HasIndex(e => e.NpcName);
            entity.Ignore(e => e.Conditions);
        });

        modelBuilder.Entity<ConversationNode>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SpeakerName).HasMaxLength(100);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(1000);
            entity.HasIndex(e => e.ConversationId);
            entity.Ignore(e => e.Responses);
        });

        modelBuilder.Entity<ConversationResponse>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.NodeId);
            entity.Ignore(e => e.Conditions);
            entity.Ignore(e => e.Actions);
        });

        modelBuilder.Entity<ConversationCondition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Value).HasMaxLength(200);
            entity.HasIndex(e => e.ConversationId);
            entity.HasIndex(e => e.ResponseId);
        });

        modelBuilder.Entity<ConversationAction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Value).HasMaxLength(200);
            entity.HasIndex(e => e.ResponseId);
        });

        modelBuilder.Entity<GameFlag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Value).HasMaxLength(500);
        });

        modelBuilder.Entity<ItemType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Ignore(e => e.InternalId);
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Ignore(e => e.InternalId);
            entity.Ignore(e => e.Type);
            entity.Ignore(e => e.Quality);
            entity.Ignore(e => e.Material);
            entity.Ignore(e => e.EffectList);
            entity.Ignore(e => e.CraftComponents);
            entity.Property<int>("ItemTypeId");
        });

        modelBuilder.Entity<Quality>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Ignore(e => e.InternalId);
            entity.Ignore(e => e.AttributeMultipliers);
        });

        modelBuilder.Entity<Material>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Ignore(e => e.InternalId);
        });

        modelBuilder.Entity<MerchantStock>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NpcName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.NpcName);
        });

        modelBuilder.Entity<InspectDescription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TargetType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TargetKey).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(2000);
            entity.HasIndex(e => new { e.TargetType, e.TargetKey });
            entity.Ignore(e => e.Conditions);
        });

        modelBuilder.Entity<InspectCondition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Value).HasMaxLength(200);
            entity.HasIndex(e => e.InspectDescriptionId);
        });

        modelBuilder.Entity<QuestDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ActivationFlag).HasMaxLength(100);
            entity.Property(e => e.CompletionFlag).HasMaxLength(100);
            entity.Property(e => e.FailureFlag).HasMaxLength(100);
            entity.Ignore(e => e.Goals);
        });

        modelBuilder.Entity<QuestGoal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.CompletionFlag).HasMaxLength(100);
            entity.HasIndex(e => e.QuestDefinitionId);
        });

        modelBuilder.Entity<Operation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Ignore(e => e.InternalId);
        });

        SeedData.Apply(modelBuilder);
    }
}
