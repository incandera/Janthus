using Xunit;
using Microsoft.EntityFrameworkCore;
using Janthus.Model.Entities;
using Janthus.Model.Enums;
using Janthus.Model.Services;
using Janthus.Data;

namespace Janthus.Model.Tests;

public class QuestEvaluatorTests : IDisposable
{
    private readonly JanthusDbContext _context;
    private readonly GameDataRepository _repository;

    public QuestEvaluatorTests()
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

    private QuestDefinition CreateTestQuest()
    {
        return new QuestDefinition
        {
            Id = 1,
            Name = "Test Quest",
            Description = "A test quest.",
            ActivationFlag = "quest_active_test",
            CompletionFlag = "quest_done_test",
            FailureFlag = "quest_failed_test",
            SortOrder = 1,
            Goals = new List<QuestGoal>
            {
                new QuestGoal { Id = 1, QuestDefinitionId = 1, Description = "Goal 1", CompletionFlag = "goal_1_done", SortOrder = 1 },
                new QuestGoal { Id = 2, QuestDefinitionId = 1, Description = "Goal 2", CompletionFlag = "goal_2_done", SortOrder = 2 }
            }
        };
    }

    [Fact]
    public void GetQuestStatus_NoFlags_ReturnsNotStarted()
    {
        var quest = CreateTestQuest();

        var status = QuestEvaluator.GetQuestStatus(quest, _repository);

        Assert.Equal(QuestStatus.NotStarted, status);
    }

    [Fact]
    public void GetQuestStatus_ActivationFlag_ReturnsActive()
    {
        var quest = CreateTestQuest();
        _repository.SetGameFlag("quest_active_test", "true");

        var status = QuestEvaluator.GetQuestStatus(quest, _repository);

        Assert.Equal(QuestStatus.Active, status);
    }

    [Fact]
    public void GetQuestStatus_CompletionFlag_ReturnsCompleted()
    {
        var quest = CreateTestQuest();
        _repository.SetGameFlag("quest_active_test", "true");
        _repository.SetGameFlag("quest_done_test", "true");

        var status = QuestEvaluator.GetQuestStatus(quest, _repository);

        Assert.Equal(QuestStatus.Completed, status);
    }

    [Fact]
    public void GetQuestStatus_FailureFlag_ReturnsFailed()
    {
        var quest = CreateTestQuest();
        _repository.SetGameFlag("quest_active_test", "true");
        _repository.SetGameFlag("quest_failed_test", "true");

        var status = QuestEvaluator.GetQuestStatus(quest, _repository);

        Assert.Equal(QuestStatus.Failed, status);
    }

    [Fact]
    public void GetQuestStatus_FailureTakesPrecedence()
    {
        var quest = CreateTestQuest();
        _repository.SetGameFlag("quest_active_test", "true");
        _repository.SetGameFlag("quest_done_test", "true");
        _repository.SetGameFlag("quest_failed_test", "true");

        var status = QuestEvaluator.GetQuestStatus(quest, _repository);

        Assert.Equal(QuestStatus.Failed, status);
    }

    [Fact]
    public void IsGoalComplete_FlagSet_ReturnsTrue()
    {
        var goal = new QuestGoal { Id = 1, CompletionFlag = "goal_1_done" };
        _repository.SetGameFlag("goal_1_done", "true");

        var result = QuestEvaluator.IsGoalComplete(goal, _repository);

        Assert.True(result);
    }

    [Fact]
    public void IsGoalComplete_FlagNotSet_ReturnsFalse()
    {
        var goal = new QuestGoal { Id = 1, CompletionFlag = "goal_1_done" };

        var result = QuestEvaluator.IsGoalComplete(goal, _repository);

        Assert.False(result);
    }

    [Fact]
    public void GetVisibleQuests_FiltersNotStarted()
    {
        var quests = new List<QuestDefinition>
        {
            new QuestDefinition
            {
                Id = 1, Name = "Active Quest",
                ActivationFlag = "quest_active_1", CompletionFlag = "quest_done_1", FailureFlag = "",
                SortOrder = 1
            },
            new QuestDefinition
            {
                Id = 2, Name = "Not Started Quest",
                ActivationFlag = "quest_active_2", CompletionFlag = "quest_done_2", FailureFlag = "",
                SortOrder = 2
            },
            new QuestDefinition
            {
                Id = 3, Name = "Completed Quest",
                ActivationFlag = "quest_active_3", CompletionFlag = "quest_done_3", FailureFlag = "",
                SortOrder = 3
            }
        };

        _repository.SetGameFlag("quest_active_1", "true");
        _repository.SetGameFlag("quest_active_3", "true");
        _repository.SetGameFlag("quest_done_3", "true");

        var visible = QuestEvaluator.GetVisibleQuests(quests, _repository);

        Assert.Equal(2, visible.Count);
        Assert.Equal("Active Quest", visible[0].Name);
        Assert.Equal("Completed Quest", visible[1].Name);
    }
}
