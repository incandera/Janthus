using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Janthus.Game.Input;

namespace Janthus.Game.UI;

public class PauseMenuPanel : UIPanel
{
    private readonly string[] _options = { "Resume", "Save", "Load", "Quit" };
    private int _selectedIndex;

    public bool ResumeRequested { get; set; }
    public bool SaveRequested { get; set; }
    public bool LoadRequested { get; set; }
    public bool QuitRequested { get; set; }

    public PauseMenuPanel(Texture2D pixelTexture, SpriteFont font, Rectangle bounds)
        : base(pixelTexture, font, bounds)
    {
        IsVisible = false;
    }

    public override void Update(GameTime gameTime, InputManager input)
    {
        if (!IsVisible) return;

        if (input.IsKeyPressed(Keys.Up) || input.IsKeyPressed(Keys.W) || input.ScrollDelta > 0)
        {
            _selectedIndex = (_selectedIndex - 1 + _options.Length) % _options.Length;
        }

        if (input.IsKeyPressed(Keys.Down) || input.IsKeyPressed(Keys.S) || input.ScrollDelta < 0)
        {
            _selectedIndex = (_selectedIndex + 1) % _options.Length;
        }

        if (input.IsKeyPressed(Keys.Enter))
        {
            switch (_selectedIndex)
            {
                case 0: // Resume
                    ResumeRequested = true;
                    break;
                case 1: // Save
                    SaveRequested = true;
                    break;
                case 2: // Load
                    LoadRequested = true;
                    break;
                case 3: // Quit
                    QuitRequested = true;
                    break;
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        DrawPanel(spriteBatch, new Color(10, 10, 20, 230), new Color(120, 100, 60));

        var x = Bounds.X + Bounds.Width / 2;
        var y = Bounds.Y + 20;

        var title = "PAUSED";
        var titleSize = Font.MeasureString(title);
        spriteBatch.DrawString(Font, title, new Vector2(x - titleSize.X / 2, y), Color.Gold);
        y += 40;

        for (int i = 0; i < _options.Length; i++)
        {
            var color = i == _selectedIndex ? Color.Yellow : Color.White;
            var prefix = i == _selectedIndex ? "> " : "  ";
            var text = prefix + _options[i];
            var textSize = Font.MeasureString(text);
            spriteBatch.DrawString(Font, text, new Vector2(x - textSize.X / 2, y), color);
            y += 30;
        }
    }
}
