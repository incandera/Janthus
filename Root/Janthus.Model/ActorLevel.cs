using System.Collections.Generic;

namespace Janthus.Model
{
    public class ActorLevel
    {
        public short Number { get; set; }
        public string LevelRankGroupName { get; set; }
        public short MinimumSumOfAttributes { get; set; }

        /// <summary>
        /// The list of additional effects conferred upon (made available to) the actor by attaining
        /// this level.
        /// </summary>
        public List<Effect> ConferredEffectList { get; set; }
    }
}
