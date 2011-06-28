using System.Collections.Generic;

namespace Janthus.Model
{
    public class Actor : JanthusObject
    {
        #region Constructors...

        public Actor() { }

        public Actor(decimal hitPoints) { CurrentHitPoints = hitPoints; }

        #endregion

        #region Properties...

        public ActorType Type { get; set; }
        public decimal CurrentHitPoints { get; set; }
        public decimal SizeMultiplier { get; set; }

        public virtual List<Attack> AttackList { get; set; }
        public List<Effect> EffectImmunityList { get; set; }
        public List<Effect> EffectVulnerabilityList { get; set; }

        #endregion
    }
}
