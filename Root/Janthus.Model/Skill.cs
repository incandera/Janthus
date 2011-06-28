using System.Collections.Generic;

namespace Janthus.Model
{
    public class Skill
    {
        public SkillType Type { get; set; }

        /// <summary>
        /// Operations conferred upon (made available to) the actor by posessing
        /// this skill.
        /// </summary>
        public List<Operation> ConferredOperationList { get; set; }

        public SkillLevel Level { get; set; }
    }
}
