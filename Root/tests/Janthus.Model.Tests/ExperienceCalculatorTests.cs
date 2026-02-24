using Xunit;
using Janthus.Model.Entities;
using Janthus.Model.Services;

namespace Janthus.Model.Tests;

public class ExperienceCalculatorTests
{
    [Fact]
    public void GetExperienceForLevel_Level1_ReturnsZero()
    {
        Assert.Equal(0, ExperienceCalculator.GetExperienceForLevel(1));
    }

    [Fact]
    public void GetExperienceForLevel_Level20_Returns45000()
    {
        Assert.Equal(45000, ExperienceCalculator.GetExperienceForLevel(20));
    }

    [Fact]
    public void CalculateLevelFromExperience_ZeroXp_ReturnsLevel1()
    {
        Assert.Equal(1, ExperienceCalculator.CalculateLevelFromExperience(0));
    }

    [Fact]
    public void CalculateLevelFromExperience_100Xp_ReturnsLevel2()
    {
        Assert.Equal(2, ExperienceCalculator.CalculateLevelFromExperience(100));
    }

    [Fact]
    public void CalculateLevelFromExperience_99Xp_ReturnsLevel1()
    {
        Assert.Equal(1, ExperienceCalculator.CalculateLevelFromExperience(99));
    }

    [Fact]
    public void CalculateLevelFromExperience_45000Xp_ReturnsLevel20()
    {
        Assert.Equal(20, ExperienceCalculator.CalculateLevelFromExperience(45000));
    }

    [Fact]
    public void CalculateLevelFromExperience_MaxXp_ReturnsLevel20()
    {
        Assert.Equal(20, ExperienceCalculator.CalculateLevelFromExperience(999999));
    }

    [Fact]
    public void GetCombatExperience_Level1_Returns11()
    {
        Assert.Equal(11, ExperienceCalculator.GetCombatExperience(1));
    }

    [Fact]
    public void GetCombatExperience_Level10_Returns110()
    {
        Assert.Equal(110, ExperienceCalculator.GetCombatExperience(10));
    }

    [Fact]
    public void DistributeAttributePoints_SoldierClass_FavorsStrengthAndConstitution()
    {
        var actor = new LeveledActor(1, 1, 1, 1, 1, 1, 1);
        var soldierClass = new CharacterClass
        {
            ConstitutionRollWeight = 0.20,
            DexterityRollWeight = 0.10,
            IntelligenceRollWeight = 0.05,
            LuckRollWeight = 0.10,
            AttunementRollWeight = 0.05,
            StrengthRollWeight = 0.30,
            WillpowerRollWeight = 0.20
        };

        ExperienceCalculator.DistributeAttributePoints(actor, soldierClass, 7);

        // Total should be original 7 + distributed 7 = 14
        Assert.Equal(14, actor.SumOfAttributes);
        // Strength should get the most (0.30 weight)
        Assert.True(actor.Strength.Value >= actor.Dexterity.Value);
        Assert.True(actor.Strength.Value >= actor.Intelligence.Value);
        Assert.True(actor.Strength.Value >= actor.Attunement.Value);
    }
}
