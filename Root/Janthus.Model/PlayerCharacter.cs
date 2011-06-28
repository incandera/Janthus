using System.Collections.Generic;

namespace Janthus.Model
{
    public class PlayerCharacter : LeveledActor, IAligned, ISkilled
    {
        #region IAligned Implementation...

        public Alignment Alignment { get; set; }

        #endregion

        #region ISkilled Implementation...

        public List<Skill> Skill { get; set; }

        #endregion
    }
}
