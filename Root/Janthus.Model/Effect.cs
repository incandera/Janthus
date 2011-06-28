namespace Janthus.Model
{
    public class Effect : JanthusObject
    {
        public Effect Negates { get; set; }
        public Effect NegatedBy { get; set; }
    }
}
