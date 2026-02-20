using Xunit;
using Janthus.Model.Entities;
using Janthus.Model.Services;

namespace Janthus.Model.Tests;

public class CharacterCalculatorTests
{
    [Fact]
    public void CalculateHitPoints_ReturnsCorrectValue()
    {
        var constitution = new CharacterAttribute { Value = 10 };
        var strength = new CharacterAttribute { Value = 8 };
        var willpower = new CharacterAttribute { Value = 6 };

        // (10 * 0.5 + 8 * 0.25 + 6 * 0.25) * 10 = (5 + 2 + 1.5) * 10 = 85
        var hp = CharacterCalculator.CalculateHitPoints(constitution, strength, willpower);

        Assert.Equal(85.0, hp);
    }

    [Fact]
    public void CalculateHitPoints_WithZeroAttributes_ReturnsZero()
    {
        var constitution = new CharacterAttribute { Value = 0 };
        var strength = new CharacterAttribute { Value = 0 };
        var willpower = new CharacterAttribute { Value = 0 };

        var hp = CharacterCalculator.CalculateHitPoints(constitution, strength, willpower);

        Assert.Equal(0.0, hp);
    }

    [Fact]
    public void CalculateMana_ReturnsCorrectValue()
    {
        var attunement = new CharacterAttribute { Value = 12 };
        var intelligence = new CharacterAttribute { Value = 10 };
        var willpower = new CharacterAttribute { Value = 8 };

        // (12 * 0.5 + 10 * 0.25 + 8 * 0.25) * 10 = (6 + 2.5 + 2) * 10 = 105
        var mana = CharacterCalculator.CalculateMana(attunement, intelligence, willpower);

        Assert.Equal(105.0, mana);
    }

    [Fact]
    public void CalculateMana_WithEqualAttributes_ReturnsCorrectValue()
    {
        var attunement = new CharacterAttribute { Value = 5 };
        var intelligence = new CharacterAttribute { Value = 5 };
        var willpower = new CharacterAttribute { Value = 5 };

        // (5 * 0.5 + 5 * 0.25 + 5 * 0.25) * 10 = (2.5 + 1.25 + 1.25) * 10 = 50
        var mana = CharacterCalculator.CalculateMana(attunement, intelligence, willpower);

        Assert.Equal(50.0, mana);
    }
}
