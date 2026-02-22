namespace Janthus.Model.Entities;

public class QuestDefinition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ActivationFlag { get; set; } = string.Empty;
    public string CompletionFlag { get; set; } = string.Empty;
    public string FailureFlag { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public List<QuestGoal> Goals { get; set; } = new();
}
