using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FontStashSharp;
using Janthus.Model.Entities;
using Janthus.Model.Services;
using Janthus.Game.Input;

namespace Janthus.Game.UI;

public class TradePanel : UIPanel
{
    private PlayerCharacter _player;
    private NonPlayerCharacter _merchant;
    private List<InventoryItem> _merchantInventory;
    private IGameDataProvider _dataProvider;

    private bool _playerSideActive; // true = player (sell), false = merchant (buy)
    private int _selectedIndex;
    private int _playerScrollOffset;
    private int _merchantScrollOffset;
    private string _statusMessage = string.Empty;
    private double _statusTimer;
    private bool _lootMode; // true = looting a corpse (free take, no sell)

    private const int PaddingX = 18;
    private const int PaddingTop = 14;
    private const int LineHeight = 24;
    private const int ItemLineHeight = 26;
    private const int HeaderHeight = 36;
    private const int DescriptionHeight = 60;
    private const int BottomBarHeight = 60;

    public TradePanel(Texture2D pixelTexture, SpriteFontBase font, Rectangle bounds)
        : base(pixelTexture, font, bounds)
    {
        IsVisible = false;
    }

    public void Show(PlayerCharacter player, NonPlayerCharacter merchant,
                     List<InventoryItem> merchantInventory, IGameDataProvider dataProvider)
    {
        _player = player;
        _merchant = merchant;
        _merchantInventory = merchantInventory;
        _dataProvider = dataProvider;
        _playerSideActive = false; // Start on merchant side (buy)
        _selectedIndex = 0;
        _playerScrollOffset = 0;
        _merchantScrollOffset = 0;
        _statusMessage = string.Empty;
        _statusTimer = 0;
        _lootMode = false;
        IsVisible = true;
    }

    public void ShowLoot(PlayerCharacter player, NonPlayerCharacter corpse,
                         List<InventoryItem> corpseInventory)
    {
        _player = player;
        _merchant = corpse;
        _merchantInventory = corpseInventory;
        _dataProvider = null;
        _playerSideActive = false; // Start on corpse side (take)
        _selectedIndex = 0;
        _playerScrollOffset = 0;
        _merchantScrollOffset = 0;
        _statusMessage = string.Empty;
        _statusTimer = 0;
        _lootMode = true;
        IsVisible = true;
    }

    public void Hide()
    {
        IsVisible = false;
        _player = null;
        _merchant = null;
        _merchantInventory = null;
        _dataProvider = null;
        _lootMode = false;
    }

    public override void Update(GameTime gameTime, InputManager input)
    {
        if (!IsVisible || _player == null) return;

        if (_statusTimer > 0)
        {
            _statusTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            if (_statusTimer <= 0)
                _statusMessage = string.Empty;
        }

        // Tab switches sides
        if (input.IsKeyPressed(Keys.Tab))
        {
            _playerSideActive = !_playerSideActive;
            _selectedIndex = 0;
        }

        var activeList = _playerSideActive ? _player.Inventory : _merchantInventory;
        if (activeList == null || activeList.Count == 0) return;

        // Navigate
        if (input.IsKeyPressed(Keys.Up) || input.ScrollDelta > 0)
            _selectedIndex = (_selectedIndex - 1 + activeList.Count) % activeList.Count;
        if (input.IsKeyPressed(Keys.Down) || input.ScrollDelta < 0)
            _selectedIndex = (_selectedIndex + 1) % activeList.Count;

        // Auto-scroll to keep selected item visible
        EnsureSelectedVisible();

        // Enter to buy/sell/take
        if (input.IsKeyPressed(Keys.Enter))
        {
            if (_lootMode)
            {
                if (_playerSideActive)
                    TryReturnToCorpse();
                else
                    TryLoot();
            }
            else
            {
                if (_playerSideActive)
                    TrySell();
                else
                    TryBuy();
            }
        }

        // Mouse click handling
        if (input.IsLeftClickPressed() && Bounds.Contains(input.MousePosition))
        {
            var columnWidth = (Bounds.Width - PaddingX * 3) / 2;
            var leftColumnX = Bounds.X + PaddingX;
            var rightColumnX = Bounds.X + PaddingX * 2 + columnWidth;
            var itemsY = Bounds.Y + PaddingTop + HeaderHeight + LineHeight + 5;

            var mouseX = input.MousePosition.X;
            var mouseY = input.MousePosition.Y;

            // Determine which side was clicked
            if (mouseX >= leftColumnX && mouseX < leftColumnX + columnWidth)
            {
                _playerSideActive = true;
                var localY = mouseY - itemsY;
                if (localY >= 0)
                {
                    var clickedIndex = localY / ItemLineHeight + _playerScrollOffset;
                    if (clickedIndex >= 0 && clickedIndex < _player.Inventory.Count)
                        _selectedIndex = clickedIndex;
                }
            }
            else if (mouseX >= rightColumnX && mouseX < rightColumnX + columnWidth)
            {
                _playerSideActive = false;
                var localY = mouseY - itemsY;
                if (localY >= 0)
                {
                    var clickedIndex = localY / ItemLineHeight + _merchantScrollOffset;
                    if (clickedIndex >= 0 && clickedIndex < _merchantInventory.Count)
                        _selectedIndex = clickedIndex;
                }
            }
        }
    }

