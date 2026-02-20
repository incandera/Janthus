using Xunit;
using Microsoft.EntityFrameworkCore;
using Janthus.Model.Entities;
using Janthus.Model.Enums;
using Janthus.Data;

namespace Janthus.Model.Tests;

public class LeveledActorTests : IDisposable
{
    private readonly JanthusDbContext _context;
    private readonly GameDataRepository _repository;

    public LeveledActorTests()
    {
        var options = new DbContextOptionsBuilder<JanthusDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        _context = new JanthusDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
        _repository = new GameDataRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    [Fact]
    public void ExplicitConstructor_SetsAttributesCorrectly()
    {
        var npc = new NonPlayerCharacter(3, 2, 2, 4, 2, 3, 3,
            new Alignment(LawfulnessType.Lawful, DispositionType.Good));

        Assert.NotNull(npc);
        Assert.Equal(3, npc.Constitution.Value);
        Assert.Equal(2, npc.Dexterity.Value);
        Assert.Equal(2, npc.Intelligence.Value);
        Assert.Equal(4, npc.Luck.Value);
        Assert.Equal(2, npc.Attunement.Value);
        Assert.Equal(3, npc.Strength.Value);
        Assert.Equal(3, npc.Willpower.Value);
        Assert.Equal(19, npc.SumOfAttributes);
    }

    [Fact]
    public void ExplicitConstructor_SetsAlignmentCorrectly()
    {
        var npc = new NonPlayerCharacter(3, 2, 2, 4, 2, 3, 3,
            new Alignment(LawfulnessType.Lawful, DispositionType.Good));

        Assert.Equal(LawfulnessType.Lawful, npc.Alignment.Lawfulness);
        Assert.Equal(DispositionType.Good, npc.Alignment.Disposition);
    }

    [Fact]
    public void RandomRoll_CreatesNpcWithCorrectAttributeSum()
    {
        var randomNpc = new NonPlayerCharacter(_repository, "Mage", 6,
            new Alignment(LawfulnessType.Neutral, DispositionType.Evil));

        var currentLevel = _repository.GetLevel(6);
        var nextLevel = _repository.GetLevel(7);

        Assert.NotNull(randomNpc);
        Assert.True(randomNpc.SumOfAttributes >= currentLevel.MinimumSumOfAttributes);
        Assert.True(randomNpc.SumOfAttributes < nextLevel.MinimumSumOfAttributes);
    }

    [Fact]
    public void RandomRoll_SetsAlignmentCorrectly()
    {
        var randomNpc = new NonPlayerCharacter(_repository, "Soldier", 3,
            new Alignment(LawfulnessType.Chaotic, DispositionType.Neutral));

        Assert.Equal(LawfulnessType.Chaotic, randomNpc.Alignment.Lawfulness);
        Assert.Equal(DispositionType.Neutral, randomNpc.Alignment.Disposition);
    }

    [Fact]
    public void RandomRoll_Level1_HasMinimumAttributes()
    {
        var npc = new NonPlayerCharacter(_repository, "Rogue", 1,
            new Alignment(LawfulnessType.Neutral, DispositionType.Neutral));

        var level1 = _repository.GetLevel(1);
        var level2 = _repository.GetLevel(2);

        Assert.True(npc.SumOfAttributes >= level1.MinimumSumOfAttributes);
        Assert.True(npc.SumOfAttributes < level2.MinimumSumOfAttributes);
    }

    [Fact]
    public void MaximumHitPoints_CalculatesFromAttributes()
    {
        var actor = new LeveledActor(10, 5, 5, 5, 5, 8, 6);

        // (10 * 0.5 + 8 * 0.25 + 6 * 0.25) * 10 = (5 + 2 + 1.5) * 10 = 85
        Assert.Equal(85.0, actor.MaximumHitPoints);
    }

    [Fact]
    public void MaximumMana_CalculatesFromAttributes()
    {
        var actor = new LeveledActor(5, 5, 10, 5, 12, 5, 8);

        // (12 * 0.5 + 10 * 0.25 + 8 * 0.25) * 10 = (6 + 2.5 + 2) * 10 = 105
        Assert.Equal(105.0, actor.MaximumMana);
    }

    [Fact]
    public void SumOfAttributes_IsCorrectTotal()
    {
        var actor = new LeveledActor(1, 2, 3, 4, 5, 6, 7);

        Assert.Equal(28, actor.SumOfAttributes);
    }

    [Fact]
    public void CollectionsAreInitialized()
    {
        var pc = new PlayerCharacter();

        Assert.NotNull(pc.AttackList);
        Assert.NotNull(pc.EffectImmunityList);
        Assert.NotNull(pc.EffectVulnerabilityList);
        Assert.NotNull(pc.Skills);
    }
}
