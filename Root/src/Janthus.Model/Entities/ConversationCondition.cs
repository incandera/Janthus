using Janthus.Model.Enums;

namespace Janthus.Model.Entities;

public class ConversationCondition
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public int ResponseId { get; set; }
    public ConditionType ConditionType { get; set; }
    public string Value { get; set; } = string.Empty;
}
