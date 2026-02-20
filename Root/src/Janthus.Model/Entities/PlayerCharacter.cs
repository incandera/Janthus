using Janthus.Model.Interfaces;

namespace Janthus.Model.Entities;

public class PlayerCharacter : LeveledActor, IAligned, ISkilled
{
    public Alignment Alignment { get; set; } = new();
    public List<Skill> Skills { get; set; } = new();
}
