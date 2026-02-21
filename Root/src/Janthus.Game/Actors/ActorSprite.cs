using Microsoft.Xna.Framework;
using Janthus.Model.Entities;
using Janthus.Model.Enums;
using Janthus.Game.Rendering;
using Janthus.Game.World;

namespace Janthus.Game.Actors;

public class ActorSprite
{
    public Actor DomainActor { get; set; }
    public int TileX { get; set; }
    public int TileY { get; set; }
    public Color Color { get; set; }
    public string Label { get; set; }
    public bool IsAdversary { get; set; }
    public bool IsFollower { get; set; }
    public FacingDirection Facing { get; set; } = FacingDirection.South;
    public ActorAnimator Animator { get; set; }
    public CharacterSpriteSheet SpriteSheet { get; set; }

    public Color EffectiveColor => DomainActor.Status == ActorStatus.Dead ? Color.DarkGray : Color;

    public Vector2 VisualPosition { get; set; }
    private Vector2 _targetPosition;
    private const float BaseSpeed = 150f;

    public bool HasReachedTarget => (VisualPosition - _targetPosition).LengthSquared() < 1f;

    public ActorSprite(Actor domainActor, int tileX, int tileY, Color color, string label = null)
    {
        DomainActor = domainActor;
        TileX = tileX;
        TileY = tileY;
        Color = color;
        Label = label ?? domainActor.Name;
        Animator = new ActorAnimator();
    }

    public void SetTilePosition(int newX, int newY, ChunkManager chunkManager)
    {
        // Compute facing from movement delta
        var dx = newX - TileX;
        var dy = newY - TileY;
        if (dx != 0 || dy != 0)
            Facing = ComputeFacing(dx, dy);

        TileX = newX;
        TileY = newY;
        var elevation = chunkManager.GetElevation(newX, newY);
        _targetPosition = RenderConstants.TileToScreen(newX, newY, elevation);
    }

    public void UpdateVisual(float deltaTime, float speedModifier)
    {
        var diff = _targetPosition - VisualPosition;
        var dist = diff.Length();
        if (dist < 0.5f)
        {
            VisualPosition = _targetPosition;
            Animator.Play(AnimationType.Idle);
            Animator.Update(deltaTime);
            return;
        }

        Animator.Play(AnimationType.Walk);
        Animator.Update(deltaTime);

        var moveAmount = BaseSpeed * speedModifier * deltaTime;
        if (moveAmount >= dist)
        {
            VisualPosition = _targetPosition;
        }
        else
        {
            VisualPosition += diff * (moveAmount / dist);
        }
    }

    public void SnapVisualToTile(ChunkManager chunkManager)
    {
        var elevation = chunkManager.GetElevation(TileX, TileY);
        VisualPosition = RenderConstants.TileToScreen(TileX, TileY, elevation);
        _targetPosition = VisualPosition;
    }

    private static FacingDirection ComputeFacing(int dx, int dy)
    {
        // Isometric direction mapping from tile movement delta
        if (dx > 0 && dy > 0) return FacingDirection.South;
        if (dx > 0 && dy == 0) return FacingDirection.SouthEast;
        if (dx > 0 && dy < 0) return FacingDirection.East;
        if (dx == 0 && dy < 0) return FacingDirection.NorthEast;
        if (dx < 0 && dy < 0) return FacingDirection.North;
        if (dx < 0 && dy == 0) return FacingDirection.NorthWest;
        if (dx < 0 && dy > 0) return FacingDirection.West;
        if (dx == 0 && dy > 0) return FacingDirection.SouthWest;
        return FacingDirection.South;
    }
}
