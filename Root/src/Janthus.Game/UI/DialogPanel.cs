using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Janthus.Game.Input;

namespace Janthus.Game.UI;

public class DialogPanel : UIPanel
{
    private string _speakerName = string.Empty;
    private string _dialogText = string.Empty;
    private readonly List<string> _responses = new();
    private int _selectedResponse;

    public DialogPanel(Texture2D pixelTexture, SpriteFont font, Rectangle bounds)
        : base(pixelTexture, font, bounds)
    {
        IsVisible = false;
    }

    public void Show(string speakerName, string text, List<string> responses)
    {
        _speakerName = speakerName;
        _dialogText = text;
        _responses.Clear();
        _responses.AddRange(responses);
        _selectedResponse = 0;
        IsVisible = true;
    }

    public void Hide()
    {
        IsVisible = false;
    }

    public override void Update(GameTime gameTime, InputManager input)
    {
        if (!IsVisible || _responses.Count == 0) return;

        if (input.IsKeyPressed(Keys.Up) || input.ScrollDelta > 0)
            _selectedResponse = (_selectedResponse - 1 + _responses.Count) % _responses.Count;
        if (input.IsKeyPressed(Keys.Down) || input.ScrollDelta < 0)
            _selectedResponse = (_selectedResponse + 1) % _responses.Count;
        if (input.IsKeyPressed(Keys.Enter))
            Hide();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        DrawPanel(spriteBatch, new Color(15, 15, 25, 230), new Color(80, 80, 120));

        var x = Bounds.X + 15;
        var y = Bounds.Y + 10;

        if (!string.IsNullOrEmpty(_speakerName))
        {
            spriteBatch.DrawString(Font, _speakerName, new Vector2(x, y), Color.Gold);
            y += 25;
        }

        spriteBatch.DrawString(Font, _dialogText, new Vector2(x, y), Color.White);
        y += 30;

        for (int i = 0; i < _responses.Count; i++)
        {
            var color = i == _selectedResponse ? Color.Yellow : Color.LightGray;
            var prefix = i == _selectedResponse ? "> " : "  ";
            spriteBatch.DrawString(Font, prefix + _responses[i], new Vector2(x, y), color);
            y += 22;
        }
    }
}
