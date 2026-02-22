using Janthus.Model.Enums;

namespace Janthus.Model.Entities;

public class Operation : JanthusObject
{
    public EffectType EffectType { get; set; }
    public decimal BasePower { get; set; }
    public decimal ManaCost { get; set; }
    public double CooldownSeconds { get; set; }
    public float Range { get; set; } = 1.0f;
}
