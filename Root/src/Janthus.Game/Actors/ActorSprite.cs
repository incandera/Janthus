using Microsoft.Xna.Framework;
using Janthus.Model.Entities;
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
    }

    public void SetTilePosition(int newX, int newY, ChunkManager chunkManager)
    {
        TileX = newX;
        TileY = newY;
        var elevation = chunkManager.GetElevation(newX, newY);
        var screenX = (newX - newY) * (IsometricRenderer.TileWidth / 2);
        var screenY = (newX + newY) * (IsometricRenderer.TileHeight / 2) - elevation * IsometricRenderer.HeightStep;
        _targetPosition = new Vector2(screenX, screenY);
    }

    public void UpdateVisual(float deltaTime, float speedModifier)
    {
        var diff = _targetPosition - VisualPosition;
        var dist = diff.Length();
        if (dist < 0.5f)
        {
            VisualPosition = _targetPosition;
            return;
        }

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
        var screenX = (TileX - TileY) * (IsometricRenderer.TileWidth / 2);
        var screenY = (TileX + TileY) * (IsometricRenderer.TileHeight / 2) - elevation * IsometricRenderer.HeightStep;
        VisualPosition = new Vector2(screenX, screenY);
        _targetPosition = VisualPosition;
    }
}
