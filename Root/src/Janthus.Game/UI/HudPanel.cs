using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Janthus.Model.Entities;
using Janthus.Game.Input;

namespace Janthus.Game.UI;

public class HudPanel : UIPanel
{
    private readonly PlayerCharacter _player;
    private bool _isPaused;

    public HudPanel(Texture2D pixelTexture, SpriteFont font, PlayerCharacter player, Rectangle bounds)
        : base(pixelTexture, font, bounds)
    {
        _player = player;
        IsVisible = true;
    }

    public void SetPaused(bool paused) => _isPaused = paused;

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        DrawPanel(spriteBatch, new Color(20, 20, 30, 200), new Color(80, 80, 100));

        var x = Bounds.X + 10;
        var y = Bounds.Y + 8;

        // Player name & level
        var levelText = _player.Level != null ? $"Lv.{_player.Level.Number}" : "Lv.?";
        var nameText = $"{_player.Name}  {levelText}";
        spriteBatch.DrawString(Font, nameText, new Vector2(x, y), Color.White);
        y += 22;

        // HP bar
        var maxHp = _player.MaximumHitPoints;
        var currentHp = (double)_player.CurrentHitPoints;
        var hpFill = maxHp > 0 ? (float)(currentHp / maxHp) : 0;
        spriteBatch.DrawString(Font, "HP", new Vector2(x, y), Color.Red);
        DrawBar(spriteBatch, new Rectangle(x + 30, y + 2, 150, 14), hpFill, Color.Red, new Color(60, 20, 20));
        spriteBatch.DrawString(Font, $"{(int)currentHp}/{(int)maxHp}", new Vector2(x + 185, y), Color.White);
        y += 20;

        // Mana bar
        var maxMana = _player.MaximumMana;
        var manaFill = maxMana > 0 ? 1.0f : 0; // Start at full
        spriteBatch.DrawString(Font, "MP", new Vector2(x, y), Color.CornflowerBlue);
        DrawBar(spriteBatch, new Rectangle(x + 30, y + 2, 150, 14), manaFill, Color.CornflowerBlue, new Color(20, 20, 60));
        spriteBatch.DrawString(Font, $"{(int)maxMana}/{(int)maxMana}", new Vector2(x + 185, y), Color.White);

        // Pause indicator
        if (_isPaused)
        {
            var pauseText = "|| PAUSED";
            var pauseSize = Font.MeasureString(pauseText);
            spriteBatch.DrawString(Font, pauseText,
                new Vector2(Bounds.Right - pauseSize.X - 10, Bounds.Y + 8), Color.Yellow);
        }
    }
}
