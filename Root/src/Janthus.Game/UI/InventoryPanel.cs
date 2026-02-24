using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FontStashSharp;
using Janthus.Model.Entities;
using Janthus.Model.Enums;
using Janthus.Model.Services;
using Janthus.Game.Input;

namespace Janthus.Game.UI;

public class InventoryPanel : UIPanel
{
    private readonly PlayerCharacter _player;
    private int _selectedIndex;
    private bool _inEquipmentSection = true;
    private int _inventoryScrollOffset;
    private string _statusMessage = string.Empty;
    private double _statusTimer;

    private const int PaddingX = 18;
    private const int PaddingTop = 14;
    private const int LineHeight = 24;
    private const int ItemLineHeight = 26;
    private const int StatLineHeight = 20;
    private const int HeaderHeight = 36;
    private const int DescriptionHeight = 80;
    private const int BottomBarHeight = 60;

    private static readonly EquipmentSlot[] DisplaySlots =
    {
        EquipmentSlot.Helmet,
        EquipmentSlot.Cuirass,
        EquipmentSlot.Gauntlets,
        EquipmentSlot.Greaves,
        EquipmentSlot.Boots,
        EquipmentSlot.Weapon,
        EquipmentSlot.Accessory
    };

    public InventoryPanel(Texture2D pixelTexture, SpriteFontBase font, PlayerCharacter player, Rectangle bounds)
        : base(pixelTexture, font, bounds)
    {
        _player = player;
        IsVisible = false;
    }

    public void Show()
    {
        _selectedIndex = 0;
        _inEquipmentSection = true;
        _inventoryScrollOffset = 0;
        _statusMessage = string.Empty;
        _statusTimer = 0;
        IsVisible = true;
    }

    public void Hide()
    {
        IsVisible = false;
    }

    private int GetMaxVisibleItems()
    {
        var itemsStartY = Bounds.Y + PaddingTop + HeaderHeight + LineHeight + 5;
        var columnsBottomY = Bounds.Bottom - DescriptionHeight - BottomBarHeight;
        return (columnsBottomY - itemsStartY) / ItemLineHeight;
    }

    private void EnsureInventorySelectedVisible()
    {
        var maxVisible = GetMaxVisibleItems();
        if (maxVisible <= 0) return;

        if (_selectedIndex < _inventoryScrollOffset)
            _inventoryScrollOffset = _selectedIndex;
        else if (_selectedIndex >= _inventoryScrollOffset + maxVisible)
            _inventoryScrollOffset = _selectedIndex - maxVisible + 1;
    }

    public override void Update(GameTime gameTime, InputManager input)
    {
        if (!IsVisible) return;

        if (_statusTimer > 0)
        {
            _statusTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            if (_statusTimer <= 0)
                _statusMessage = string.Empty;
        }

        // Tab switches between equipment and inventory sections
        if (input.IsKeyPressed(Keys.Tab))
        {
            _inEquipmentSection = !_inEquipmentSection;
            _selectedIndex = 0;
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

            // Left column (equipment)
            if (mouseX >= leftColumnX && mouseX < leftColumnX + columnWidth)
            {
                _inEquipmentSection = true;
                var localY = mouseY - itemsY;
                if (localY >= 0)
                {
                    var clickedIndex = localY / ItemLineHeight;
                    if (clickedIndex >= 0 && clickedIndex < DisplaySlots.Length)
                        _selectedIndex = clickedIndex;
                }
            }
            // Right column (inventory)
            else if (mouseX >= rightColumnX && mouseX < rightColumnX + columnWidth)
            {
                _inEquipmentSection = false;
                var localY = mouseY - itemsY;
                if (localY >= 0)
                {
                    var clickedIndex = localY / ItemLineHeight + _inventoryScrollOffset;
                    if (clickedIndex >= 0 && clickedIndex < _player.Inventory.Count)
                        _selectedIndex = clickedIndex;
                }
            }
        }

        if (_inEquipmentSection)
        {
            UpdateEquipmentSection(input);
        }
        else
        {
            UpdateInventorySection(input);
        }
    }

    private void UpdateEquipmentSection(InputManager input)
    {
        if (input.IsKeyPressed(Keys.Up) || input.ScrollDelta > 0)
            _selectedIndex = (_selectedIndex - 1 + DisplaySlots.Length) % DisplaySlots.Length;
        if (input.IsKeyPressed(Keys.Down) || input.ScrollDelta < 0)
            _selectedIndex = (_selectedIndex + 1) % DisplaySlots.Length;

        // E or Enter to unequip
        if (input.IsKeyPressed(Keys.E) || input.IsKeyPressed(Keys.Enter))
        {
            var slot = DisplaySlots[_selectedIndex];
            if (_player.Equipment.ContainsKey(slot))
            {
                CombatCalculator.Unequip(_player, _player.Inventory, slot);
                _statusMessage = $"Unequipped {slot}";
                _statusTimer = 2.5;
            }
        }
    }

