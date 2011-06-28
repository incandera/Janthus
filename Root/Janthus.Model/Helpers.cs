using System.Linq;

namespace Janthus.Model
{
    public class Helpers
    {
        public static Class GetClass(string name)
        {
            var domainClass = DataProvider.Classes.Where(x => x.Name == name).SingleOrDefault();

            return domainClass;
        }

        public static ActorLevel GetLevel(int number)
        {
            var level = DataProvider.Levels.Where(x => x.Number == number).SingleOrDefault();

            return level;    
        }

        public static ActorLevel CalculateLevel(int sumOfAttributes)
        {
            var levelIndex = DataProvider.Levels.FindIndex(x => x.MinimumSumOfAttributes > sumOfAttributes); // Get the next level "up"
            var level = DataProvider.Levels[levelIndex - 1]; // The target level is the one below that

            return level;            
        }

        public static double CalculateHitPoints(Attribute constitution,
                                                Attribute strength,
                                                Attribute willpower)
        {
            return ((constitution.Value * 0.5) + (strength.Value * 0.25) + (willpower.Value * 0.25)) * 10;
        }

        public static double CalculateMana(Attribute attunement,
                                           Attribute intelligence,
                                           Attribute willpower)
        {
            return ((attunement.Value * 0.5) + (intelligence.Value * 0.25) + (willpower.Value * 0.25)) * 10;
        }
    }
}
