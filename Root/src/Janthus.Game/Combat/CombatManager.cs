using Microsoft.Xna.Framework;
using Janthus.Model.Entities;
using Janthus.Model.Enums;
using Janthus.Model.Services;
using Janthus.Game.Actors;

namespace Janthus.Game.Combat;

public class CombatManager
{
    public class CombatEncounter
    {
        public ActorSprite Attacker { get; set; }
        public ActorSprite Defender { get; set; }
        public float AttackTimer { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CombatLogEntry
    {
        public string Message { get; set; }
        public Color Color { get; set; }
        public float TimeRemaining { get; set; }
    }

    private const float AttackInterval = 1.5f;
    private const float AggroRange = 4.0f;
    private const float MaxCombatRange = 2.0f;
    private const int MaxLogEntries = 8;
    private const float LogDuration = 6.0f;

    private readonly List<CombatEncounter> _encounters = new();
    private readonly List<CombatLogEntry> _log = new();
    private readonly IGameDataProvider _dataProvider;
    private readonly Random _rng = new();

    public IReadOnlyList<CombatLogEntry> CombatLog => _log;

    public CombatManager(IGameDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public void InitiatePlayerAttack(ActorSprite playerSprite, ActorSprite targetSprite)
    {
        if (targetSprite.DomainActor.Status == ActorStatus.Dead) return;

        // Check if already in combat with this target
        foreach (var enc in _encounters)
        {
            if (enc.IsActive && enc.Attacker == playerSprite && enc.Defender == targetSprite)
                return;
        }

        // Create two encounters: player attacks target, target attacks player
        _encounters.Add(new CombatEncounter
        {
            Attacker = playerSprite,
            Defender = targetSprite,
            AttackTimer = 0.5f // Short initial delay for player
        });

        _encounters.Add(new CombatEncounter
        {
            Attacker = targetSprite,
            Defender = playerSprite,
            AttackTimer = AttackInterval // Full delay for target's counter-attack
        });

        AddLogEntry($"Combat with {targetSprite.Label} begins!", Color.White);
    }

    public void Update(GameTime gameTime, ActorSprite playerSprite, List<NpcController> npcControllers)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Check proximity aggro
        if (playerSprite.DomainActor.Status == ActorStatus.Alive)
        {
            foreach (var npc in npcControllers)
            {
                if (!npc.Sprite.IsAdversary) continue;
                if (npc.Sprite.DomainActor.Status == ActorStatus.Dead) continue;

                // Already in combat with this NPC?
                var alreadyEngaged = false;
                foreach (var enc in _encounters)
                {
                    if (enc.IsActive &&
                        ((enc.Attacker == npc.Sprite && enc.Defender == playerSprite) ||
                         (enc.Attacker == playerSprite && enc.Defender == npc.Sprite)))
                    {
                        alreadyEngaged = true;
                        break;
                    }
                }
                if (alreadyEngaged) continue;

                var dx = npc.Sprite.TileX - playerSprite.TileX;
                var dy = npc.Sprite.TileY - playerSprite.TileY;
                var dist = Math.Sqrt(dx * dx + dy * dy);

                if (dist <= AggroRange)
                {
                    InitiatePlayerAttack(playerSprite, npc.Sprite);
                }
            }
        }

        // Process encounters
        for (int i = _encounters.Count - 1; i >= 0; i--)
        {
            var enc = _encounters[i];
            if (!enc.IsActive)
            {
                _encounters.RemoveAt(i);
                continue;
            }

            // End combat if either party is dead
            if (enc.Attacker.DomainActor.Status == ActorStatus.Dead ||
                enc.Defender.DomainActor.Status == ActorStatus.Dead)
            {
                enc.IsActive = false;
                _encounters.RemoveAt(i);
                continue;
            }

            // End combat if too far apart
            var combatDx = enc.Attacker.TileX - enc.Defender.TileX;
            var combatDy = enc.Attacker.TileY - enc.Defender.TileY;
            var combatDist = Math.Sqrt(combatDx * combatDx + combatDy * combatDy);
            if (combatDist > MaxCombatRange)
            {
                enc.IsActive = false;
                _encounters.RemoveAt(i);
                AddLogEntry($"{enc.Attacker.Label} disengages from {enc.Defender.Label}.", Color.Gray);
                continue;
            }

            // Tick attack timer
            enc.AttackTimer -= deltaTime;
            if (enc.AttackTimer <= 0)
            {
                enc.AttackTimer = AttackInterval;
                ProcessAttack(enc);
            }
        }

        // Decay log entries
        for (int i = _log.Count - 1; i >= 0; i--)
        {
            _log[i].TimeRemaining -= deltaTime;
            if (_log[i].TimeRemaining <= 0)
                _log.RemoveAt(i);
        }
    }

    private void ProcessAttack(CombatEncounter encounter)
    {
        var attacker = encounter.Attacker.DomainActor as LeveledActor;
        var defender = encounter.Defender.DomainActor as LeveledActor;
        if (attacker == null || defender == null) return;

        var attackerSkills = GetSkills(encounter.Attacker);
        var defenderSkills = GetSkills(encounter.Defender);

        // Roll hit
        if (!CombatCalculator.RollHit(attacker, attackerSkills, defender, _dataProvider, _rng))
        {
            AddLogEntry($"{encounter.Attacker.Label} misses {encounter.Defender.Label}!", Color.LightGray);
            return;
        }

        // Calculate and apply damage
        var damage = CombatCalculator.CalculateDamage(attacker, attackerSkills, defender, defenderSkills, _dataProvider, _rng);
        defender.CurrentHitPoints -= damage;

        AddLogEntry($"{encounter.Attacker.Label} hits {encounter.Defender.Label} for {damage} damage!", Color.Orange);

        if (defender.CurrentHitPoints <= 0)
        {
            defender.CurrentHitPoints = 0;
            defender.Status = ActorStatus.Dead;
            AddLogEntry($"{encounter.Defender.Label} has been slain!", Color.Red);
        }
    }

    public bool IsInCombat(ActorSprite sprite)
    {
        foreach (var enc in _encounters)
        {
            if (enc.IsActive && (enc.Attacker == sprite || enc.Defender == sprite))
                return true;
        }
        return false;
    }

    private List<Skill> GetSkills(ActorSprite sprite)
    {
        if (sprite.DomainActor is PlayerCharacter pc)
            return pc.Skills;
        if (sprite.DomainActor is NonPlayerCharacter npc)
            return npc.Skills;
        return new List<Skill>();
    }

    private void AddLogEntry(string message, Color color)
    {
        _log.Insert(0, new CombatLogEntry
        {
            Message = message,
            Color = color,
            TimeRemaining = LogDuration
        });

        while (_log.Count > MaxLogEntries)
            _log.RemoveAt(_log.Count - 1);
    }
}
