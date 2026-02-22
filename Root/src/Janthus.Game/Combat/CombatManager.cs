using Microsoft.Xna.Framework;
using Janthus.Model.Entities;
using Janthus.Model.Enums;
using Janthus.Model.Services;
using Janthus.Game.Actors;
using Janthus.Game.Audio;

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
    private readonly AudioManager _audioManager;
    private readonly Random _rng = new();

    public Action OnCombatStarted { get; set; }
    public Action OnAllCombatEnded { get; set; }
    public IReadOnlyList<CombatLogEntry> CombatLog => _log;

    private bool _wasInCombat;
    private int _playerTileX = -1;
    private int _playerTileY = -1;

    public CombatManager(IGameDataProvider dataProvider, AudioManager audioManager)
    {
        _dataProvider = dataProvider;
        _audioManager = audioManager;
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
        _audioManager.PlaySound(SoundId.CombatStart);
        if (!_wasInCombat)
        {
            _wasInCombat = true;
            OnCombatStarted?.Invoke();
        }
    }

    public void InitiateFollowerCombat(ActorSprite followerSprite, ActorSprite targetSprite)
    {
        if (targetSprite.DomainActor.Status == ActorStatus.Dead) return;
        if (followerSprite.DomainActor.Status == ActorStatus.Dead) return;

        // Check if already in combat with this target
        foreach (var enc in _encounters)
        {
            if (enc.IsActive && enc.Attacker == followerSprite && enc.Defender == targetSprite)
                return;
        }

        _encounters.Add(new CombatEncounter
        {
            Attacker = followerSprite,
            Defender = targetSprite,
            AttackTimer = 0.8f
        });

        _encounters.Add(new CombatEncounter
        {
            Attacker = targetSprite,
            Defender = followerSprite,
            AttackTimer = AttackInterval
        });

        AddLogEntry($"{followerSprite.Label} joins combat against {targetSprite.Label}!", Color.LightBlue);
    }

    public void Update(GameTime gameTime, ActorSprite playerSprite, List<NpcController> npcControllers,
                       List<FollowerController> followerControllers = null)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Cache player position for sound attenuation
        _playerTileX = playerSprite.TileX;
        _playerTileY = playerSprite.TileY;

        // Check proximity aggro
        if (playerSprite.DomainActor.Status == ActorStatus.Alive)
        {
            foreach (var npc in npcControllers)
            {
                if (!npc.Sprite.IsAdversary) continue;
                if (npc.Sprite.DomainActor.Status == ActorStatus.Dead) continue;

                // Already in combat with player?
                var alreadyEngagedPlayer = false;
                foreach (var enc in _encounters)
                {
                    if (enc.IsActive &&
                        ((enc.Attacker == npc.Sprite && enc.Defender == playerSprite) ||
                         (enc.Attacker == playerSprite && enc.Defender == npc.Sprite)))
                    {
                        alreadyEngagedPlayer = true;
                        break;
                    }
                }

                var dx = npc.Sprite.TileX - playerSprite.TileX;
                var dy = npc.Sprite.TileY - playerSprite.TileY;
                var dist = Math.Sqrt(dx * dx + dy * dy);

                if (!alreadyEngagedPlayer && dist <= AggroRange)
                {
                    InitiatePlayerAttack(playerSprite, npc.Sprite);
                }

                // Engage nearby followers when adversary aggroes
                if (followerControllers != null && dist <= AggroRange)
                {
                    foreach (var follower in followerControllers)
                    {
                        if (follower.Sprite.DomainActor.Status == ActorStatus.Dead) continue;

                        var fdx = npc.Sprite.TileX - follower.Sprite.TileX;
                        var fdy = npc.Sprite.TileY - follower.Sprite.TileY;
                        var fdist = Math.Sqrt(fdx * fdx + fdy * fdy);
                        if (fdist > AggroRange) continue;

                        InitiateFollowerCombat(follower.Sprite, npc.Sprite);
                        follower.CombatTarget = npc.Sprite;
                    }
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
                ClearFollowerTarget(followerControllers, enc.Attacker, enc.Defender);
                _encounters.RemoveAt(i);
                continue;
            }

            // End combat if too far apart (use max range of either combatant)
            var combatDx = enc.Attacker.TileX - enc.Defender.TileX;
            var combatDy = enc.Attacker.TileY - enc.Defender.TileY;
            var combatDist = Math.Sqrt(combatDx * combatDx + combatDy * combatDy);
            var effectiveRange = Math.Max(GetMaxCombatRange(enc.Attacker), GetMaxCombatRange(enc.Defender));
            if (combatDist > effectiveRange)
            {
                enc.IsActive = false;
                ClearFollowerTarget(followerControllers, enc.Attacker, enc.Defender);
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

        // Check if all combat ended
        if (_wasInCombat && _encounters.Count == 0)
        {
            _wasInCombat = false;
            OnAllCombatEnded?.Invoke();
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

        // Calculate distance between combatants
        var dx = encounter.Attacker.TileX - encounter.Defender.TileX;
        var dy = encounter.Attacker.TileY - encounter.Defender.TileY;
        var dist = (float)Math.Sqrt(dx * dx + dy * dy);

        // Distance from player for sound attenuation
        var soundDist = _playerTileX >= 0
            ? (float)Math.Sqrt(
                Math.Pow(encounter.Attacker.TileX - _playerTileX, 2) +
                Math.Pow(encounter.Attacker.TileY - _playerTileY, 2))
            : 0f;

        // Try spell first
        var spell = CombatCalculator.SelectOperation(attacker, attackerSkills, dist);
        if (spell != null && spell.EffectType == EffectType.Magical)
        {
            // Deduct mana
            attacker.CurrentMana -= spell.ManaCost;

            // Roll magic hit
            if (!CombatCalculator.RollMagicHit(attacker, attackerSkills, defender, _dataProvider, _rng))
            {
                AddLogEntry($"{encounter.Attacker.Label}'s {spell.Name} fizzles against {encounter.Defender.Label}!", Color.LightGray);
                _audioManager.PlaySoundAtDistance(SoundId.SpellFizzle, soundDist);
                return;
            }

            // Calculate and apply magic damage
            var magicDamage = CombatCalculator.CalculateMagicDamage(attacker, attackerSkills, spell,
                defender, defenderSkills, _dataProvider, _rng);
            defender.CurrentHitPoints -= magicDamage;

            AddLogEntry($"{encounter.Attacker.Label} casts {spell.Name} on {encounter.Defender.Label} for {magicDamage} damage!", Color.MediumPurple);
            _audioManager.PlaySoundAtDistance(SoundId.SpellCast, soundDist);
        }
        else
        {
            // Physical attack — skip if out of melee range
            if (dist > MaxCombatRange) return;

            // Roll hit
            if (!CombatCalculator.RollHit(attacker, attackerSkills, defender, _dataProvider, _rng))
            {
                AddLogEntry($"{encounter.Attacker.Label} misses {encounter.Defender.Label}!", Color.LightGray);
                _audioManager.PlaySoundAtDistance(SoundId.MeleeMiss, soundDist);
                return;
            }

            // Calculate and apply damage
            var damage = CombatCalculator.CalculateDamage(attacker, attackerSkills, defender, defenderSkills, _dataProvider, _rng);
            defender.CurrentHitPoints -= damage;

            AddLogEntry($"{encounter.Attacker.Label} hits {encounter.Defender.Label} for {damage} damage!", Color.Orange);
            _audioManager.PlaySoundAtDistance(SoundId.MeleeHit, soundDist);
        }

        if (defender.CurrentHitPoints <= 0)
        {
            defender.CurrentHitPoints = 0;
            defender.Status = ActorStatus.Dead;
            AddLogEntry($"{encounter.Defender.Label} has been slain!", Color.Red);
            _audioManager.PlaySoundAtDistance(SoundId.Death, soundDist);
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

    private void ClearFollowerTarget(List<FollowerController> followers, ActorSprite attacker, ActorSprite defender)
    {
        if (followers == null) return;
        foreach (var f in followers)
        {
            if (f.CombatTarget == attacker || f.CombatTarget == defender)
            {
                // Only clear if the target is dead or no longer in combat
                if (f.CombatTarget.DomainActor.Status == ActorStatus.Dead || !IsInCombat(f.Sprite))
                    f.CombatTarget = null;
            }
        }
    }

    private float GetMaxCombatRange(ActorSprite sprite)
    {
        var actor = sprite.DomainActor as LeveledActor;
        if (actor == null) return MaxCombatRange;

        var skills = GetSkills(sprite);
        var maxRange = MaxCombatRange;
        foreach (var skill in skills)
        {
            foreach (var op in skill.ConferredOperationList)
            {
                if (op.ManaCost <= actor.CurrentMana && op.BasePower > 0 && op.Range > maxRange)
                    maxRange = op.Range;
            }
        }
        return maxRange;
    }

    private List<Skill> GetSkills(ActorSprite sprite)
    {
        if (sprite.DomainActor is PlayerCharacter pc)
            return pc.Skills;
        if (sprite.DomainActor is NonPlayerCharacter npc)
            return npc.Skills;
        return new List<Skill>();
    }

    public void AddEventLog(string message, Color color)
    {
        AddLogEntry(message, color);
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
