using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FontStashSharp;
using Janthus.Game.Input;
using Janthus.Game.Saving;

namespace Janthus.Game.UI;

public class SaveLoadPanel : UIPanel
{
    private bool _isSaveMode;
    private int _selectedIndex;
    private SaveSlotInfo[] _slots;
    private string _statusMessage;
    private double _statusTimer;

    public Action<int> OnSlotConfirmed { get; set; }

    public SaveLoadPanel(Texture2D pixelTexture, SpriteFontBase font, Rectangle bounds)
        : base(pixelTexture, font, bounds)
    {
        IsVisible = false;
        _slots = new SaveSlotInfo[SaveManager.MaxSlots];
        for (int i = 0; i < SaveManager.MaxSlots; i++)
            _slots[i] = new SaveSlotInfo { Slot = i + 1 };
    }

    public void Show(bool isSaveMode)
    {
        _isSaveMode = isSaveMode;
        _selectedIndex = 0;
        _statusMessage = null;
        _statusTimer = 0;
        _slots = SaveManager.GetSlotSummaries();
        IsVisible = true;
    }

    public void Hide()
    {
        IsVisible = false;
    }

    public override void Update(GameTime gameTime, InputManager input)
    {
        if (!IsVisible) return;

        if (_statusTimer > 0)
        {
            _statusTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            if (_statusTimer <= 0)
                _statusMessage = null;
        }

        if (input.IsKeyPressed(Keys.Escape))
        {
            Hide();
            return;
        }

        if (input.IsKeyPressed(Keys.Up) || input.IsKeyPressed(Keys.W) || input.ScrollDelta > 0)
        {
            _selectedIndex = (_selectedIndex - 1 + SaveManager.MaxSlots) % SaveManager.MaxSlots;
        }

        if (input.IsKeyPressed(Keys.Down) || input.IsKeyPressed(Keys.S) || input.ScrollDelta < 0)
        {
            _selectedIndex = (_selectedIndex + 1) % SaveManager.MaxSlots;
        }

        if (input.IsKeyPressed(Keys.Enter))
        {
            var slotIndex = _selectedIndex;
            var slot = _slots[slotIndex];

            if (_isSaveMode)
            {
                OnSlotConfirmed?.Invoke(slot.Slot);
                _slots = SaveManager.GetSlotSummaries();
                _statusMessage = "Saved!";
                _statusTimer = 2.0;
            }
            else
            {
                // Load mode — only load occupied slots
                if (slot.Exists)
                {
                    OnSlotConfirmed?.Invoke(slot.Slot);
                }
            }
        }

        // X key to delete a save
        if (input.IsKeyPressed(Keys.X))
        {
            var slot = _slots[_selectedIndex];
            if (slot.Exists)
            {
                SaveManager.DeleteSlot(slot.Slot);
                _slots = SaveManager.GetSlotSummaries();
                _statusMessage = "Deleted.";
                _statusTimer = 2.0;
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        DrawPanel(spriteBatch, new Color(10, 10, 20, 230), new Color(120, 100, 60));

        var x = Bounds.X + Bounds.Width / 2;
        var y = Bounds.Y + 20;

        // Title
        var title = _isSaveMode ? "SAVE GAME" : "LOAD GAME";
        var titleSize = Font.MeasureString(title);
        spriteBatch.DrawString(Font, title, new Vector2(x - titleSize.X / 2, y), Color.Gold);
        y += 50;

        // Slot entries
        for (int i = 0; i < SaveManager.MaxSlots; i++)
        {
            var slot = _slots[i];
            var selected = i == _selectedIndex;
            var color = selected ? Color.Yellow : Color.White;
            var prefix = selected ? "> " : "  ";

            string text;
            if (slot.Exists)
            {
                var localTime = slot.SaveTime.ToLocalTime();
                text = $"{prefix}Slot {slot.Slot}: {slot.SaveName} - {localTime:yyyy-MM-dd HH:mm}";
            }
            else
            {
                text = $"{prefix}Slot {slot.Slot}: (empty)";
                if (!_isSaveMode)
                    color = selected ? Color.DarkGoldenrod : Color.DarkGray;
            }

            var textSize = Font.MeasureString(text);
            spriteBatch.DrawString(Font, text, new Vector2(x - textSize.X / 2, y), color);
            y += 34;
        }

        // Status message
        if (_statusMessage != null)
        {
            y += 10;
            var msgSize = Font.MeasureString(_statusMessage);
            spriteBatch.DrawString(Font, _statusMessage, new Vector2(x - msgSize.X / 2, y), Color.LimeGreen);
            y += 25;
        }

        // Hints
        y = Bounds.Bottom - 34;
        var hint = "[Enter] Select  [X] Delete  [Esc] Cancel";
        var hintSize = Font.MeasureString(hint);
        var hintX = Math.Max(Bounds.X + 14, x - hintSize.X / 2);
        spriteBatch.DrawString(Font, hint, new Vector2(hintX, y), Color.Gray);
    }
}
