using Janthus.Model.Enums;

namespace Janthus.Model.Entities;

public class ConversationAction
{
    public int Id { get; set; }
    public int ResponseId { get; set; }
    public ConversationActionType ActionType { get; set; }
    public string Value { get; set; } = string.Empty;
}
