using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Janthus.Model.Entities;
using Janthus.Game.Input;

namespace Janthus.Game.UI;

public class CharacterPanel : UIPanel
{
    private readonly PlayerCharacter _player;

    public CharacterPanel(Texture2D pixelTexture, SpriteFont font, PlayerCharacter player, Rectangle bounds)
        : base(pixelTexture, font, bounds)
    {
        _player = player;
        IsVisible = false;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        DrawPanel(spriteBatch, new Color(20, 20, 40, 220), new Color(100, 80, 60));

        var x = Bounds.X + 15;
        var y = Bounds.Y + 10;
        var lineHeight = 22;

        spriteBatch.DrawString(Font, "CHARACTER", new Vector2(x, y), Color.Gold);
        y += lineHeight + 5;

        spriteBatch.DrawString(Font, $"Name: {_player.Name}", new Vector2(x, y), Color.White);
        y += lineHeight;

        var levelText = _player.Level != null ? $"{_player.Level.Number} ({_player.Level.LevelRankGroupName})" : "?";
        spriteBatch.DrawString(Font, $"Level: {levelText}", new Vector2(x, y), Color.White);
        y += lineHeight;

        spriteBatch.DrawString(Font, $"Alignment: {_player.Alignment.Lawfulness} {_player.Alignment.Disposition}", new Vector2(x, y), Color.LightGray);
        y += lineHeight + 10;

        // Attributes section
        spriteBatch.DrawString(Font, "ATTRIBUTES", new Vector2(x, y), Color.Gold);
        y += lineHeight;

        DrawAttributeLine(spriteBatch, "Constitution", _player.Constitution.Value, x, ref y, lineHeight);
        DrawAttributeLine(spriteBatch, "Strength", _player.Strength.Value, x, ref y, lineHeight);
        DrawAttributeLine(spriteBatch, "Dexterity", _player.Dexterity.Value, x, ref y, lineHeight);
        DrawAttributeLine(spriteBatch, "Intelligence", _player.Intelligence.Value, x, ref y, lineHeight);
        DrawAttributeLine(spriteBatch, "Willpower", _player.Willpower.Value, x, ref y, lineHeight);
        DrawAttributeLine(spriteBatch, "Attunement", _player.Attunement.Value, x, ref y, lineHeight);
        DrawAttributeLine(spriteBatch, "Luck", _player.Luck.Value, x, ref y, lineHeight);

        y += 10;
        spriteBatch.DrawString(Font, "COMBAT", new Vector2(x, y), Color.Gold);
        y += lineHeight;
        spriteBatch.DrawString(Font, $"Max HP: {_player.MaximumHitPoints:F0}", new Vector2(x, y), Color.White);
        y += lineHeight;
        spriteBatch.DrawString(Font, $"Max Mana: {_player.MaximumMana:F0}", new Vector2(x, y), Color.White);
        y += lineHeight;
        spriteBatch.DrawString(Font, $"Sum of Attr: {_player.SumOfAttributes}", new Vector2(x, y), Color.LightGray);

        y += lineHeight + 10;
        if (_player.Skills.Count > 0)
        {
            spriteBatch.DrawString(Font, "SKILLS", new Vector2(x, y), Color.Gold);
            y += lineHeight;
            foreach (var skill in _player.Skills)
            {
                var skillText = $"{skill.Type?.Name ?? "Unknown"}: {skill.Level?.Name ?? "Unknown"}";
                spriteBatch.DrawString(Font, skillText, new Vector2(x, y), Color.White);
                y += lineHeight;
            }
        }
    }

    private void DrawAttributeLine(SpriteBatch spriteBatch, string name, int value, int x, ref int y, int lineHeight)
    {
        spriteBatch.DrawString(Font, $"  {name,-15} {value,3}", new Vector2(x, y), Color.White);
        y += lineHeight;
    }
}
