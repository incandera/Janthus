using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Janthus.Model.Entities;
using Janthus.Game.Input;

namespace Janthus.Game.UI;

public class UIManager
{
    private readonly HudPanel _hud;
    private readonly CharacterPanel _characterPanel;
    private readonly PauseMenuPanel _pauseMenuPanel;
    private readonly DialogPanel _dialogPanel;
    private readonly ContextMenuPanel _contextMenu;

    public bool ResumeRequested
    {
        get => _pauseMenuPanel.ResumeRequested;
        set => _pauseMenuPanel.ResumeRequested = value;
    }

    public bool QuitRequested => _pauseMenuPanel.QuitRequested;
    public bool IsContextMenuVisible => _contextMenu.IsVisible;
    public bool ContextMenuConsumedInput => _contextMenu.ConsumedInput;

    public UIManager(Texture2D pixelTexture, SpriteFont font, PlayerCharacter player, Viewport viewport)
    {
        _hud = new HudPanel(pixelTexture, font, player,
            new Rectangle(10, 10, 300, 80));

        _characterPanel = new CharacterPanel(pixelTexture, font, player,
            new Rectangle(viewport.Width - 260, 10, 250, 420));

        _pauseMenuPanel = new PauseMenuPanel(pixelTexture, font,
            new Rectangle(viewport.Width / 2 - 120, viewport.Height / 2 - 100, 240, 200));

        _dialogPanel = new DialogPanel(pixelTexture, font,
            new Rectangle(50, viewport.Height - 170, viewport.Width - 100, 150));

        _contextMenu = new ContextMenuPanel(pixelTexture, font, viewport);
    }

    public void UpdateLayout(Viewport viewport)
    {
        _characterPanel.Bounds = new Rectangle(viewport.Width - 260, 10, 250, 420);
        _pauseMenuPanel.Bounds = new Rectangle(viewport.Width / 2 - 120, viewport.Height / 2 - 100, 240, 200);
        _dialogPanel.Bounds = new Rectangle(50, viewport.Height - 170, viewport.Width - 100, 150);
        _contextMenu.UpdateViewport(viewport);
    }

    public void SetPaused(bool paused) => _hud.SetPaused(paused);

    public void ToggleCharacterPanel() => _characterPanel.IsVisible = !_characterPanel.IsVisible;

    public void TogglePauseMenu()
    {
        _pauseMenuPanel.IsVisible = !_pauseMenuPanel.IsVisible;
    }

    public void ClosePauseMenu() => _pauseMenuPanel.IsVisible = false;

    public void ShowContextMenu(Point screenPos, List<string> items, Action<int> onSelect)
    {
        _contextMenu.Show(screenPos, items, onSelect);
    }

    public void CloseContextMenu() => _contextMenu.Close();

    public void Update(GameTime gameTime, InputManager input)
    {
        _hud.Update(gameTime, input);
        _characterPanel.Update(gameTime, input);
        _pauseMenuPanel.Update(gameTime, input);
        _dialogPanel.Update(gameTime, input);
        _contextMenu.Update(gameTime, input);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _hud.Draw(spriteBatch);
        _characterPanel.Draw(spriteBatch);
        _pauseMenuPanel.Draw(spriteBatch);
        _dialogPanel.Draw(spriteBatch);
        _contextMenu.Draw(spriteBatch);
    }
}
