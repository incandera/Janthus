namespace Janthus.Model.Entities;

public class QuestGoal
{
    public int Id { get; set; }
    public int QuestDefinitionId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string CompletionFlag { get; set; } = string.Empty;
    public bool IsOptional { get; set; }
    public int SortOrder { get; set; }
}
