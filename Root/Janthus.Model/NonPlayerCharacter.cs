using System.Collections.Generic;

namespace Janthus.Model
{
    public class NonPlayerCharacter : LeveledActor, IAligned, ISkilled
    {
        #region Constructors...

        public NonPlayerCharacter() { }

        public NonPlayerCharacter(int constitution,
                                  int dexterity,
                                  int intelligence,
                                  int luck,
                                  int attunement,
                                  int strength,
                                  int willpower,
                                  Alignment alignment) : base(constitution, 
                                                              dexterity, 
                                                              intelligence, 
                                                              luck, 
                                                              attunement, 
                                                              strength, 
                                                              willpower)
        {
            Alignment = alignment;
        }

        public NonPlayerCharacter(string rollAsClass,
                                  int level,
                                  Alignment alignment) : base(rollAsClass, level)
        {
            Alignment = alignment;
        }

        #endregion

        #region IAligned Implementation...

        public Alignment Alignment { get; set; }

        #endregion

        #region ISkilled Implementation...

        public List<Skill> Skill { get; set; }

        #endregion
    }
}
