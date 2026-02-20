using Janthus.Model.Entities;
using Janthus.Model.Enums;

namespace Janthus.Model.Services;

public static class AdversaryCalculator
{
    public static bool IsAdversary(Alignment self, Alignment other, int seed)
    {
        var baseAdversary = GetBaseAdversary(self.Disposition, other.Disposition);
        var flipChance = GetFlipChance(self.Lawfulness, other.Lawfulness);

        if (flipChance > 0)
        {
            var roll = DeterministicRoll(seed);
            if (roll < flipChance)
                baseAdversary = !baseAdversary;
        }

        return baseAdversary;
    }

    private static bool GetBaseAdversary(DispositionType self, DispositionType other)
    {
        if (self == DispositionType.Neutral || other == DispositionType.Neutral)
            return false;

        // Good vs Evil or Evil vs Good
        return self != other;
    }

    private static double GetFlipChance(LawfulnessType self, LawfulnessType other)
    {
        if (self == LawfulnessType.Chaotic && other == LawfulnessType.Chaotic)
            return 0.50;
        if (self == LawfulnessType.Chaotic || other == LawfulnessType.Chaotic)
            return 0.25;
        if (self == LawfulnessType.Lawful && other == LawfulnessType.Lawful)
            return 0.25;

        return 0;
    }

    private static double DeterministicRoll(int seed)
    {
        // Simple deterministic hash to [0, 1)
        var hash = seed * 2654435761u;
        return (hash & 0x7FFFFFFF) / (double)0x80000000;
    }
}
