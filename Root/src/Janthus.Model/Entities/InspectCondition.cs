using Janthus.Model.Enums;

namespace Janthus.Model.Entities;

public class InspectCondition
{
    public int Id { get; set; }
    public int InspectDescriptionId { get; set; }
    public ConditionType ConditionType { get; set; }
    public string Value { get; set; } = string.Empty;
}
