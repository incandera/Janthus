using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Janthus.Game.Input;
using Janthus.Game.Settings;

namespace Janthus.Game.GameState;

public class OptionsState : IGameState
{
    private readonly JanthusGame _game;
    private readonly InputManager _input;
    private readonly SpriteFont _font;

    private int _selectedRow; // 0 = Resolution, 1 = Fullscreen
    private int _entryResolutionIndex;
    private bool _entryIsFullScreen;

    public OptionsState(JanthusGame game, InputManager input, SpriteFont font)
    {
        _game = game;
        _input = input;
        _font = font;
    }

    public void Enter()
    {
        _entryResolutionIndex = _game.ResolutionIndex;
        _entryIsFullScreen = _game.IsFullScreen;
        _selectedRow = 0;
    }

    public void Exit() { }

    public void Update(GameTime gameTime)
    {
        // Navigation (keyboard + mousewheel)
        if (_input.IsKeyPressed(Keys.Up) || _input.ScrollDelta > 0)
            _selectedRow = _selectedRow == 0 ? 1 : 0;
        if (_input.IsKeyPressed(Keys.Down) || _input.ScrollDelta < 0)
            _selectedRow = _selectedRow == 1 ? 0 : 1;

        // Cycle option values (live preview)
        if (_input.IsKeyPressed(Keys.Left) || _input.IsKeyPressed(Keys.Right))
        {
            int direction = _input.IsKeyPressed(Keys.Right) ? 1 : -1;

            if (_selectedRow == 0) // Resolution
            {
                int newIndex = (_game.ResolutionIndex + direction + _game.ResolutionCount) % _game.ResolutionCount;
                _game.SetResolution(newIndex);
            }
            else // Fullscreen
            {
                _game.SetFullScreen(!_game.IsFullScreen);
            }
        }

        // Enter — apply & save, pop back
        if (_input.IsKeyPressed(Keys.Enter) &&
            !_input.IsKeyDown(Keys.LeftAlt) && !_input.IsKeyDown(Keys.RightAlt))
        {
            var settings = new GameSettings
            {
                ResolutionIndex = _game.ResolutionIndex,
                IsFullScreen = _game.IsFullScreen
            };
            settings.Save();
            _game.StateManager.PopState();
        }

        // Escape — revert to entry values, pop back
        if (_input.IsKeyPressed(Keys.Escape))
        {
            _game.SetResolution(_entryResolutionIndex);
            _game.SetFullScreen(_entryIsFullScreen);
            _game.StateManager.PopState();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var viewport = spriteBatch.GraphicsDevice.Viewport;

        // Title
        var title = "OPTIONS";
        var titleSize = _font.MeasureString(title);
        var titlePos = new Vector2((viewport.Width - titleSize.X) / 2, viewport.Height * 0.2f);
        spriteBatch.DrawString(_font, title, titlePos, Color.Gold);

        // Options rows
        float centerX = viewport.Width / 2f;
        float startY = viewport.Height * 0.4f;
        float rowSpacing = 30f;

        var resLabel = JanthusGame.Resolutions[_game.ResolutionIndex];
        var resText = $"< {resLabel.Width}x{resLabel.Height} >";
        DrawOptionRow(spriteBatch, "Resolution:", resText, centerX, startY, _selectedRow == 0);

        var fsText = _game.IsFullScreen ? "< On >" : "< Off >";
        DrawOptionRow(spriteBatch, "Fullscreen:", fsText, centerX, startY + rowSpacing, _selectedRow == 1);

        // Footer hints
        var hint = "ENTER to Apply    ESC to Cancel";
        var hintSize = _font.MeasureString(hint);
        var hintPos = new Vector2((viewport.Width - hintSize.X) / 2, viewport.Height * 0.7f);
        spriteBatch.DrawString(_font, hint, hintPos, Color.DarkGray);

        // Current resolution in corner
        var resCorner = _game.CurrentResolutionLabel;
        var resCornerSize = _font.MeasureString(resCorner);
        spriteBatch.DrawString(_font, resCorner,
            new Vector2(viewport.Width - resCornerSize.X - 10, viewport.Height - resCornerSize.Y - 10),
            Color.Gray);
    }

    private void DrawOptionRow(SpriteBatch spriteBatch, string label, string value,
        float centerX, float y, bool selected)
    {
        var color = selected ? Color.White : Color.Gray;
        var indicator = selected ? "> " : "  ";

        var labelSize = _font.MeasureString(label);
        var valueSize = _font.MeasureString(value);

        float gap = 16f;
        float totalWidth = labelSize.X + gap + valueSize.X;
        float startX = centerX - totalWidth / 2f;

        // Selection indicator
        var indSize = _font.MeasureString(indicator);
        spriteBatch.DrawString(_font, indicator,
            new Vector2(startX - indSize.X, y), Color.Gold);

        // Label
        spriteBatch.DrawString(_font, label,
            new Vector2(startX, y), color);

        // Value
        spriteBatch.DrawString(_font, value,
            new Vector2(startX + labelSize.X + gap, y), color);
    }
}