    private void TryBuy()
    {
        if (_merchantInventory == null || _selectedIndex >= _merchantInventory.Count) return;

        var stock = _merchantInventory[_selectedIndex];
        var price = TradeCalculator.CalculateBuyPrice(
            stock.Item, GetMerchantPriceMultiplier(stock.Item),
            _player.Alignment, _merchant.Alignment,
            _player.Skills, _dataProvider);

        if (_player.Gold < price)
        {
            SetStatus("Not enough gold!");
            return;
        }

        _player.Gold -= price;
        _merchant.Gold += price;

        // Add to player inventory
        var existing = _player.Inventory.Find(i => i.Item.Id == stock.Item.Id);
        if (existing != null)
            existing.Quantity++;
        else
            _player.Inventory.Add(new InventoryItem(stock.Item));

        // Remove from merchant inventory
        stock.Quantity--;
        if (stock.Quantity <= 0)
        {
            _merchantInventory.RemoveAt(_selectedIndex);
            if (_selectedIndex >= _merchantInventory.Count && _merchantInventory.Count > 0)
                _selectedIndex = _merchantInventory.Count - 1;
        }

        SetStatus($"Bought {stock.Item.Name} for {price}g");
    }

    private void TrySell()
    {
        if (_player.Inventory == null || _selectedIndex >= _player.Inventory.Count) return;

        var inv = _player.Inventory[_selectedIndex];
        var price = TradeCalculator.CalculateSellPrice(
            inv.Item, _player.Alignment, _merchant.Alignment,
            _player.Skills, _dataProvider);

        if (_merchant.Gold < price)
        {
            SetStatus("Merchant can't afford that!");
            return;
        }

        _player.Gold += price;
        _merchant.Gold -= price;

        // Add to merchant inventory
        var existing = _merchantInventory.Find(i => i.Item.Id == inv.Item.Id);
        if (existing != null)
            existing.Quantity++;
        else
            _merchantInventory.Add(new InventoryItem(inv.Item));

        // Remove from player inventory
        inv.Quantity--;
        if (inv.Quantity <= 0)
        {
            _player.Inventory.RemoveAt(_selectedIndex);
            if (_selectedIndex >= _player.Inventory.Count && _player.Inventory.Count > 0)
                _selectedIndex = _player.Inventory.Count - 1;
        }

        SetStatus($"Sold {inv.Item.Name} for {price}g");
    }

    private void TryLoot()
    {
        if (_merchantInventory == null || _selectedIndex >= _merchantInventory.Count) return;

        var stock = _merchantInventory[_selectedIndex];
        var itemName = stock.Item.Name;

        // Add to player inventory
        var existing = _player.Inventory.Find(i => i.Item.Id == stock.Item.Id);
        if (existing != null)
            existing.Quantity++;
        else
            _player.Inventory.Add(new InventoryItem(stock.Item));

        // Remove from corpse inventory
        stock.Quantity--;
        if (stock.Quantity <= 0)
        {
            _merchantInventory.RemoveAt(_selectedIndex);
            if (_selectedIndex >= _merchantInventory.Count && _merchantInventory.Count > 0)
                _selectedIndex = _merchantInventory.Count - 1;
        }

        SetStatus($"Took {itemName}");
    }

    private void TryReturnToCorpse()
    {
        if (_player.Inventory == null || _selectedIndex >= _player.Inventory.Count) return;

        var inv = _player.Inventory[_selectedIndex];
        var itemName = inv.Item.Name;

        // Add to corpse inventory
        var existing = _merchantInventory.Find(i => i.Item.Id == inv.Item.Id);
        if (existing != null)
            existing.Quantity++;
        else
            _merchantInventory.Add(new InventoryItem(inv.Item));

        // Remove from player inventory
        inv.Quantity--;
        if (inv.Quantity <= 0)
        {
            _player.Inventory.RemoveAt(_selectedIndex);
            if (_selectedIndex >= _player.Inventory.Count && _player.Inventory.Count > 0)
                _selectedIndex = _player.Inventory.Count - 1;
        }

        SetStatus($"Returned {itemName}");
    }

