using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Janthus.Game.Input;

namespace Janthus.Game.UI;

public class ContextMenuPanel : UIPanel
{
    private List<string> _items = new();
    private Action<int> _onSelect;
    private int _selectedIndex;
    private Viewport _viewport;
    private bool _consumedInput;

    public bool ConsumedInput => _consumedInput;

    private const int ItemHeight = 22;
    private const int PaddingX = 16;
    private const int PaddingY = 8;

    public ContextMenuPanel(Texture2D pixelTexture, SpriteFont font, Viewport viewport)
        : base(pixelTexture, font, Rectangle.Empty)
    {
        IsVisible = false;
        _viewport = viewport;
    }

    public void UpdateViewport(Viewport viewport)
    {
        _viewport = viewport;
    }

    public void Show(Point screenPos, List<string> items, Action<int> onSelect)
    {
        _items = items;
        _onSelect = onSelect;
        _selectedIndex = 0;

        // Measure width based on longest item
        var maxWidth = 0;
        foreach (var item in items)
        {
            var size = Font.MeasureString(item);
            if ((int)size.X > maxWidth)
                maxWidth = (int)size.X;
        }

        var width = maxWidth + PaddingX * 2;
        var height = items.Count * ItemHeight + PaddingY * 2;

        // Clamp to viewport
        var x = screenPos.X;
        var y = screenPos.Y;
        if (x + width > _viewport.Width)
            x = _viewport.Width - width;
        if (y + height > _viewport.Height)
            y = _viewport.Height - height;
        if (x < 0) x = 0;
        if (y < 0) y = 0;

        Bounds = new Rectangle(x, y, width, height);
        IsVisible = true;
    }

    public void Close()
    {
        IsVisible = false;
        _items.Clear();
        _onSelect = null;
    }

    public override void Update(GameTime gameTime, InputManager input)
    {
        _consumedInput = false;
        if (!IsVisible) return;

        // Close on ESC
        if (input.IsKeyPressed(Keys.Escape))
        {
            Close();
            _consumedInput = true;
            return;
        }

        // Navigate with Up/Down (arrow keys and WASD)
        if (input.IsKeyPressed(Keys.Up) || input.IsKeyPressed(Keys.W))
        {
            _selectedIndex = (_selectedIndex - 1 + _items.Count) % _items.Count;
        }

        if (input.IsKeyPressed(Keys.Down) || input.IsKeyPressed(Keys.S))
        {
            _selectedIndex = (_selectedIndex + 1) % _items.Count;
        }

        // Select with Enter
        if (input.IsKeyPressed(Keys.Enter))
        {
            var callback = _onSelect;
            var index = _selectedIndex;
            Close();
            _consumedInput = true;
            callback?.Invoke(index);
            return;
        }

        // Left-click handling
        if (input.IsLeftClickPressed())
        {
            if (Bounds.Contains(input.MousePosition))
            {
                // Click on an item
                var localY = input.MousePosition.Y - Bounds.Y - PaddingY;
                var clickedIndex = localY / ItemHeight;
                if (clickedIndex >= 0 && clickedIndex < _items.Count)
                {
                    var callback = _onSelect;
                    Close();
                    _consumedInput = true;
                    callback?.Invoke(clickedIndex);
                }
            }
            else
            {
                // Click outside — close
                Close();
                _consumedInput = true;
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        DrawPanel(spriteBatch, new Color(10, 10, 20, 220), new Color(120, 100, 60));

        var y = Bounds.Y + PaddingY;
        for (int i = 0; i < _items.Count; i++)
        {
            // Highlight selected row
            if (i == _selectedIndex)
            {
                spriteBatch.Draw(PixelTexture,
                    new Rectangle(Bounds.X + 2, y, Bounds.Width - 4, ItemHeight),
                    Color.White * 0.15f);
            }

            var color = i == _selectedIndex ? Color.Yellow : Color.White;
            spriteBatch.DrawString(Font, _items[i], new Vector2(Bounds.X + PaddingX, y + 3), color);
            y += ItemHeight;
        }
    }
}
