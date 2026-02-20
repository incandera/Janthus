namespace Janthus.Model.Entities;

public class Actor : JanthusObject
{
    public Actor() { }

    public Actor(decimal hitPoints)
    {
        CurrentHitPoints = hitPoints;
    }

    public ActorType Type { get; set; }
    public decimal CurrentHitPoints { get; set; }
    public decimal SizeMultiplier { get; set; }

    public virtual List<Attack> AttackList { get; set; } = new();
    public List<Effect> EffectImmunityList { get; set; } = new();
    public List<Effect> EffectVulnerabilityList { get; set; } = new();
}
