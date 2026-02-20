using Janthus.Model.Entities;
using Janthus.Model.Enums;

namespace Janthus.Model.Services;

public static class ConversationManager
{
    public static Conversation FindConversation(
        IGameDataProvider dataProvider,
        string npcName,
        PlayerCharacter player,
        string playerClassName)
    {
        var conversations = dataProvider.GetConversationsForNpc(npcName);

        foreach (var conversation in conversations)
        {
            if (!conversation.IsRepeatable)
            {
                var completedFlag = dataProvider.GetGameFlag($"conv_completed_{conversation.Id}");
                if (completedFlag != null)
                    continue;
            }

            if (conversation.Conditions.Count == 0 ||
                AllConditionsMet(conversation.Conditions, player, playerClassName, dataProvider))
                return conversation;
        }

        return null;
    }

    public static List<ConversationResponse> GetAvailableResponses(
        List<ConversationResponse> allResponses,
        PlayerCharacter player,
        string playerClassName,
        IGameDataProvider dataProvider)
    {
        var available = new List<ConversationResponse>();
        foreach (var response in allResponses)
        {
            if (response.Conditions.Count == 0 ||
                AllConditionsMet(response.Conditions, player, playerClassName, dataProvider))
            {
                available.Add(response);
            }
        }
        return available;
    }

    public static bool AllConditionsMet(
        List<ConversationCondition> conditions,
        PlayerCharacter player,
        string playerClassName,
        IGameDataProvider dataProvider)
    {
        foreach (var condition in conditions)
        {
            if (!EvaluateCondition(condition, player, playerClassName, dataProvider))
                return false;
        }
        return true;
    }

    private static bool EvaluateCondition(
        ConversationCondition condition,
        PlayerCharacter player,
        string playerClassName,
        IGameDataProvider dataProvider)
    {
        switch (condition.ConditionType)
        {
            case ConditionType.None:
                return true;

            case ConditionType.PlayerClass:
                return string.Equals(playerClassName, condition.Value, StringComparison.OrdinalIgnoreCase);

            case ConditionType.PlayerDisposition:
                return string.Equals(player.Alignment.Disposition.ToString(), condition.Value,
                    StringComparison.OrdinalIgnoreCase);

            case ConditionType.PlayerLawfulness:
                return string.Equals(player.Alignment.Lawfulness.ToString(), condition.Value,
                    StringComparison.OrdinalIgnoreCase);

            case ConditionType.MinAttribute:
                return EvaluateMinAttribute(condition.Value, player);

            case ConditionType.MinSkillLevel:
                return EvaluateMinSkillLevel(condition.Value, player, dataProvider);

            case ConditionType.FlagSet:
                return dataProvider.GetGameFlag(condition.Value) != null;

            case ConditionType.FlagNotSet:
                return dataProvider.GetGameFlag(condition.Value) == null;

            case ConditionType.MinLevel:
                if (int.TryParse(condition.Value, out var minLevel) && player.Level != null)
                    return player.Level.Number >= minLevel;
                return false;

            case ConditionType.MinGold:
                if (decimal.TryParse(condition.Value, out var minGold))
                    return player.Gold >= minGold;
                return false;

            case ConditionType.HasItem:
                return player.Inventory.Exists(i => i.Item.Name == condition.Value && i.Quantity > 0);

            default:
                return false;
        }
    }

    private static bool EvaluateMinAttribute(string value, PlayerCharacter player)
    {
        var parts = value.Split(':');
        if (parts.Length != 2 || !int.TryParse(parts[1], out var minValue))
            return false;

        var attrValue = parts[0] switch
        {
            "Constitution" => player.Constitution.Value,
            "Dexterity" => player.Dexterity.Value,
            "Intelligence" => player.Intelligence.Value,
            "Luck" => player.Luck.Value,
            "Attunement" => player.Attunement.Value,
            "Strength" => player.Strength.Value,
            "Willpower" => player.Willpower.Value,
            _ => 0
        };

        return attrValue >= minValue;
    }

    private static bool EvaluateMinSkillLevel(string value, PlayerCharacter player, IGameDataProvider dataProvider)
    {
        var parts = value.Split(':');
        if (parts.Length != 2) return false;

        var skillTypeName = parts[0];
        var requiredLevelName = parts[1];

        var skillLevels = dataProvider.GetSkillLevels();
        var requiredLevel = skillLevels.Find(sl => sl.Name == requiredLevelName);
        if (requiredLevel == null) return false;

        var playerSkill = player.Skills.Find(s => s.Type != null && s.Type.Name == skillTypeName);
        if (playerSkill?.Level == null) return false;

        var playerLevel = skillLevels.Find(sl => sl.Name == playerSkill.Level.Name);
        if (playerLevel == null) return false;

        return playerLevel.Id >= requiredLevel.Id;
    }
}
