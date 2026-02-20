using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Janthus.Game.Input;

namespace Janthus.Game.GameState;

public class MenuState : IGameState
{
    private readonly JanthusGame _game;
    private readonly InputManager _input;
    private readonly SpriteFont _font;

    private static readonly string[] MenuItems = { "New Game", "Options", "Quit" };
    private int _selectedIndex;

    public MenuState(JanthusGame game, InputManager input, SpriteFont font)
    {
        _game = game;
        _input = input;
        _font = font;
    }

    public void Enter() { }
    public void Exit() { }

    public void Update(GameTime gameTime)
    {
        // Menu navigation
        if (_input.IsKeyPressed(Keys.Up))
            _selectedIndex = (_selectedIndex - 1 + MenuItems.Length) % MenuItems.Length;
        if (_input.IsKeyPressed(Keys.Down))
            _selectedIndex = (_selectedIndex + 1) % MenuItems.Length;

        // Select
        if (_input.IsKeyPressed(Keys.Enter) &&
            !_input.IsKeyDown(Keys.LeftAlt) && !_input.IsKeyDown(Keys.RightAlt))
        {
            switch (_selectedIndex)
            {
                case 0: // New Game
                    _game.StartPlaying();
                    break;
                case 1: // Options
                    var options = new OptionsState(_game, _input, _font);
                    _game.StateManager.PushState(options);
                    break;
                case 2: // Quit
                    _game.Exit();
                    break;
            }
        }

        if (_input.IsKeyPressed(Keys.Escape))
        {
            _game.Exit();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var viewport = spriteBatch.GraphicsDevice.Viewport;

        // Title
        var title = "JANTHUS";
        var titleSize = _font.MeasureString(title);
        var titlePos = new Vector2((viewport.Width - titleSize.X) / 2, viewport.Height * 0.25f);
        spriteBatch.DrawString(_font, title, titlePos, Color.Gold);

        var subtitle = "An Isometric RPG";
        var subtitleSize = _font.MeasureString(subtitle);
        var subtitlePos = new Vector2((viewport.Width - subtitleSize.X) / 2, titlePos.Y + titleSize.Y + 10);
        spriteBatch.DrawString(_font, subtitle, subtitlePos, Color.LightGray);

        // Menu items
        float menuStartY = viewport.Height * 0.5f;
        float itemSpacing = 24f;

        for (int i = 0; i < MenuItems.Length; i++)
        {
            var item = MenuItems[i];
            var itemSize = _font.MeasureString(item);
            float y = menuStartY + i * itemSpacing;
            float x = (viewport.Width - itemSize.X) / 2f;

            bool selected = i == _selectedIndex;
            var color = selected ? Color.White : Color.Gray;

            if (selected)
            {
                var indicator = "> ";
                var indSize = _font.MeasureString(indicator);
                spriteBatch.DrawString(_font, indicator,
                    new Vector2(x - indSize.X, y), Color.Gold);
            }

            spriteBatch.DrawString(_font, item, new Vector2(x, y), color);
        }

        // Controls hint
        var controlsY = viewport.Height * 0.75f;
        var controls = new[]
        {
            "F11 / Shift+F11  Cycle Resolution",
            "ALT+Enter        Toggle Fullscreen",
        };
        foreach (var line in controls)
        {
            var lineSize = _font.MeasureString(line);
            spriteBatch.DrawString(_font, line,
                new Vector2((viewport.Width - lineSize.X) / 2, controlsY), Color.DarkGray);
            controlsY += 20;
        }

        // Current resolution
        var resText = _game.CurrentResolutionLabel;
        var resSize = _font.MeasureString(resText);
        spriteBatch.DrawString(_font, resText,
            new Vector2(viewport.Width - resSize.X - 10, viewport.Height - resSize.Y - 10), Color.Gray);
    }
}
