using Janthus.Model.Enums;

namespace Janthus.Model.Entities;

public class Effect : JanthusObject
{
    public EffectType EffectType { get; set; }
    public Effect Negates { get; set; }
    public Effect NegatedBy { get; set; }
}
