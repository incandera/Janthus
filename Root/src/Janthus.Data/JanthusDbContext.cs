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

        SeedData.Apply(modelBuilder);
    }
}
