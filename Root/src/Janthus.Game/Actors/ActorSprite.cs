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

    public Vector2 ScreenPosition => new(
        (TileX - TileY) * (IsometricRenderer.TileWidth / 2),
        (TileX + TileY) * (IsometricRenderer.TileHeight / 2));

    public ActorSprite(Actor domainActor, int tileX, int tileY, Color color, string label = null)
    {
        DomainActor = domainActor;
        TileX = tileX;
        TileY = tileY;
        Color = color;
        Label = label ?? domainActor.Name;
    }
}
