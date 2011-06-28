namespace Janthus.Model
{
    public class Alignment
    {
        #region Properties...

        /// <summary>
        /// One of: Lawful, Neutral, Chaotic. Among other things, modifies the tendency of a NPC instigating a
        /// random encounter.
        /// </summary>
        public Enumerations.LawfulnessType Lawfulness { get; set; }

        /// <summary>
        /// One of: Good, Neutral, Evil. Among other things, modifies the tendency of a NPC instigating a
        /// random encounter.
        /// </summary>
        public Enumerations.DispositionType Disposition { get; set; }

        #endregion

        #region Constructors...

        public Alignment() { }

        public Alignment(Enumerations.LawfulnessType lawfulness, Enumerations.DispositionType disposition)
        {
            Lawfulness = lawfulness;
            Disposition = disposition;
        }

        #endregion
    }
}
