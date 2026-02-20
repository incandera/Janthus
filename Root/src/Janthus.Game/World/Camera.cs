using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Janthus.Game.World;

public class Camera
{
    public Vector2 Position { get; set; }
    public float Zoom { get; set; } = 1.0f;
    public float MinZoom { get; } = 0.5f;
    public float MaxZoom { get; } = 3.0f;
    private Viewport _viewport;

    public Camera(Viewport viewport)
    {
        _viewport = viewport;
    }

    public void UpdateViewport(Viewport viewport)
    {
        _viewport = viewport;
    }

    public Matrix GetTransformMatrix()
    {
        return Matrix.CreateTranslation(-Position.X, -Position.Y, 0f) *
               Matrix.CreateScale(Zoom, Zoom, 1f) *
               Matrix.CreateTranslation(_viewport.Width / 2f, _viewport.Height / 2f, 0f);
    }

    public void Follow(Vector2 targetScreenPos, IsometricRenderer renderer)
    {
        var lerpSpeed = 0.1f;
        Position = Vector2.Lerp(Position, targetScreenPos, lerpSpeed);
    }

    public void AdjustZoom(float delta)
    {
        Zoom = MathHelper.Clamp(Zoom + delta, MinZoom, MaxZoom);
    }

    public Viewport GetViewport() => _viewport;

    public Vector2 ScreenToWorld(Vector2 screenPos)
    {
        var inverseTransform = Matrix.Invert(GetTransformMatrix());
        return Vector2.Transform(screenPos, inverseTransform);
    }
}
