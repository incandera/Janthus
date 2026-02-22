using Microsoft.Xna.Framework;
using Janthus.Model.Entities;
using Janthus.Model.Enums;
using Janthus.Game.Audio;
using Janthus.Game.World;

namespace Janthus.Game.Actors;

public class FollowerController
{
    public ActorSprite Sprite { get; }
    public ActorSprite CombatTarget { get; set; }
    private readonly ChunkManager _chunkManager;
    private readonly AudioManager _audioManager;
    private float _moveTimer;
    private List<Point> _path;

    private const float MoveInterval = 0.18f;
    private const float CombatMoveInterval = 0.25f;
    private const int FollowThreshold = 2;
    private const int TeleportThreshold = 15;

    public FollowerController(ActorSprite sprite, ChunkManager chunkManager, AudioManager audioManager)
    {
        Sprite = sprite;
        _chunkManager = chunkManager;
        _audioManager = audioManager;
    }

    public void Update(GameTime gameTime, ActorSprite leader, List<ActorSprite> allActors, bool isInCombat)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Always update visual interpolation
        Sprite.UpdateVisual(deltaTime, 1.0f);

        // Skip movement when dead
        if (Sprite.DomainActor.Status == ActorStatus.Dead) return;

        // Combat movement: move toward target if out of range
        if (isInCombat && CombatTarget != null && CombatTarget.DomainActor.Status == ActorStatus.Alive)
        {
            if (!Sprite.HasReachedTarget) return;

            var maxRange = GetMaxOperationRange();
            var cdx = Sprite.TileX - CombatTarget.TileX;
            var cdy = Sprite.TileY - CombatTarget.TileY;
            var combatDist = (float)Math.Sqrt(cdx * cdx + cdy * cdy);

            // Already in range — stop and let CombatManager handle attacks
            if (combatDist <= maxRange)
            {
                _path = null;
                return;
            }

            // Path toward target
            _moveTimer -= deltaTime;
            if (_moveTimer > 0) return;

            if (_path == null || _path.Count == 0)
            {
                var start = new Point(Sprite.TileX, Sprite.TileY);
                var target = new Point(CombatTarget.TileX, CombatTarget.TileY);
                _path = Pathfinder.FindPathAdjacentTo(_chunkManager, start, target, allActors);
            }

            if (_path is { Count: > 0 })
            {
                var next = _path[0];
                _path.RemoveAt(0);

                if (_chunkManager.IsWalkable(next.X, next.Y))
                {
                    Sprite.SetTilePosition(next.X, next.Y, _chunkManager);
                    _moveTimer = CombatMoveInterval;
                    PlayFootstepAtLeaderDistance(next.X, next.Y, leader);
                }
                else
                {
                    _path = null;
                }

                if (_path != null && _path.Count == 0)
                    _path = null;
            }
            return;
        }

        // Not in combat — clear target and follow leader
        if (!isInCombat)
            CombatTarget = null;

        // Wait for visual to catch up
        if (!Sprite.HasReachedTarget) return;

        // Calculate Chebyshev distance to leader
        var dx = Math.Abs(Sprite.TileX - leader.TileX);
        var dy = Math.Abs(Sprite.TileY - leader.TileY);
        var chebyshevDist = Math.Max(dx, dy);

        // Teleport if too far away
        if (chebyshevDist > TeleportThreshold)
        {
            TeleportNearLeader(leader, allActors);
            return;
        }

        // Don't move if close enough
        if (chebyshevDist <= FollowThreshold)
        {
            _path = null;
            return;
        }

        // Repath to follow leader
        _moveTimer -= deltaTime;
        if (_moveTimer > 0) return;

        if (_path == null || _path.Count == 0)
        {
            var start = new Point(Sprite.TileX, Sprite.TileY);
            var target = new Point(leader.TileX, leader.TileY);
            _path = Pathfinder.FindPathAdjacentTo(_chunkManager, start, target, allActors);
        }

        if (_path is { Count: > 0 })
        {
            var next = _path[0];
            _path.RemoveAt(0);

            if (_chunkManager.IsWalkable(next.X, next.Y))
            {
                Sprite.SetTilePosition(next.X, next.Y, _chunkManager);
                _moveTimer = MoveInterval;
                PlayFootstepAtLeaderDistance(next.X, next.Y, leader);
            }
            else
            {
                _path = null;
            }

            if (_path != null && _path.Count == 0)
                _path = null;
        }
    }

    private float GetMaxOperationRange()
    {
        var skills = GetFollowerSkills();
        var actor = Sprite.DomainActor as LeveledActor;
        float maxRange = 1.0f; // default melee range
        if (actor == null) return maxRange;

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

    private List<Skill> GetFollowerSkills()
    {
        if (Sprite.DomainActor is NonPlayerCharacter npc)
            return npc.Skills;
        return new List<Skill>();
    }

    private void PlayFootstepAtLeaderDistance(int tileX, int tileY, ActorSprite leader)
    {
        var dist = (float)Math.Sqrt(
            Math.Pow(tileX - leader.TileX, 2) + Math.Pow(tileY - leader.TileY, 2));
        var stepTile = _chunkManager.GetTile(tileX, tileY);
        _audioManager.PlayFootstepAtDistance(stepTile?.Name ?? "Dirt", dist);
    }

    public void ClearPath()
    {
        _path = null;
    }

    private void TeleportNearLeader(ActorSprite leader, List<ActorSprite> allActors)
    {
        // Find a walkable tile adjacent to the leader
        for (int tdy = -1; tdy <= 1; tdy++)
        {
            for (int tdx = -1; tdx <= 1; tdx++)
            {
                if (tdx == 0 && tdy == 0) continue;
                var nx = leader.TileX + tdx;
                var ny = leader.TileY + tdy;
                if (!_chunkManager.IsWalkable(nx, ny)) continue;

                // Check no other actor occupies this tile
                var occupied = false;
                foreach (var actor in allActors)
                {
                    if (actor != Sprite && actor.TileX == nx && actor.TileY == ny)
                    {
                        occupied = true;
                        break;
                    }
                }
                if (occupied) continue;

                Sprite.SetTilePosition(nx, ny, _chunkManager);
                Sprite.SnapVisualToTile(_chunkManager);
                _path = null;
                return;
            }
        }
    }
}
