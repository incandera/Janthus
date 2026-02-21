using Microsoft.Xna.Framework;
using Janthus.Game.Actors;

namespace Janthus.Game.Rendering;

public enum LightType
{
    Ambient,
    Point,
    Actor
}

public class LightSource
{
    public LightType Type { get; set; }
    public Vector2 WorldPosition { get; set; }
    public float Radius { get; set; }
    public Color Color { get; set; }
    public float Intensity { get; set; }
    public ActorSprite AttachedActor { get; set; }

    public Vector2 EffectivePosition
    {
        get
        {
            if (AttachedActor != null)
            {
                return AttachedActor.VisualPosition + new Vector2(
                    RenderConstants.TileWidth / 2f,
                    RenderConstants.TileHeight / 2f - RenderConstants.ActorHeight / 2f);
            }
            return WorldPosition;
        }
    }
}
