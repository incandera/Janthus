namespace Janthus.Model.Entities;

public class Skill
{
    public int Id { get; set; }
    public SkillType Type { get; set; }
    public SkillLevel Level { get; set; }
    public List<Operation> ConferredOperationList { get; set; } = new();
}
