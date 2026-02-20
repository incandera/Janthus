namespace Janthus.Model.Entities;

public class ActorLevel
{
    public int Id { get; set; }
    public short Number { get; set; }
    public string LevelRankGroupName { get; set; } = string.Empty;
    public short MinimumSumOfAttributes { get; set; }
    public List<Effect> ConferredEffectList { get; set; } = new();
}
