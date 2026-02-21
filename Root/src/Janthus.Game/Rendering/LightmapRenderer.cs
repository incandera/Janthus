using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Janthus.Game.World;

namespace Janthus.Game.Rendering;

public class LightmapRenderer
{
    private Texture2D _radialGradient;
    public Color AmbientColor { get; set; } = new Color(200, 200, 220);

    public void Initialize(GraphicsDevice device)
    {
        _radialGradient = GenerateRadialGradient(device, 256);
    }

    public void Draw(SpriteBatch spriteBatch, Camera camera, Viewport viewport, List<LightSource> lights)
    {
        if (_radialGradient == null) return;

        // Phase 1: Fill with ambient color (opaque)
        var pixelTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
        pixelTexture.SetData(new[] { Color.White });

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
        spriteBatch.Draw(pixelTexture, new Rectangle(0, 0, viewport.Width, viewport.Height), AmbientColor);
        spriteBatch.End();

        pixelTexture.Dispose();

        // Phase 2: Additive lights with camera transform
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
            SamplerState.LinearClamp, null, null, null,
            camera.GetTransformMatrix());

        foreach (var light in lights)
        {
            if (light.Type == LightType.Ambient) continue;

            var pos = light.EffectivePosition;
            var size = light.Radius * 2;
            var destRect = new Rectangle(
                (int)(pos.X - light.Radius),
                (int)(pos.Y - light.Radius),
                (int)size,
                (int)size);

            var lightColor = new Color(
                (int)(light.Color.R * light.Intensity),
                (int)(light.Color.G * light.Intensity),
                (int)(light.Color.B * light.Intensity));

            spriteBatch.Draw(_radialGradient, destRect, lightColor);
        }

        spriteBatch.End();
    }

    private static Texture2D GenerateRadialGradient(GraphicsDevice device, int size)
    {
        var texture = new Texture2D(device, size, size);
        var pixels = new Color[size * size];
        var center = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                var dx = x - center;
                var dy = y - center;
                var dist = (float)Math.Sqrt(dx * dx + dy * dy) / center;

                // Quadratic falloff
                var alpha = Math.Max(0f, 1f - dist * dist);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetData(pixels);
        return texture;
    }
}