    private int GetMaxVisibleItems()
    {
        var itemsStartY = Bounds.Y + PaddingTop + HeaderHeight + LineHeight + 5;
        var columnsBottomY = Bounds.Bottom - DescriptionHeight - BottomBarHeight;
        return (columnsBottomY - itemsStartY) / ItemLineHeight;
    }

    private void EnsureSelectedVisible()
    {
        var maxVisible = GetMaxVisibleItems();
        if (maxVisible <= 0) return;

        ref int scrollOffset = ref (_playerSideActive ? ref _playerScrollOffset : ref _merchantScrollOffset);

        if (_selectedIndex < scrollOffset)
            scrollOffset = _selectedIndex;
        else if (_selectedIndex >= scrollOffset + maxVisible)
            scrollOffset = _selectedIndex - maxVisible + 1;
    }

    private decimal GetMerchantPriceMultiplier(Item item)
    {
        if (_merchant == null || _dataProvider == null) return 1.0m;
        var stock = _dataProvider.GetMerchantStock(_merchant.Name);
        var entry = stock.Find(s => s.ItemId == item.Id);
        return entry?.PriceMultiplier ?? 1.0m;
    }

    private void SetStatus(string message)
    {
        _statusMessage = message;
        _statusTimer = 3.0;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible || _player == null) return;

        DrawPanel(spriteBatch, new Color(15, 15, 25, 235), new Color(100, 80, 60));

        var columnWidth = (Bounds.Width - PaddingX * 3) / 2;
        var leftX = Bounds.X + PaddingX;
        var rightX = Bounds.X + PaddingX * 2 + columnWidth;
        var y = Bounds.Y + PaddingTop;

        // Title
        var title = _lootMode
            ? $"Loot: {_merchant?.Name ?? "Corpse"}"
            : $"Trade with {_merchant?.Name ?? "Merchant"}";
        var titleSize = Font.MeasureString(title);
        spriteBatch.DrawString(Font, title,
            new Vector2(Bounds.X + (Bounds.Width - titleSize.X) / 2, y), _lootMode ? Color.OrangeRed : Color.Gold);
        y += HeaderHeight;

        // Column headers
        var playerHeaderColor = _playerSideActive ? Color.Yellow : Color.LightGray;
        var merchantHeaderColor = _playerSideActive ? Color.LightGray : Color.Yellow;
        spriteBatch.DrawString(Font, $"Your Items ({_player.Gold:F0}g)", new Vector2(leftX, y), playerHeaderColor);
        if (_lootMode)
            spriteBatch.DrawString(Font, $"{_merchant?.Name ?? "Corpse"}'s Items", new Vector2(rightX, y), merchantHeaderColor);
        else
            spriteBatch.DrawString(Font, $"Merchant ({_merchant?.Gold:F0}g)", new Vector2(rightX, y), merchantHeaderColor);
        y += LineHeight + 5;

        // Draw player inventory (left column)
        var itemsStartY = y;
        var columnsBottomY = Bounds.Bottom - DescriptionHeight - BottomBarHeight;
        var maxItems = (columnsBottomY - itemsStartY) / ItemLineHeight;
        DrawItemList(spriteBatch, _player.Inventory, leftX, columnWidth, itemsStartY, maxItems,
            _playerSideActive, true, _lootMode, _playerScrollOffset);

        // Draw merchant/corpse inventory (right column)
        DrawItemList(spriteBatch, _merchantInventory, rightX, columnWidth, itemsStartY, maxItems,
            !_playerSideActive, false, _lootMode, _merchantScrollOffset);

        // Vertical divider (between columns only, not into description area)
        var dividerX = Bounds.X + PaddingX + columnWidth + PaddingX / 2;
        spriteBatch.Draw(PixelTexture,
            new Rectangle(dividerX, Bounds.Y + PaddingTop + HeaderHeight - 5, 1, columnsBottomY - (Bounds.Y + PaddingTop + HeaderHeight - 5)),
            new Color(80, 80, 100));

        // Separator above description
        spriteBatch.Draw(PixelTexture,
            new Rectangle(Bounds.X + PaddingX, columnsBottomY, Bounds.Width - PaddingX * 2, 1),
            new Color(80, 80, 100));

