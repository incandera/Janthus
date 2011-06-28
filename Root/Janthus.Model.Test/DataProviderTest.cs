using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Janthus.Model.Test
{
    /// <summary>
    /// Summary description for DataProviderTest
    /// </summary>
    [TestClass]
    public class DataProviderTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void GetActorTypes()
        {
            var actorTypes = DataProvider.ActorTypes;

            Assert.IsTrue(actorTypes.Count > 0);
        }

        [TestMethod]
        public void GetBestiary()
        {
            var bestiary = DataProvider.Bestiary;

            Assert.IsTrue(bestiary.Count > 0);
        }

        [TestMethod]
        public void GetClasses()
        {
            var classes = DataProvider.Classes;

            Assert.IsTrue(classes.Count > 0);
        }

        [TestMethod]
        public void GetLevels()
        {
            var levels = DataProvider.Levels;

            Assert.IsTrue(levels.Count > 0);
        }

        [TestMethod]
        public void GetSkillLevels()
        {
            var skillLevels = DataProvider.SkillLevels;

            Assert.IsTrue(skillLevels.Count > 0);
        }

        [TestMethod]
        public void GetSkillTypes()
        {
            var skillTypes = DataProvider.SkillTypes;

            Assert.IsTrue(skillTypes.Count > 0);
        }

        [TestMethod]
        public void CreateNpcExplicitly()
        {
            var npc = new NonPlayerCharacter(3, 2, 2, 4, 2, 3, 3,
                                             new Alignment(Enumerations.LawfulnessType.Lawful, Enumerations.DispositionType.Good));

            Assert.IsNotNull(npc);
            Assert.AreEqual(3, npc.Constitution.Value);
            Assert.AreEqual(2, npc.Dexterity.Value);
            Assert.AreEqual(2, npc.Intelligence.Value);
            Assert.AreEqual(4, npc.Luck.Value);
            Assert.AreEqual(2, npc.Attunement.Value);
            Assert.AreEqual(3, npc.Strength.Value);
            Assert.AreEqual(3, npc.Willpower.Value);
            Assert.AreEqual(Enumerations.LawfulnessType.Lawful, npc.Alignment.Lawfulness);
            Assert.AreEqual(Enumerations.DispositionType.Good, npc.Alignment.Disposition);
        }

        [TestMethod]
        public void CreateNpcByRandomRoll()
        {
            var randomNpc = new NonPlayerCharacter("Mage",
                                                   6,
                                                   new Alignment(Enumerations.LawfulnessType.Neutral, Enumerations.DispositionType.Evil));

            var currentLevel = Helpers.GetLevel(6);
            var nextLevel = Helpers.GetLevel(7);
            var minimumSumOfAttributes = currentLevel.MinimumSumOfAttributes;
            var maximumSumOfAttributes = nextLevel.MinimumSumOfAttributes - 1;

            Assert.IsNotNull(randomNpc);
            Assert.AreEqual(6, randomNpc.Level.Number);
            Assert.IsTrue(randomNpc.SumOfAttributes >= minimumSumOfAttributes);
            Assert.IsTrue(randomNpc.SumOfAttributes <= maximumSumOfAttributes);
            Assert.AreEqual(Enumerations.LawfulnessType.Neutral, randomNpc.Alignment.Lawfulness);
            Assert.AreEqual(Enumerations.DispositionType.Evil, randomNpc.Alignment.Disposition);
        }
    }
}
