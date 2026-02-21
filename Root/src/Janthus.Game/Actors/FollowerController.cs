using Microsoft.Xna.Framework;
using Janthus.Model.Enums;
using Janthus.Game.World;

namespace Janthus.Game.Actors;

public class FollowerController
{
    public ActorSprite Sprite { get; }
    private readonly ChunkManager _chunkManager;
    private float _moveTimer;
    private List<Point> _path;

    private const float MoveInterval = 0.18f;
    private const int FollowThreshold = 2;
    private const int TeleportThreshold = 15;

    public FollowerController(ActorSprite sprite, ChunkManager chunkManager)
    {
        Sprite = sprite;
        _chunkManager = chunkManager;
    }

    public void Update(GameTime gameTime, ActorSprite leader, List<ActorSprite> allActors, bool isInCombat)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Always update visual interpolation
        Sprite.UpdateVisual(deltaTime, 1.0f);

        // Skip movement when dead or in combat
        if (Sprite.DomainActor.Status == ActorStatus.Dead) return;
        if (isInCombat) return;

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
            }
            else
            {
                _path = null;
            }

            if (_path != null && _path.Count == 0)
                _path = null;
        }
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
