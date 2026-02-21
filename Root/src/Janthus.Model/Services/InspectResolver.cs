using Janthus.Model.Entities;

namespace Janthus.Model.Services;

public static class InspectResolver
{
    public static string ResolveDescription(
        IGameDataProvider dataProvider,
        string targetType,
        string targetKey,
        PlayerCharacter player,
        string playerClassName)
    {
        var descriptions = dataProvider.GetInspectDescriptions(targetType, targetKey);

        foreach (var desc in descriptions.OrderByDescending(d => d.Priority))
        {
            if (desc.Conditions.Count == 0)
                return desc.Text;

            var convConditions = desc.Conditions
                .Select(c => new ConversationCondition
                {
                    ConditionType = c.ConditionType,
                    Value = c.Value
                })
                .ToList();

            if (ConversationManager.AllConditionsMet(convConditions, player, playerClassName, dataProvider))
                return desc.Text;
        }

        return GetFallbackDescription(targetType, targetKey);
    }

    private static string GetFallbackDescription(string targetType, string targetKey)
    {
        return targetType switch
        {
            "Npc" => $"You see {targetKey}. Nothing else stands out.",
            "Object" => $"You see a {targetKey.ToLower()}. Nothing remarkable.",
            "Tile" => "Unremarkable terrain stretches before you.",
            _ => "You see nothing of interest."
        };
    }
}
