using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Janthus.Game.Audio;
using Janthus.Game.Input;
using Janthus.Game.Saving;

namespace Janthus.Game.GameState;

public class LoadGameState : IGameState
{
    private readonly JanthusGame _game;
    private readonly InputManager _input;
    private readonly SpriteFont _font;
    private readonly AudioManager _audioManager;

    private SaveSlotInfo[] _slots;
    private int _selectedIndex;

    public LoadGameState(JanthusGame game, InputManager input, SpriteFont font, AudioManager audioManager)
    {
        _game = game;
        _input = input;
        _font = font;
        _audioManager = audioManager;
    }

    public void Enter()
    {
        _slots = SaveManager.GetSlotSummaries();
        _selectedIndex = 0;

        // Pre-select the first occupied slot
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].Exists)
            {
                _selectedIndex = i;
                break;
            }
        }
    }

    public void Exit() { }

    public void Update(GameTime gameTime)
    {
        if (_input.IsKeyPressed(Keys.Up) || _input.ScrollDelta > 0)
        {
            _selectedIndex = (_selectedIndex - 1 + _slots.Length) % _slots.Length;
            _audioManager.PlaySound(SoundId.UINavigate);
        }
        if (_input.IsKeyPressed(Keys.Down) || _input.ScrollDelta < 0)
        {
            _selectedIndex = (_selectedIndex + 1) % _slots.Length;
            _audioManager.PlaySound(SoundId.UINavigate);
        }

        if (_input.IsKeyPressed(Keys.Enter) &&
            !_input.IsKeyDown(Keys.LeftAlt) && !_input.IsKeyDown(Keys.RightAlt))
        {
            var slot = _slots[_selectedIndex];
            if (slot.Exists)
            {
                _audioManager.PlaySound(SoundId.UISelect);
                var saveData = SaveManager.LoadFromSlot(slot.Slot);
                if (saveData != null)
                {
                    _game.StateManager.PopState(); // pop LoadGameState
                    _game.StartFromSave(saveData);
                }
            }
        }

        if (_input.IsKeyPressed(Keys.Escape))
        {
            _audioManager.PlaySound(SoundId.UINavigate);
            _game.StateManager.PopState();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var viewport = spriteBatch.GraphicsDevice.Viewport;

        // Title
        var title = "LOAD GAME";
        var titleSize = _font.MeasureString(title);
        var titlePos = new Vector2((viewport.Width - titleSize.X) / 2, viewport.Height * 0.25f);
        spriteBatch.DrawString(_font, title, titlePos, Color.Gold);

        // Slot list
        float slotStartY = viewport.Height * 0.4f;
        float slotSpacing = 30f;

        for (int i = 0; i < _slots.Length; i++)
        {
            var slot = _slots[i];
            bool selected = i == _selectedIndex;
            float y = slotStartY + i * slotSpacing;

            string label;
            Color color;
            if (slot.Exists)
            {
                var localTime = slot.SaveTime.ToLocalTime();
                label = $"Slot {slot.Slot}: {slot.SaveName}  -  {localTime:yyyy-MM-dd  HH:mm}";
                color = selected ? Color.White : Color.Gray;
            }
            else
            {
                label = $"Slot {slot.Slot}: (empty)";
                color = selected ? Color.DarkGray : new Color(60, 60, 60);
            }

            var labelSize = _font.MeasureString(label);
            float x = (viewport.Width - labelSize.X) / 2f;

            if (selected)
            {
                var indicator = "> ";
                var indSize = _font.MeasureString(indicator);
                spriteBatch.DrawString(_font, indicator,
                    new Vector2(x - indSize.X, y), Color.Gold);
            }

            spriteBatch.DrawString(_font, label, new Vector2(x, y), color);
        }

        // Hints
        var hint = "ENTER to Load    ESC to Back";
        var hintSize = _font.MeasureString(hint);
        var hintPos = new Vector2((viewport.Width - hintSize.X) / 2, viewport.Height * 0.75f);
        spriteBatch.DrawString(_font, hint, hintPos, Color.DarkGray);

        // Current resolution in corner
        var resText = _game.CurrentResolutionLabel;
        var resSize = _font.MeasureString(resText);
        spriteBatch.DrawString(_font, resText,
            new Vector2(viewport.Width - resSize.X - 10, viewport.Height - resSize.Y - 10), Color.Gray);
    }
}