        // Full-width item description (word-wrapped)
        var descY = columnsBottomY + 6;
        var descMaxWidth = Bounds.Width - PaddingX * 2;
        var activeList = _playerSideActive ? _player.Inventory : _merchantInventory;
        if (activeList != null && _selectedIndex >= 0 && _selectedIndex < activeList.Count)
        {
            var desc = activeList[_selectedIndex].Item.Description ?? string.Empty;
            var lines = WrapText(desc, descMaxWidth);
            foreach (var line in lines)
            {
                if (descY + LineHeight > Bounds.Bottom - BottomBarHeight) break;
                spriteBatch.DrawString(Font, line, new Vector2(leftX, descY), Color.Gray);
                descY += LineHeight;
            }
        }

        // Bottom bar
        var bottomY = Bounds.Bottom - BottomBarHeight + 5;

        // Separator above bottom bar
        spriteBatch.Draw(PixelTexture,
            new Rectangle(Bounds.X + PaddingX, bottomY - 5, Bounds.Width - PaddingX * 2, 1),
            new Color(80, 80, 100));

        // Status message
        if (!string.IsNullOrEmpty(_statusMessage))
        {
            spriteBatch.DrawString(Font, _statusMessage, new Vector2(leftX, bottomY), Color.LightGreen);
        }

        // Control hints
        var hints = _lootMode
            ? "[Enter] Take/Return  [Tab] Switch  [Esc] Close"
            : "[Enter] Buy/Sell  [Tab] Switch  [Esc] Close";
        var hintsSize = Font.MeasureString(hints);
        spriteBatch.DrawString(Font, hints,
            new Vector2(Bounds.Right - PaddingX - hintsSize.X, bottomY + LineHeight), Color.DarkGray);
    }

    private void DrawItemList(SpriteBatch spriteBatch, List<InventoryItem> items,
        int x, int width, int startY, int maxItems, bool isActiveSide, bool isSelling,
        bool isLootMode, int scrollOffset)
    {
        if (items == null) return;

        var y = startY;
        var endIndex = Math.Min(items.Count, scrollOffset + maxItems);
        for (int i = scrollOffset; i < endIndex; i++)
        {
            var inv = items[i];

            // Selection highlight
            if (isActiveSide && i == _selectedIndex)
            {
                spriteBatch.Draw(PixelTexture,
                    new Rectangle(x - 2, y, width + 4, ItemLineHeight),
                    Color.White * 0.12f);
            }

            var textColor = (isActiveSide && i == _selectedIndex) ? Color.Yellow : Color.White;
            var prefix = (isActiveSide && i == _selectedIndex) ? "> " : "  ";
            var itemText = inv.Quantity > 1 ? $"{prefix}{inv.Item.Name} x{inv.Quantity}" : $"{prefix}{inv.Item.Name}";

            spriteBatch.DrawString(Font, itemText, new Vector2(x, y + 2), textColor);

            // Price on right side (skip in loot mode)
            if (!isLootMode)
            {
                decimal price;
                if (isSelling)
                {
                    price = _dataProvider != null
                        ? TradeCalculator.CalculateSellPrice(inv.Item, _player.Alignment, _merchant.Alignment, _player.Skills, _dataProvider)
                        : inv.Item.TradeValue * 0.5m;
                }
                else
                {
                    price = _dataProvider != null
                        ? TradeCalculator.CalculateBuyPrice(inv.Item, GetMerchantPriceMultiplier(inv.Item), _player.Alignment, _merchant.Alignment, _player.Skills, _dataProvider)
                        : inv.Item.TradeValue;
                }

                var priceText = $"{price}g";
                var priceSize = Font.MeasureString(priceText);
                spriteBatch.DrawString(Font, priceText,
                    new Vector2(x + width - priceSize.X, y + 2), Color.LightGoldenrodYellow);
            }

            y += ItemLineHeight;
        }

        // Scroll indicator (top) — right-aligned just left of price column, on first item line
        if (scrollOffset > 0)
        {
            var arrowText = "...";
            var arrowSize = Font.MeasureString(arrowText);
            var priceGap = Font.MeasureString("0000g").X;
            spriteBatch.DrawString(Font, arrowText,
                new Vector2(x + width - priceGap - arrowSize.X - 4, startY + 2), Color.DarkGray);
        }

        // Scroll indicator (bottom) — right-aligned just left of price column, on last item line
        if (endIndex < items.Count)
        {
            var arrowText = "...";
            var arrowSize = Font.MeasureString(arrowText);
            var priceGap = Font.MeasureString("0000g").X;
            var arrowY = startY + (maxItems - 1) * ItemLineHeight + 2;
            spriteBatch.DrawString(Font, arrowText,
                new Vector2(x + width - priceGap - arrowSize.X - 4, arrowY), Color.DarkGray);
        }

        if (items.Count == 0)
        {
            spriteBatch.DrawString(Font, "  (empty)", new Vector2(x, startY + 2), Color.DarkGray);
        }
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
