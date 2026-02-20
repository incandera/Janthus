using Janthus.Model.Enums;

namespace Janthus.Model.Entities;

public class Alignment
{
    public LawfulnessType Lawfulness { get; set; }
    public DispositionType Disposition { get; set; }

    public Alignment() { }

    public Alignment(LawfulnessType lawfulness, DispositionType disposition)
    {
        Lawfulness = lawfulness;
        Disposition = disposition;
    }
}
