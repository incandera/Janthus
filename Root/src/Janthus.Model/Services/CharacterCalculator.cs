using Janthus.Model.Entities;

namespace Janthus.Model.Services;

public static class CharacterCalculator
{
    public static double CalculateHitPoints(CharacterAttribute constitution,
                                            CharacterAttribute strength,
                                            CharacterAttribute willpower)
    {
        return ((constitution.Value * 0.5) + (strength.Value * 0.25) + (willpower.Value * 0.25)) * 10;
    }

    public static double CalculateMana(CharacterAttribute attunement,
                                       CharacterAttribute intelligence,
                                       CharacterAttribute willpower)
    {
        return ((attunement.Value * 0.5) + (intelligence.Value * 0.25) + (willpower.Value * 0.25)) * 10;
    }
}