    private void UpdateInventorySection(InputManager input)
    {
        if (_player.Inventory.Count == 0) return;

        if (input.IsKeyPressed(Keys.Up) || input.ScrollDelta > 0)
            _selectedIndex = (_selectedIndex - 1 + _player.Inventory.Count) % _player.Inventory.Count;
        if (input.IsKeyPressed(Keys.Down) || input.ScrollDelta < 0)
            _selectedIndex = (_selectedIndex + 1) % _player.Inventory.Count;

        EnsureInventorySelectedVisible();

        // E or Enter to equip or consume
        if (input.IsKeyPressed(Keys.E) || input.IsKeyPressed(Keys.Enter))
        {
            if (_selectedIndex >= 0 && _selectedIndex < _player.Inventory.Count)
            {
                var item = _player.Inventory[_selectedIndex].Item;
                if (item.Slot != EquipmentSlot.None)
                {
                    CombatCalculator.Equip(_player, _player.Inventory, item);
                    _statusMessage = $"Equipped {item.Name}";
                    _statusTimer = 2.5;
                    if (_selectedIndex >= _player.Inventory.Count)
                        _selectedIndex = Math.Max(0, _player.Inventory.Count - 1);
                }
                else
                {
                    var result = CombatCalculator.TryConsumeItem(_player, _player.Inventory, item);
                    if (result != null)
                    {
                        _statusMessage = result;
                        _statusTimer = 2.5;
                        if (_selectedIndex >= _player.Inventory.Count)
                            _selectedIndex = Math.Max(0, _player.Inventory.Count - 1);
                    }
                }
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        DrawPanel(spriteBatch, new Color(15, 15, 25, 235), new Color(100, 80, 60));

        var columnWidth = (Bounds.Width - PaddingX * 3) / 2;
        var leftX = Bounds.X + PaddingX;
        var rightX = Bounds.X + PaddingX * 2 + columnWidth;
        var y = Bounds.Y + PaddingTop;

        // Title with gold
        var title = "INVENTORY";
        var titleSize = Font.MeasureString(title);
        spriteBatch.DrawString(Font, title,
            new Vector2(Bounds.X + (Bounds.Width - titleSize.X) / 2, y), Color.Gold);
        var goldText = $"{_player.Gold:F0}g";
        var goldSize = Font.MeasureString(goldText);
        spriteBatch.DrawString(Font, goldText,
            new Vector2(Bounds.Right - PaddingX - goldSize.X, y), Color.Yellow);
        y += HeaderHeight;

        // Column headers with ATK/ARM summary
        var equipHeaderColor = _inEquipmentSection ? Color.Yellow : Color.LightGray;
        var itemsHeaderColor = _inEquipmentSection ? Color.LightGray : Color.Yellow;
        var baseAttack = _player.EffectiveStrength * 1.5m + _player.TotalEquipmentAttackRating;
        var baseArmor = _player.EffectiveConstitution * 0.5m + _player.TotalEquipmentArmorRating;
        spriteBatch.DrawString(Font, $"Equipment ATK:{baseAttack:F0} ARM:{baseArmor:F0}",
            new Vector2(leftX, y), equipHeaderColor);
        spriteBatch.DrawString(Font, "Items", new Vector2(rightX, y), itemsHeaderColor);
        y += LineHeight + 5;

        var itemsStartY = y;
        var columnsBottomY = Bounds.Bottom - DescriptionHeight - BottomBarHeight;
        var maxItems = GetMaxVisibleItems();

        // Draw equipment column (left)
        DrawEquipmentColumn(spriteBatch, leftX, columnWidth, itemsStartY, columnsBottomY);

        // Draw inventory column (right)
        DrawInventoryColumn(spriteBatch, rightX, columnWidth, itemsStartY, maxItems);

        // Vertical divider
        var dividerX = Bounds.X + PaddingX + columnWidth + PaddingX / 2;
        spriteBatch.Draw(PixelTexture,
            new Rectangle(dividerX, Bounds.Y + PaddingTop + HeaderHeight - 5, 1,
                columnsBottomY - (Bounds.Y + PaddingTop + HeaderHeight - 5)),
            new Color(80, 80, 100));

        // Separator above description
        spriteBatch.Draw(PixelTexture,
            new Rectangle(Bounds.X + PaddingX, columnsBottomY, Bounds.Width - PaddingX * 2, 1),
            new Color(80, 80, 100));

        // Description area
        DrawDescriptionArea(spriteBatch, leftX, columnsBottomY + 6);

        // Bottom bar
        DrawBottomBar(spriteBatch, leftX);
    }

    private void DrawEquipmentColumn(SpriteBatch spriteBatch, int x, int width, int startY, int bottomY)
    {
        var y = startY;
        for (int i = 0; i < DisplaySlots.Length; i++)
        {
            if (y + ItemLineHeight > bottomY) break;

            var slot = DisplaySlots[i];
            var equipped = _player.Equipment.TryGetValue(slot, out var item);

            if (_inEquipmentSection && i == _selectedIndex)
            {
                spriteBatch.Draw(PixelTexture,
                    new Rectangle(x - 2, y, width + 4, ItemLineHeight),
                    Color.White * 0.12f);
            }

            var textColor = _inEquipmentSection && i == _selectedIndex ? Color.Yellow : Color.White;
            var prefix = _inEquipmentSection && i == _selectedIndex ? "> " : "  ";

            if (equipped)
            {
                var itemColor = _inEquipmentSection && i == _selectedIndex ? Color.Yellow : Color.LightGreen;
                var statStr = BuildStatSuffix(item);
                var slotStr = $"{prefix}{slot.ToString(),-10}";
                spriteBatch.DrawString(Font, slotStr, new Vector2(x, y + 2), textColor);
                var afterSlot = x + Font.MeasureString(slotStr).X;
                spriteBatch.DrawString(Font, item.Name, new Vector2(afterSlot, y + 2), itemColor);

                if (statStr.Length > 0)
                {
                    var afterName = afterSlot + Font.MeasureString(item.Name).X;
                    var availableWidth = x + width - afterName;
                    var statDisplay = $" {statStr}";
                    if (Font.MeasureString(statDisplay).X <= availableWidth)
                    {
                        spriteBatch.DrawString(Font, statDisplay, new Vector2(afterName, y + 2), Color.Gray);
                    }
                }
            }
            else
            {
                spriteBatch.DrawString(Font, $"{prefix}{slot.ToString(),-10}", new Vector2(x, y + 2), textColor);
                var afterSlot = x + Font.MeasureString($"{prefix}{slot.ToString(),-10}").X;
                spriteBatch.DrawString(Font, "(empty)", new Vector2(afterSlot, y + 2), Color.DarkGray);
            }

            y += ItemLineHeight;
        }
    }

    private void DrawInventoryColumn(SpriteBatch spriteBatch, int x, int width, int startY, int maxItems)
    {
        if (_player.Inventory.Count == 0)
        {
            spriteBatch.DrawString(Font, "  (empty)", new Vector2(x, startY + 2), Color.DarkGray);
            return;
        }

        // Clamp selected index
        if (!_inEquipmentSection && _selectedIndex >= _player.Inventory.Count)
            _selectedIndex = Math.Max(0, _player.Inventory.Count - 1);

        var y = startY;
        var endIndex = Math.Min(_player.Inventory.Count, _inventoryScrollOffset + maxItems);
        for (int i = _inventoryScrollOffset; i < endIndex; i++)
        {
            var inv = _player.Inventory[i];

            if (!_inEquipmentSection && i == _selectedIndex)
            {
                spriteBatch.Draw(PixelTexture,
                    new Rectangle(x - 2, y, width + 4, ItemLineHeight),
                    Color.White * 0.12f);
            }

            var textColor = !_inEquipmentSection && i == _selectedIndex ? Color.Yellow : Color.White;
            var prefix = !_inEquipmentSection && i == _selectedIndex ? "> " : "  ";
            var equipTag = IsItemEquipped(inv.Item) ? " [E]" : "";
            var text = inv.Quantity > 1
                ? $"{prefix}{inv.Item.Name} x{inv.Quantity}{equipTag}"
                : $"{prefix}{inv.Item.Name}{equipTag}";

            spriteBatch.DrawString(Font, text, new Vector2(x, y + 2), textColor);
            y += ItemLineHeight;
        }

        // Scroll indicator (top)
        if (_inventoryScrollOffset > 0)
        {
            var arrowText = "^";
            var arrowSize = Font.MeasureString(arrowText);
            spriteBatch.DrawString(Font, arrowText,
                new Vector2(x + width - arrowSize.X - 4, startY + 2), Color.DarkGray);
        }

        // Scroll indicator (bottom)
        if (endIndex < _player.Inventory.Count)
        {
            var arrowText = "v";
            var arrowSize = Font.MeasureString(arrowText);
            var arrowY = startY + (maxItems - 1) * ItemLineHeight + 2;
            spriteBatch.DrawString(Font, arrowText,
                new Vector2(x + width - arrowSize.X - 4, arrowY), Color.DarkGray);
        }
    }

    private bool IsItemEquipped(Item item)
    {
        if (item.Slot == EquipmentSlot.None) return false;
        return _player.Equipment.TryGetValue(item.Slot, out var equipped) && equipped.Id == item.Id;
    }

    private void DrawDescriptionArea(SpriteBatch spriteBatch, int leftX, int descY)
    {
        var descMaxWidth = Bounds.Width - PaddingX * 2;

        if (_inEquipmentSection)
        {
            // Show stat detail for equipped item
            var slot = DisplaySlots[_selectedIndex];
            if (_player.Equipment.TryGetValue(slot, out var item))
            {
                var detailText = BuildStatDetail(item);
                if (detailText.Length > 0)
                {
                    spriteBatch.DrawString(Font, detailText, new Vector2(leftX, descY), Color.CornflowerBlue);
                    descY += StatLineHeight;
                }

                // Item description
                if (!string.IsNullOrEmpty(item.Description))
                {
                    var lines = WrapText(item.Description, descMaxWidth);
                    foreach (var line in lines)
                    {
                        if (descY + LineHeight > Bounds.Bottom - BottomBarHeight) break;
                        spriteBatch.DrawString(Font, line, new Vector2(leftX, descY), Color.Gray);
                        descY += LineHeight;
                    }
                }
            }
            else
            {
                spriteBatch.DrawString(Font, $"{slot} — (empty)", new Vector2(leftX, descY), Color.DarkGray);
            }
        }
        else
        {
            // Show description + stats for selected inventory item
            if (_selectedIndex >= 0 && _selectedIndex < _player.Inventory.Count)
            {
                var item = _player.Inventory[_selectedIndex].Item;

                if (item.Slot != EquipmentSlot.None)
                {
                    var detailText = BuildStatDetail(item);
                    if (detailText.Length > 0)
                    {
                        spriteBatch.DrawString(Font, detailText, new Vector2(leftX, descY), Color.CornflowerBlue);
                        descY += StatLineHeight;
                    }
                }

                if (!string.IsNullOrEmpty(item.Description))
                {
                    var lines = WrapText(item.Description, descMaxWidth);
                    foreach (var line in lines)
                    {
                        if (descY + LineHeight > Bounds.Bottom - BottomBarHeight) break;
                        spriteBatch.DrawString(Font, line, new Vector2(leftX, descY), Color.Gray);
                        descY += LineHeight;
                    }
                }
            }
        }
    }

    private void DrawBottomBar(SpriteBatch spriteBatch, int leftX)
    {
        var bottomY = Bounds.Bottom - BottomBarHeight + 5;

        // Separator above bottom bar
        spriteBatch.Draw(PixelTexture,
            new Rectangle(Bounds.X + PaddingX, bottomY - 5, Bounds.Width - PaddingX * 2, 1),
            new Color(80, 80, 100));

        // Status message
        if (!string.IsNullOrEmpty(_statusMessage))
        {
            var alpha = _statusTimer > 0.5 ? 1f : (float)(_statusTimer / 0.5);
            spriteBatch.DrawString(Font, _statusMessage, new Vector2(leftX, bottomY), Color.LightGreen * alpha);
        }

        // Control hints
        var hints = "[E] Equip/Use  [Tab] Switch  [Esc] Close";
        var hintsSize = Font.MeasureString(hints);
        spriteBatch.DrawString(Font, hints,
            new Vector2(Bounds.Right - PaddingX - hintsSize.X, bottomY + LineHeight), Color.DarkGray);
    }

    private static string BuildStatSuffix(Item item)
    {
        var parts = new List<string>();
        if (item.AttackRating > 0) parts.Add($"{item.AttackRating:F0}A");
        if (item.ArmorRating > 0) parts.Add($"{item.ArmorRating:F0}D");
        if (item.StrengthBonus > 0) parts.Add($"{item.StrengthBonus}S");
        if (item.DexterityBonus > 0) parts.Add($"{item.DexterityBonus}Dx");
        if (item.ConstitutionBonus > 0) parts.Add($"{item.ConstitutionBonus}C");
        if (item.LuckBonus > 0) parts.Add($"{item.LuckBonus}L");
        return parts.Count > 0 ? string.Join(" ", parts) : "";
    }

    private static string BuildStatDetail(Item item)
    {
        var parts = new List<string>();
        if (item.AttackRating > 0) parts.Add($"ATK +{item.AttackRating:F0}");
        if (item.ArmorRating > 0) parts.Add($"ARM +{item.ArmorRating:F0}");
        if (item.StrengthBonus > 0) parts.Add($"STR +{item.StrengthBonus}");
        if (item.DexterityBonus > 0) parts.Add($"DEX +{item.DexterityBonus}");
        if (item.ConstitutionBonus > 0) parts.Add($"CON +{item.ConstitutionBonus}");
        if (item.LuckBonus > 0) parts.Add($"LCK +{item.LuckBonus}");
        parts.Add($"[{item.Slot}]");
        return string.Join("  ", parts);
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
