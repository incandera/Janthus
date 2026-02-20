namespace Janthus.Model.Entities;

public class ConversationNode
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public string SpeakerName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool IsEndNode { get; set; }
    public List<ConversationResponse> Responses { get; set; } = new();
}
