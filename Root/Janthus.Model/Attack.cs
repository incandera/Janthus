namespace Janthus.Model
{
    public class Attack : Effect
    {
        /// <summary>
        /// Indicates favored priority of usage, for example a creature that
        /// prefers to try to bite first; then crush; then use a magical attack
        /// </summary>
        public int UsageRank { get; set; }

        public decimal Value { get; set; }
    }
}
