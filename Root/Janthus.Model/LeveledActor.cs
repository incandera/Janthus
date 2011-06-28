using System;
using System.Collections.Generic;
using System.Linq;

namespace Janthus.Model
{
    public class LeveledActor : Actor
    {
        #region Fields...

        private Attribute _constitution;
        private Attribute _dexterity;
        private Attribute _intelligence;
        private Attribute _luck;
        private Attribute _attunement;
        private Attribute _strength;
        private Attribute _willpower;

        #endregion

        #region Properties...

        public Attribute Constitution
        {
            get { if (Equals(_constitution, null)) { _constitution = new Attribute(); } return _constitution; }
            set { _constitution = value; }
        }
        public Attribute Dexterity
        {
            get { if (Equals(_dexterity, null)) { _dexterity = new Attribute(); } return _dexterity; }
            set { _dexterity = value; }
        }
        public Attribute Intelligence
        {
            get { if (Equals(_intelligence, null)) { _intelligence = new Attribute(); } return _intelligence; }
            set { _intelligence = value; }
        }
        public Attribute Luck
        {
            get { if (Equals(_luck, null)) { _luck = new Attribute(); } return _luck; }
            set { _luck = value; }
        }
        public Attribute Attunement
        {
            get { if (Equals(_attunement, null)) { _attunement = new Attribute(); } return _attunement; }
            set { _attunement = value; }
        }
        public Attribute Strength
        {
            get { if (Equals(_strength, null)) { _strength = new Attribute(); } return _strength; }
            set { _strength = value; }
        }
        public Attribute Willpower
        {
            get { if (Equals(_willpower, null)) { _willpower = new Attribute(); } return _willpower; }
            set { _willpower = value; }
        }
        public int SumOfAttributes
        {
            get
            {
                var sumOfAttributes = Constitution.Value
                                      + Dexterity.Value
                                      + Intelligence.Value
                                      + Luck.Value
                                      + Attunement.Value
                                      + Strength.Value
                                      + Willpower.Value;

                return sumOfAttributes;
            }
        }

        public ActorLevel Level { get { return Helpers.CalculateLevel(SumOfAttributes); } }
        public double MaximumHitPoints { get { return Helpers.CalculateHitPoints(Constitution, Strength, Willpower); } }
        public double MaximumMana { get { return Helpers.CalculateMana(Attunement, Intelligence, Willpower); } }

        #endregion

        #region Constructors...

        public LeveledActor() { }

        public LeveledActor(string rollAsClass, 
                            int level)
        {
            // Facilitates generating leveled NPCs (e.g. "Level 5 Soldier") for random encounters; "level" indicates the desired 
            // level; the lookup will yield the appropriate number of attribute points to be distributed; "rollAs" will provide 
            // hints for the appropriate distribution of points; for example, rolling as a "Mage" will favor Attunement, Intelligence, 
            // and Willpower, whereas rolling as a "Soldier" will favor Constitution, Strength, and Willpower
            var random = new Random();
            var targetLevel = Helpers.GetLevel(level);
            var nextLevel = Helpers.GetLevel(level + 1); // The next level "up"
            var distributablePointDifference = nextLevel.MinimumSumOfAttributes - targetLevel.MinimumSumOfAttributes;

            // Roll somewhere between the minimum for the level ("low") and the next one ("high")
            var distributablePoints = targetLevel.MinimumSumOfAttributes + (random.Next(0, distributablePointDifference));

            // Assign attribute points according to the weighted spread
            var domainClass = Helpers.GetClass(rollAsClass);

            Constitution.Value = (int)Math.Round(distributablePoints * domainClass.ConstitutionRollWeight);
            Dexterity.Value = (int)Math.Round(distributablePoints * domainClass.DexterityRollWeight);
            Intelligence.Value = (int)Math.Round(distributablePoints * domainClass.IntelligenceRollWeight);
            Luck.Value = (int)Math.Round(distributablePoints * domainClass.LuckRollWeight);
            Attunement.Value = (int)Math.Round(distributablePoints * domainClass.AttunementRollWeight);
            Strength.Value = (int)Math.Round(distributablePoints * domainClass.StrengthRollWeight);
            Willpower.Value = (int)Math.Round(distributablePoints * domainClass.WillpowerRollWeight);

            var rollWeights = new List<KeyValuePair<string, double>>
                                      {
                                          new KeyValuePair<string, double>("Constitution", domainClass.ConstitutionRollWeight),
                                          new KeyValuePair<string, double>("Dexterity", domainClass.DexterityRollWeight),
                                          new KeyValuePair<string, double>("Intelligence", domainClass.IntelligenceRollWeight),
                                          new KeyValuePair<string, double>("Luck", domainClass.LuckRollWeight),
                                          new KeyValuePair<string, double>("Attunement", domainClass.AttunementRollWeight),
                                          new KeyValuePair<string, double>("Strength", domainClass.StrengthRollWeight),
                                          new KeyValuePair<string, double>("Willpower", domainClass.WillpowerRollWeight)
                                      };

            if (SumOfAttributes < distributablePoints)
            {
                // Order by descending to order the attributes by most to least significant for this class
                var orderedRollWeights = rollWeights.OrderByDescending(x => x.Value).ToList();

                do
                {
                    var npcType = GetType();
                    var attribute = npcType.GetProperty(orderedRollWeights[0].Key);

                    // Remove the top of the ordered list, which will contain the _most_ significant
                    // attribute for this class
                    orderedRollWeights.RemoveAt(0);

                    var attributeProperty = (Attribute)attribute.GetValue(this, null);

                    attributeProperty.Value++;

                } while (SumOfAttributes < distributablePoints);
            }
            else if (SumOfAttributes > distributablePoints)
            {
                // Order by ascending to order the attributes by least to most significant for this class
                var orderedRollWeights = rollWeights.OrderBy(x => x.Value).ToList();

                do
                {
                    var npcType = GetType();
                    var attribute = npcType.GetProperty(orderedRollWeights[0].Key);

                    // Remove the top of the ordered list, which will contain the _least_ significant
                    // attribute for this class
                    orderedRollWeights.RemoveAt(0);

                    var attributeProperty = (Attribute)attribute.GetValue(this, null);

                    if (attributeProperty.Value == 1) { continue; }

                    attributeProperty.Value--;

                } while (SumOfAttributes < distributablePoints);
            }
        }

        public LeveledActor(int constitution,
                            int dexterity,
                            int intelligence,
                            int luck,
                            int attunement,
                            int strength,
                            int willpower)
        {
            Constitution.Value = constitution;
            Dexterity.Value = dexterity;
            Intelligence.Value = intelligence;
            Luck.Value = luck;
            Attunement.Value = attunement;
            Strength.Value = strength;
            Willpower.Value = willpower;
        }

        #endregion

        #region Base Overrides...

        public override List<Attack> AttackList { get; set; }

        #endregion
    }
}
