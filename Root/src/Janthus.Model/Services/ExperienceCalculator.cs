using Janthus.Model.Entities;

namespace Janthus.Model.Services;

public static class ExperienceCalculator
{
    private static readonly int[] XpThresholds =
    {
        0,      // Level 1
        100,    // Level 2
        250,    // Level 3
        500,    // Level 4
        850,    // Level 5
        1300,   // Level 6
        1900,   // Level 7
        2650,   // Level 8
        3600,   // Level 9
        4800,   // Level 10
        6300,   // Level 11
        8100,   // Level 12
        10300,  // Level 13
        13000,  // Level 14
        16200,  // Level 15
        20000,  // Level 16
        24500,  // Level 17
        30000,  // Level 18
        37000,  // Level 19
        45000,  // Level 20
    };

    public static int GetExperienceForLevel(int level)
    {
        if (level < 1) return 0;
        if (level > 20) return XpThresholds[19];
        return XpThresholds[level - 1];
    }

    public static int CalculateLevelFromExperience(int xp)
    {
        for (int i = XpThresholds.Length - 1; i >= 0; i--)
        {
            if (xp >= XpThresholds[i])
                return i + 1;
        }
        return 1;
    }

    public static int GetCombatExperience(int enemyLevel)
    {
        return 10 + enemyLevel * enemyLevel;
    }

    public static void DistributeAttributePoints(LeveledActor actor, CharacterClass charClass, int points)
    {
        var weights = new (string name, double weight, Func<CharacterAttribute> accessor)[]
        {
            ("Constitution", charClass.ConstitutionRollWeight, () => actor.Constitution),
            ("Dexterity", charClass.DexterityRollWeight, () => actor.Dexterity),
            ("Intelligence", charClass.IntelligenceRollWeight, () => actor.Intelligence),
            ("Luck", charClass.LuckRollWeight, () => actor.Luck),
            ("Attunement", charClass.AttunementRollWeight, () => actor.Attunement),
            ("Strength", charClass.StrengthRollWeight, () => actor.Strength),
            ("Willpower", charClass.WillpowerRollWeight, () => actor.Willpower),
        };

        // Proportional pass: distribute points by weight
        var totalWeight = 0.0;
        foreach (var w in weights)
            totalWeight += w.weight;

        var distributed = 0;
        var fractional = new (int index, double frac)[weights.Length];

        for (int i = 0; i < weights.Length; i++)
        {
            var exact = points * (weights[i].weight / totalWeight);
            var whole = (int)Math.Floor(exact);
            fractional[i] = (i, exact - whole);
            weights[i].accessor().Value += whole;
            distributed += whole;
        }

        // Remainder to highest-weighted attributes
        var remaining = points - distributed;
        Array.Sort(fractional, (a, b) => b.frac.CompareTo(a.frac));
        for (int i = 0; i < remaining && i < fractional.Length; i++)
        {
            weights[fractional[i].index].accessor().Value++;
        }
    }
}
