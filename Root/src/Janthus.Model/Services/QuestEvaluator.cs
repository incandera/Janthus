using Janthus.Model.Entities;
using Janthus.Model.Enums;

namespace Janthus.Model.Services;

public static class QuestEvaluator
{
    public static QuestStatus GetQuestStatus(QuestDefinition quest, IGameDataProvider dataProvider)
    {
        // Failed takes precedence
        if (!string.IsNullOrEmpty(quest.FailureFlag) && dataProvider.GetGameFlag(quest.FailureFlag) != null)
            return QuestStatus.Failed;

        if (!string.IsNullOrEmpty(quest.CompletionFlag) && dataProvider.GetGameFlag(quest.CompletionFlag) != null)
            return QuestStatus.Completed;

        if (!string.IsNullOrEmpty(quest.ActivationFlag) && dataProvider.GetGameFlag(quest.ActivationFlag) != null)
            return QuestStatus.Active;

        return QuestStatus.NotStarted;
    }

    public static bool IsGoalComplete(QuestGoal goal, IGameDataProvider dataProvider)
    {
        return !string.IsNullOrEmpty(goal.CompletionFlag) &&
               dataProvider.GetGameFlag(goal.CompletionFlag) != null;
    }

    public static List<QuestDefinition> GetVisibleQuests(List<QuestDefinition> quests, IGameDataProvider dataProvider)
    {
        var visible = new List<QuestDefinition>();
        foreach (var quest in quests)
        {
            var status = GetQuestStatus(quest, dataProvider);
            if (status != QuestStatus.NotStarted)
                visible.Add(quest);
        }
        return visible;
    }
}
