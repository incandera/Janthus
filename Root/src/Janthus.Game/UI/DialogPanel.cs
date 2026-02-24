using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FontStashSharp;
using Janthus.Game.Input;

namespace Janthus.Game.UI;

public class DialogPanel : UIPanel
{
    private string _speakerName = string.Empty;
    private List<string> _wrappedLines = new();
    private readonly List<string> _responses = new();
    private int _selectedResponse;
    private Action<int> _onSelect;
    private Action _onDismiss;
    private bool _isEndNode;

    private bool _consumedInput;

    public bool ConsumedInput => _consumedInput;

    private const int PaddingX = 15;
    private const int PaddingTop = 10;
    private const int LineHeight = 18;
    private const int ResponseLineHeight = 22;

    public DialogPanel(Texture2D pixelTexture, SpriteFontBase font, Rectangle bounds)
        : base(pixelTexture, font, bounds)
    {
        IsVisible = false;
    }

    public void Show(string speakerName, string text, List<string> responses,
                     Action<int> onSelect, bool isEndNode = false, Action onDismiss = null)
    {
        _speakerName = speakerName;
        _responses.Clear();
        _responses.AddRange(responses);
        _selectedResponse = 0;
        _onSelect = onSelect;
        _onDismiss = onDismiss;
        _isEndNode = isEndNode;
        IsVisible = true;

        _wrappedLines = WrapText(text, Bounds.Width - PaddingX * 2);
    }

    public void Hide()
    {
        IsVisible = false;
        _onSelect = null;
        _onDismiss = null;
    }

    public override void Update(GameTime gameTime, InputManager input)
    {
        _consumedInput = false;
        if (!IsVisible) return;

        if (_isEndNode || _responses.Count == 0)
        {
            if (input.IsKeyPressed(Keys.Enter) || input.IsLeftClickPressed())
            {
                _consumedInput = true;
                var dismiss = _onDismiss;
                Hide();
                dismiss?.Invoke();
            }
            return;
        }

        if (input.IsKeyPressed(Keys.Up) || input.ScrollDelta > 0)
            _selectedResponse = (_selectedResponse - 1 + _responses.Count) % _responses.Count;
        if (input.IsKeyPressed(Keys.Down) || input.ScrollDelta < 0)
            _selectedResponse = (_selectedResponse + 1) % _responses.Count;

        if (input.IsKeyPressed(Keys.Enter))
        {
            var callback = _onSelect;
            var index = _selectedResponse;
            Hide();
            callback?.Invoke(index);
            return;
        }

        if (input.IsLeftClickPressed() && Bounds.Contains(input.MousePosition))
        {
            _consumedInput = true;
            var responseAreaY = GetResponseAreaY();
            var localY = input.MousePosition.Y - responseAreaY;
            var clickedIndex = localY / ResponseLineHeight;
            if (clickedIndex >= 0 && clickedIndex < _responses.Count)
            {
                var callback = _onSelect;
                Hide();
                callback?.Invoke(clickedIndex);
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        DrawPanel(spriteBatch, new Color(15, 15, 25, 230), new Color(80, 80, 120));

        var x = Bounds.X + PaddingX;
        var y = Bounds.Y + PaddingTop;

        if (!string.IsNullOrEmpty(_speakerName))
        {
            spriteBatch.DrawString(Font, _speakerName, new Vector2(x, y), Color.Gold);
            y += LineHeight + 4;
        }

        foreach (var line in _wrappedLines)
        {
            spriteBatch.DrawString(Font, line, new Vector2(x, y), Color.White);
            y += LineHeight;
        }

        y += 8;

        // Separator line
        spriteBatch.Draw(PixelTexture,
            new Rectangle(x, y, Bounds.Width - PaddingX * 2, 1),
            new Color(80, 80, 120));
        y += 10;

        if (_isEndNode || _responses.Count == 0)
        {
            spriteBatch.DrawString(Font, "[Press Enter to continue]", new Vector2(x, y), Color.Gray);
        }
        else
        {
            for (int i = 0; i < _responses.Count; i++)
            {
                var color = i == _selectedResponse ? Color.Yellow : Color.LightGray;
                var prefix = i == _selectedResponse ? "> " : "  ";
                spriteBatch.DrawString(Font, prefix + _responses[i], new Vector2(x, y), color);
                y += ResponseLineHeight;
            }
        }
    }

    private int GetResponseAreaY()
    {
        var y = Bounds.Y + PaddingTop;
        if (!string.IsNullOrEmpty(_speakerName))
            y += LineHeight + 4;
        y += _wrappedLines.Count * LineHeight;
        y += 8 + 10; // separator gap
        return y;
    }

    private List<string> WrapText(string text, int maxWidth)
    {
        var lines = new List<string>();
        if (string.IsNullOrEmpty(text)) return lines;

        var words = text.Split(' ');
        var currentLine = string.Empty;

        foreach (var word in words)
        {
            var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
            var size = Font.MeasureString(testLine);
            if (size.X > maxWidth && !string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
                currentLine = word;
            }
            else
            {
                currentLine = testLine;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
            lines.Add(currentLine);

        return lines;
    }
}
