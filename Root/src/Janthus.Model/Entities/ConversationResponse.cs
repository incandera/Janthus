namespace Janthus.Model.Entities;

public class ConversationResponse
{
    public int Id { get; set; }
    public int NodeId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int NextNodeId { get; set; }
    public int SortOrder { get; set; }
    public List<ConversationCondition> Conditions { get; set; } = new();
    public List<ConversationAction> Actions { get; set; } = new();
}
