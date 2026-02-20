using Janthus.Model.Interfaces;
using Janthus.Model.Services;

namespace Janthus.Model.Entities;

public class NonPlayerCharacter : LeveledActor, IAligned, ISkilled
{
    public NonPlayerCharacter() { }

    public NonPlayerCharacter(int constitution, int dexterity, int intelligence,
                              int luck, int attunement, int strength, int willpower,
                              Alignment alignment)
        : base(constitution, dexterity, intelligence, luck, attunement, strength, willpower)
    {
        Alignment = alignment;
    }

    public NonPlayerCharacter(IGameDataProvider dataProvider, string rollAsClass,
                              int level, Alignment alignment)
        : base(dataProvider, rollAsClass, level)
    {
        Alignment = alignment;
    }

    public Alignment Alignment { get; set; } = new();
    public List<Skill> Skills { get; set; } = new();
    public decimal Gold { get; set; }
    public List<InventoryItem> Inventory { get; set; } = new();
}
