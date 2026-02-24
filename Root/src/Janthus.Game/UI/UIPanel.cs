using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FontStashSharp;
using Janthus.Game.Input;

namespace Janthus.Game.UI;

public abstract class UIPanel
{
    public Rectangle Bounds { get; set; }
    public bool IsVisible { get; set; }
    protected Texture2D PixelTexture { get; }
    protected SpriteFontBase Font { get; }

    protected UIPanel(Texture2D pixelTexture, SpriteFontBase font, Rectangle bounds)
    {
        PixelTexture = pixelTexture;
        Font = font;
        Bounds = bounds;
    }

    public virtual void Update(GameTime gameTime, InputManager input) { }
    public abstract void Draw(SpriteBatch spriteBatch);

    protected void DrawPanel(SpriteBatch spriteBatch, Color backgroundColor, Color borderColor)
    {
        // Background
        spriteBatch.Draw(PixelTexture, Bounds, backgroundColor);

        // Border (top, bottom, left, right)
        spriteBatch.Draw(PixelTexture, new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, 2), borderColor);
        spriteBatch.Draw(PixelTexture, new Rectangle(Bounds.X, Bounds.Bottom - 2, Bounds.Width, 2), borderColor);
        spriteBatch.Draw(PixelTexture, new Rectangle(Bounds.X, Bounds.Y, 2, Bounds.Height), borderColor);
        spriteBatch.Draw(PixelTexture, new Rectangle(Bounds.Right - 2, Bounds.Y, 2, Bounds.Height), borderColor);
    }

    protected void DrawBar(SpriteBatch spriteBatch, Rectangle rect, float fill, Color barColor, Color bgColor)
    {
        spriteBatch.Draw(PixelTexture, rect, bgColor);
        var fillRect = new Rectangle(rect.X, rect.Y, (int)(rect.Width * fill), rect.Height);
        spriteBatch.Draw(PixelTexture, fillRect, barColor);
    }
}
