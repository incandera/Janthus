using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Janthus.Game.Audio;
using Janthus.Game.Input;
using Janthus.Game.Settings;

namespace Janthus.Game.GameState;

public class OptionsState : IGameState
{
    private readonly JanthusGame _game;
    private readonly InputManager _input;
    private readonly SpriteFont _font;
    private readonly AudioManager _audioManager;

    private const int RowCount = 6; // Resolution, Fullscreen, Master, Music, SFX, Ambient
    private int _selectedRow;
    private int _entryResolutionIndex;
    private bool _entryIsFullScreen;
    private float _entryMasterVolume;
    private float _entryMusicVolume;
    private float _entrySfxVolume;
    private float _entryAmbientVolume;

    public OptionsState(JanthusGame game, InputManager input, SpriteFont font, AudioManager audioManager)
    {
        _game = game;
        _input = input;
        _font = font;
        _audioManager = audioManager;
    }

    public void Enter()
    {
        _entryResolutionIndex = _game.ResolutionIndex;
        _entryIsFullScreen = _game.IsFullScreen;
        _entryMasterVolume = _audioManager.MasterVolume;
        _entryMusicVolume = _audioManager.MusicVolume;
        _entrySfxVolume = _audioManager.SfxVolume;
        _entryAmbientVolume = _audioManager.AmbientVolume;
        _selectedRow = 0;
    }

    public void Exit() { }

    public void Update(GameTime gameTime)
    {
        // Navigation
        if (_input.IsKeyPressed(Keys.Up) || _input.ScrollDelta > 0)
            _selectedRow = (_selectedRow - 1 + RowCount) % RowCount;
        if (_input.IsKeyPressed(Keys.Down) || _input.ScrollDelta < 0)
            _selectedRow = (_selectedRow + 1) % RowCount;

        // Cycle option values (live preview)
        if (_input.IsKeyPressed(Keys.Left) || _input.IsKeyPressed(Keys.Right))
        {
            int direction = _input.IsKeyPressed(Keys.Right) ? 1 : -1;

            switch (_selectedRow)
            {
                case 0: // Resolution
                    int newIndex = (_game.ResolutionIndex + direction + _game.ResolutionCount) % _game.ResolutionCount;
                    _game.SetResolution(newIndex);
                    break;
                case 1: // Fullscreen
                    _game.SetFullScreen(!_game.IsFullScreen);
                    break;
                case 2: // Master Volume
                    _audioManager.MasterVolume = Math.Clamp(_audioManager.MasterVolume + direction * 0.1f, 0f, 1f);
                    _audioManager.UpdateMusicVolume();
                    break;
                case 3: // Music Volume
                    _audioManager.MusicVolume = Math.Clamp(_audioManager.MusicVolume + direction * 0.1f, 0f, 1f);
                    _audioManager.UpdateMusicVolume();
                    break;
                case 4: // SFX Volume
                    _audioManager.SfxVolume = Math.Clamp(_audioManager.SfxVolume + direction * 0.1f, 0f, 1f);
                    break;
                case 5: // Ambient Volume
                    _audioManager.AmbientVolume = Math.Clamp(_audioManager.AmbientVolume + direction * 0.1f, 0f, 1f);
                    break;
            }

            _audioManager.PlaySound(SoundId.UINavigate);
        }

        // Enter - apply & save, pop back
        if (_input.IsKeyPressed(Keys.Enter) &&
            !_input.IsKeyDown(Keys.LeftAlt) && !_input.IsKeyDown(Keys.RightAlt))
        {
            var settings = new GameSettings
            {
                ResolutionIndex = _game.ResolutionIndex,
                IsFullScreen = _game.IsFullScreen,
                MasterVolume = _audioManager.MasterVolume,
                MusicVolume = _audioManager.MusicVolume,
                SfxVolume = _audioManager.SfxVolume,
                AmbientVolume = _audioManager.AmbientVolume,
            };
            settings.Save();
            _audioManager.PlaySound(SoundId.UISelect);
            _game.StateManager.PopState();
        }

        // Escape - revert to entry values, pop back
        if (_input.IsKeyPressed(Keys.Escape))
        {
            _game.SetResolution(_entryResolutionIndex);
            _game.SetFullScreen(_entryIsFullScreen);
            _audioManager.MasterVolume = _entryMasterVolume;
            _audioManager.MusicVolume = _entryMusicVolume;
            _audioManager.SfxVolume = _entrySfxVolume;
            _audioManager.AmbientVolume = _entryAmbientVolume;
            _audioManager.UpdateMusicVolume();
            _game.StateManager.PopState();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var viewport = spriteBatch.GraphicsDevice.Viewport;

        // Title
        var title = "OPTIONS";
        var titleSize = _font.MeasureString(title);
        var titlePos = new Vector2((viewport.Width - titleSize.X) / 2, viewport.Height * 0.15f);
        spriteBatch.DrawString(_font, title, titlePos, Color.Gold);

        // Options rows
        float centerX = viewport.Width / 2f;
        float startY = viewport.Height * 0.3f;
        float rowSpacing = 30f;

        var resLabel = JanthusGame.Resolutions[_game.ResolutionIndex];
        var resText = $"< {resLabel.Width}x{resLabel.Height} >";
        DrawOptionRow(spriteBatch, "Resolution:", resText, centerX, startY, _selectedRow == 0);

        var fsText = _game.IsFullScreen ? "< On >" : "< Off >";
        DrawOptionRow(spriteBatch, "Fullscreen:", fsText, centerX, startY + rowSpacing, _selectedRow == 1);

        DrawOptionRow(spriteBatch, "Master Volume:", VolumeText(_audioManager.MasterVolume),
            centerX, startY + rowSpacing * 2, _selectedRow == 2);

        DrawOptionRow(spriteBatch, "Music Volume:", VolumeText(_audioManager.MusicVolume),
            centerX, startY + rowSpacing * 3, _selectedRow == 3);

        DrawOptionRow(spriteBatch, "SFX Volume:", VolumeText(_audioManager.SfxVolume),
            centerX, startY + rowSpacing * 4, _selectedRow == 4);

        DrawOptionRow(spriteBatch, "Ambient Volume:", VolumeText(_audioManager.AmbientVolume),
            centerX, startY + rowSpacing * 5, _selectedRow == 5);

        // Footer hints
        var hint = "ENTER to Apply    ESC to Cancel";
        var hintSize = _font.MeasureString(hint);
        var hintPos = new Vector2((viewport.Width - hintSize.X) / 2, viewport.Height * 0.75f);
        spriteBatch.DrawString(_font, hint, hintPos, Color.DarkGray);

        // Current resolution in corner
        var resCorner = _game.CurrentResolutionLabel;
        var resCornerSize = _font.MeasureString(resCorner);
        spriteBatch.DrawString(_font, resCorner,
            new Vector2(viewport.Width - resCornerSize.X - 10, viewport.Height - resCornerSize.Y - 10),
            Color.Gray);
    }

    private static string VolumeText(float value)
    {
        return $"< {(int)(value * 100 + 0.5f)}% >";
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
