using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FontStashSharp;
using Janthus.Model.Entities;
using Janthus.Model.Services;
using Janthus.Game.Input;

namespace Janthus.Game.UI;

public class HudPanel : UIPanel
{
    private readonly PlayerCharacter _player;
    private bool _isPaused;

    public HudPanel(Texture2D pixelTexture, SpriteFontBase font, PlayerCharacter player, Rectangle bounds)
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
        var xpLevel = ExperienceCalculator.CalculateLevelFromExperience(_player.ExperiencePoints);
        var levelText = $"Lv.{xpLevel}";
        var nameText = $"{_player.Name}  {levelText}";
        spriteBatch.DrawString(Font, nameText, new Vector2(x, y), Color.White);
        y += 24;

        // HP bar
        var maxHp = _player.MaximumHitPoints;
        var currentHp = (double)_player.CurrentHitPoints;
        var hpFill = maxHp > 0 ? (float)(currentHp / maxHp) : 0;
        spriteBatch.DrawString(Font, "HP", new Vector2(x, y), Color.Red);
        DrawBar(spriteBatch, new Rectangle(x + 50, y + 2, 200, 14), hpFill, Color.Red, new Color(60, 20, 20));
        spriteBatch.DrawString(Font, $"{(int)currentHp}/{(int)maxHp}", new Vector2(x + 260, y), Color.White);
        y += 22;

        // Mana bar
        var maxMana = _player.MaximumMana;
        var currentMana = _player.CurrentMana;
        var manaFill = maxMana > 0 ? (float)(currentMana / (decimal)maxMana) : 0;
        spriteBatch.DrawString(Font, "MP", new Vector2(x, y), Color.CornflowerBlue);
        DrawBar(spriteBatch, new Rectangle(x + 50, y + 2, 200, 14), manaFill, Color.CornflowerBlue, new Color(20, 20, 60));
        spriteBatch.DrawString(Font, $"{(int)currentMana}/{(int)maxMana}", new Vector2(x + 260, y), Color.White);
        y += 20;

        // Gold display
        spriteBatch.DrawString(Font, $"Gold: {_player.Gold:F0}", new Vector2(x, y), Color.Yellow);
        y += 22;

        // XP bar
        var currentXp = _player.ExperiencePoints;
        var currentLevelXp = ExperienceCalculator.GetExperienceForLevel(xpLevel);
        var nextLevelXp = ExperienceCalculator.GetExperienceForLevel(xpLevel + 1);
        var xpInLevel = currentXp - currentLevelXp;
        var xpNeeded = nextLevelXp - currentLevelXp;
        var xpFill = xpLevel >= 20 ? 1f : (xpNeeded > 0 ? (float)xpInLevel / xpNeeded : 0f);
        spriteBatch.DrawString(Font, "XP", new Vector2(x, y), Color.Gold);
        DrawBar(spriteBatch, new Rectangle(x + 50, y + 2, 200, 14), xpFill, Color.Gold, new Color(50, 40, 10));
        var xpText = xpLevel >= 20 ? "MAX" : $"{xpInLevel}/{xpNeeded}";
        spriteBatch.DrawString(Font, xpText, new Vector2(x + 260, y), Color.White);

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
