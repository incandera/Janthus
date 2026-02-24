using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FontStashSharp;
using Janthus.Game.Combat;
using Janthus.Game.Input;

namespace Janthus.Game.UI;

public class CombatLogPanel : UIPanel
{
    private readonly CombatManager _combatManager;
    private const int PaddingX = 8;
    private const int PaddingTop = 6;
    private const int LineHeight = 16;

    public CombatLogPanel(Texture2D pixelTexture, SpriteFontBase font, CombatManager combatManager, Rectangle bounds)
        : base(pixelTexture, font, bounds)
    {
        _combatManager = combatManager;
        IsVisible = true; // Always visible
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        var log = _combatManager.CombatLog;
        if (log.Count == 0) return;

        // Semi-transparent background
        spriteBatch.Draw(PixelTexture, Bounds, new Color(10, 10, 20, 160));

        var x = Bounds.X + PaddingX;
        var y = Bounds.Y + PaddingTop;
        var maxLines = (Bounds.Height - PaddingTop * 2) / LineHeight;

        for (int i = 0; i < Math.Min(log.Count, maxLines); i++)
        {
            var entry = log[i];
            var alpha = Math.Clamp(entry.TimeRemaining / 2.0f, 0.1f, 1.0f);
            var color = entry.Color * alpha;

            spriteBatch.DrawString(Font, entry.Message, new Vector2(x, y), color);
            y += LineHeight;
        }
    }
}
