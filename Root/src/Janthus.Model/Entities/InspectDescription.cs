namespace Janthus.Model.Entities;

public class InspectDescription
{
    public int Id { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public string TargetKey { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<InspectCondition> Conditions { get; set; } = new();
}
