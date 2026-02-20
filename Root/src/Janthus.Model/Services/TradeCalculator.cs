using Janthus.Model.Entities;
using Janthus.Model.Enums;

namespace Janthus.Model.Services;

public static class TradeCalculator
{
    public static decimal CalculateBuyPrice(
        Item item,
        decimal merchantPriceMultiplier,
        Alignment playerAlignment,
        Alignment merchantAlignment,
        List<Skill> playerSkills,
        IGameDataProvider dataProvider)
    {
        var basePrice = item.TradeValue;

        if (item.Quality != null && item.Quality.TradeValueMultiplier != 0)
            basePrice *= item.Quality.TradeValueMultiplier;
        if (item.Material != null && item.Material.TradeValueMultiplier != 0)
            basePrice *= item.Material.TradeValueMultiplier;

        // Merchant markup
        basePrice *= merchantPriceMultiplier;

        // Alignment modifier
        var alignmentMod = 1.0m;
        if (merchantAlignment.Disposition == DispositionType.Good)
            alignmentMod -= 0.05m;
        else if (merchantAlignment.Disposition == DispositionType.Evil)
            alignmentMod += 0.15m;

        if (merchantAlignment.Lawfulness == LawfulnessType.Lawful)
            alignmentMod -= 0.05m;
        else if (merchantAlignment.Lawfulness == LawfulnessType.Chaotic)
            alignmentMod += 0.10m;

        // Same disposition sympathy
        if (playerAlignment.Disposition == merchantAlignment.Disposition &&
            playerAlignment.Disposition != DispositionType.Neutral)
            alignmentMod -= 0.05m;

        basePrice *= alignmentMod;

        // Diplomacy discount (0-15%)
        var diplomacyDiscount = GetDiplomacyModifier(playerSkills, dataProvider);
        basePrice *= (1.0m - diplomacyDiscount);

        return Math.Max(1, Math.Round(basePrice));
    }

    public static decimal CalculateSellPrice(
        Item item,
        Alignment playerAlignment,
        Alignment merchantAlignment,
        List<Skill> playerSkills,
        IGameDataProvider dataProvider)
    {
        var basePrice = item.TradeValue;

        if (item.Quality != null && item.Quality.TradeValueMultiplier != 0)
            basePrice *= item.Quality.TradeValueMultiplier;
        if (item.Material != null && item.Material.TradeValueMultiplier != 0)
            basePrice *= item.Material.TradeValueMultiplier;

        // 50% sell fraction
        basePrice *= 0.50m;

        // Alignment modifier (inverted)
        var alignmentMod = 1.0m;
        if (merchantAlignment.Disposition == DispositionType.Good)
            alignmentMod += 0.10m;
        else if (merchantAlignment.Disposition == DispositionType.Evil)
            alignmentMod -= 0.10m;

        basePrice *= alignmentMod;

        // Diplomacy bonus (0-15%)
        var diplomacyBonus = GetDiplomacyModifier(playerSkills, dataProvider);
        basePrice *= (1.0m + diplomacyBonus);

        return Math.Max(1, Math.Round(basePrice));
    }

    private static decimal GetDiplomacyModifier(List<Skill> playerSkills, IGameDataProvider dataProvider)
    {
        var diplomacySkill = playerSkills.Find(s => s.Type != null && s.Type.Name == "Diplomacy");
        if (diplomacySkill?.Level == null) return 0m;

        var skillLevels = dataProvider.GetSkillLevels();
        var level = skillLevels.Find(sl => sl.Name == diplomacySkill.Level.Name);
        if (level == null) return 0m;

        var midpoint = (level.ConferredEffectivenessMinimum + level.ConferredEffectivenessMaximum) / 2.0m;
        return midpoint * 0.15m; // 0-15% range
    }
}
