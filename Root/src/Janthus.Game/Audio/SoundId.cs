namespace Janthus.Game.Audio;

public enum SoundId
{
    // Combat - Melee
    MeleeHit,
    MeleeMiss,

    // Combat - Magic
    SpellCast,
    SpellFizzle,

    // Combat - Events
    CombatStart,
    Death,

    // Footsteps by surface (each may have multiple variants: _01, _02, _03)
    FootstepGrass,
    FootstepStone,
    FootstepDirt,
    FootstepSand,
    FootstepWater,

    // UI
    UISelect,
    UINavigate,
    UIOpen,
    UIClose,

    // Events
    ItemPickup,
    GoldReceive,
    QuestAccepted,
    QuestCompleted,
    FollowerJoined,
    LevelUp,
}
