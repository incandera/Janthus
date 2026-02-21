using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Janthus.Model.Entities;
using Janthus.Model.Services;
using Janthus.Game.Combat;
using Janthus.Game.Input;

namespace Janthus.Game.UI;

public class UIManager
{
    private readonly HudPanel _hud;
    private readonly CharacterPanel _characterPanel;
    private readonly PauseMenuPanel _pauseMenuPanel;
    private readonly DialogPanel _dialogPanel;
    private readonly ContextMenuPanel _contextMenu;
    private readonly TradePanel _tradePanel;
    private readonly InventoryPanel _inventoryPanel;
    private readonly CombatLogPanel _combatLogPanel;
    private readonly SaveLoadPanel _saveLoadPanel;

    public bool ResumeRequested
    {
        get => _pauseMenuPanel.ResumeRequested;
        set => _pauseMenuPanel.ResumeRequested = value;
    }

    public bool SaveRequested
    {
        get => _pauseMenuPanel.SaveRequested;
        set => _pauseMenuPanel.SaveRequested = value;
    }

    public bool LoadRequested
    {
        get => _pauseMenuPanel.LoadRequested;
        set => _pauseMenuPanel.LoadRequested = value;
    }

    public bool QuitRequested => _pauseMenuPanel.QuitRequested;
    public bool IsContextMenuVisible => _contextMenu.IsVisible;
    public bool ContextMenuConsumedInput => _contextMenu.ConsumedInput;
    public bool IsDialogVisible => _dialogPanel.IsVisible;
    public bool IsTradeVisible => _tradePanel.IsVisible;
    public bool IsInventoryVisible => _inventoryPanel.IsVisible;
    public bool IsSaveLoadVisible => _saveLoadPanel.IsVisible;
    public bool IsAnyMenuVisible => _pauseMenuPanel.IsVisible || _contextMenu.IsVisible ||
                                    _dialogPanel.IsVisible || _tradePanel.IsVisible ||
                                    _inventoryPanel.IsVisible || _saveLoadPanel.IsVisible;

    public Action<int> OnSaveSlot { get; set; }
    public Action<int> OnLoadSlot { get; set; }

    public UIManager(Texture2D pixelTexture, SpriteFont font, PlayerCharacter player,
                     Viewport viewport, CombatManager combatManager)
    {
        _hud = new HudPanel(pixelTexture, font, player,
            new Rectangle(10, 10, 350, 100));

        _characterPanel = new CharacterPanel(pixelTexture, font, player,
            new Rectangle(viewport.Width - 360, 10, 350, 460));

        _pauseMenuPanel = new PauseMenuPanel(pixelTexture, font,
            new Rectangle(viewport.Width / 2 - 120, viewport.Height / 2 - 100, 240, 200));

        _dialogPanel = new DialogPanel(pixelTexture, font,
            new Rectangle(50, viewport.Height - 170, viewport.Width - 100, 150));

        _contextMenu = new ContextMenuPanel(pixelTexture, font, viewport);

        _tradePanel = new TradePanel(pixelTexture, font,
            new Rectangle(viewport.Width / 2 - 350, viewport.Height / 2 - 250, 700, 500));

        _inventoryPanel = new InventoryPanel(pixelTexture, font, player,
            new Rectangle(10, 120, 350, 500));

        _combatLogPanel = new CombatLogPanel(pixelTexture, font, combatManager,
            new Rectangle(10, viewport.Height - 180, 350, 150));

        _saveLoadPanel = new SaveLoadPanel(pixelTexture, font,
            new Rectangle(viewport.Width / 2 - 200, viewport.Height / 2 - 150, 400, 300));
    }

    public void UpdateLayout(Viewport viewport)
    {
        _characterPanel.Bounds = new Rectangle(viewport.Width - 360, 10, 350, 460);
        _pauseMenuPanel.Bounds = new Rectangle(viewport.Width / 2 - 120, viewport.Height / 2 - 100, 240, 200);
        _dialogPanel.Bounds = new Rectangle(50, viewport.Height - 170, viewport.Width - 100, 150);
        _contextMenu.UpdateViewport(viewport);
        _tradePanel.Bounds = new Rectangle(viewport.Width / 2 - 350, viewport.Height / 2 - 250, 700, 500);
        _combatLogPanel.Bounds = new Rectangle(10, viewport.Height - 180, 350, 150);
        _saveLoadPanel.Bounds = new Rectangle(viewport.Width / 2 - 200, viewport.Height / 2 - 150, 400, 300);
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

    public void ShowDialog(string speakerName, string text, List<string> responses,
                           Action<int> onSelect, bool isEndNode = false, Action onDismiss = null)
    {
        _dialogPanel.Show(speakerName, text, responses, onSelect, isEndNode, onDismiss);
    }

    public void HideDialog() => _dialogPanel.Hide();

    public void ShowTrade(PlayerCharacter player, NonPlayerCharacter merchant,
                          List<InventoryItem> merchantInventory, IGameDataProvider dataProvider)
    {
        _tradePanel.Show(player, merchant, merchantInventory, dataProvider);
    }

    public void ShowLoot(PlayerCharacter player, NonPlayerCharacter corpse,
                         List<InventoryItem> corpseInventory)
    {
        _tradePanel.ShowLoot(player, corpse, corpseInventory);
    }

    public void HideTrade() => _tradePanel.Hide();

    public void ToggleInventory() => _inventoryPanel.IsVisible = !_inventoryPanel.IsVisible;

    public void HideInventory() => _inventoryPanel.IsVisible = false;

    public void ShowSavePanel()
    {
        _saveLoadPanel.Show(isSaveMode: true);
        _saveLoadPanel.OnSlotConfirmed = slot =>
        {
            OnSaveSlot?.Invoke(slot);
        };
    }

    public void ShowLoadPanel()
    {
        _saveLoadPanel.Show(isSaveMode: false);
        _saveLoadPanel.OnSlotConfirmed = slot =>
        {
            OnLoadSlot?.Invoke(slot);
        };
    }

    public void HideSaveLoadPanel() => _saveLoadPanel.Hide();

    public void Update(GameTime gameTime, InputManager input)
    {
        _hud.Update(gameTime, input);
        _characterPanel.Update(gameTime, input);
        _pauseMenuPanel.Update(gameTime, input);
        _dialogPanel.Update(gameTime, input);
        _contextMenu.Update(gameTime, input);
        _tradePanel.Update(gameTime, input);
        _inventoryPanel.Update(gameTime, input);
        _combatLogPanel.Update(gameTime, input);
        _saveLoadPanel.Update(gameTime, input);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _hud.Draw(spriteBatch);
        _characterPanel.Draw(spriteBatch);
        _pauseMenuPanel.Draw(spriteBatch);
        _dialogPanel.Draw(spriteBatch);
        _contextMenu.Draw(spriteBatch);
        _tradePanel.Draw(spriteBatch);
        _inventoryPanel.Draw(spriteBatch);
        _combatLogPanel.Draw(spriteBatch);
        _saveLoadPanel.Draw(spriteBatch);
    }
}
