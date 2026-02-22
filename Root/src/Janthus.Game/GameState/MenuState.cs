using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Janthus.Game.Audio;
using Janthus.Game.Input;
using Janthus.Game.Saving;

namespace Janthus.Game.GameState;

public class MenuState : IGameState
{
    private readonly JanthusGame _game;
    private readonly InputManager _input;
    private readonly SpriteFont _font;
    private readonly AudioManager _audioManager;

    private readonly string[] _menuItems;
    private readonly bool _hasContinue;
    private int _selectedIndex;

    public MenuState(JanthusGame game, InputManager input, SpriteFont font, AudioManager audioManager)
    {
        _game = game;
        _input = input;
        _font = font;
        _audioManager = audioManager;

        _hasContinue = SaveManager.AnySavesExist();
        _menuItems = _hasContinue
            ? new[] { "Continue", "Load Game", "New Game", "Options", "Quit" }
            : new[] { "New Game", "Options", "Quit" };
    }

    public void Enter()
    {
        _audioManager.PlayMusic(MusicId.Menu);
    }

    public void Exit() { }

    public void Update(GameTime gameTime)
    {
        // Menu navigation (keyboard + mousewheel)
        if (_input.IsKeyPressed(Keys.Up) || _input.ScrollDelta > 0)
        {
            _selectedIndex = (_selectedIndex - 1 + _menuItems.Length) % _menuItems.Length;
            _audioManager.PlaySound(SoundId.UINavigate);
        }
        if (_input.IsKeyPressed(Keys.Down) || _input.ScrollDelta < 0)
        {
            _selectedIndex = (_selectedIndex + 1) % _menuItems.Length;
            _audioManager.PlaySound(SoundId.UINavigate);
        }

        // Select
        if (_input.IsKeyPressed(Keys.Enter) &&
            !_input.IsKeyDown(Keys.LeftAlt) && !_input.IsKeyDown(Keys.RightAlt))
        {
            _audioManager.PlaySound(SoundId.UISelect);
            var selected = _menuItems[_selectedIndex];
            switch (selected)
            {
                case "Continue":
                    var saveData = SaveManager.LoadMostRecent();
                    if (saveData != null)
                        _game.StartFromSave(saveData);
                    break;
                case "Load Game":
                    var loadState = new LoadGameState(_game, _input, _font, _audioManager);
                    _game.StateManager.PushState(loadState);
                    break;
                case "New Game":
                    _game.StartPlaying();
                    break;
                case "Options":
                    var options = new OptionsState(_game, _input, _font, _audioManager);
                    _game.StateManager.PushState(options);
                    break;
                case "Quit":
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

        for (int i = 0; i < _menuItems.Length; i++)
        {
            var item = _menuItems[i];
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
