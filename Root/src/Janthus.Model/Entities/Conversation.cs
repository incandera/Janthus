namespace Janthus.Model.Entities;

public class Conversation
{
    public int Id { get; set; }
    public string NpcName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Priority { get; set; }
    public int EntryNodeId { get; set; }
    public bool IsRepeatable { get; set; } = true;
    public List<ConversationCondition> Conditions { get; set; } = new();
}
