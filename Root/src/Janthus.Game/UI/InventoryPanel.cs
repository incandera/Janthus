using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
    private string _statusMessage = string.Empty;
    private double _statusTimer;

    private const int PaddingX = 12;
    private const int PaddingTop = 10;
    private const int LineHeight = 20;
    private const int ItemLineHeight = 22;
    private const int StatLineHeight = 16;

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

    public InventoryPanel(Texture2D pixelTexture, SpriteFont font, PlayerCharacter player, Rectangle bounds)
        : base(pixelTexture, font, bounds)
    {
        _player = player;
        IsVisible = false;
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

        // Mouse click on either section — switches tab and selects item in one action
        if (input.IsLeftClickPressed() && Bounds.Contains(input.MousePosition))
        {
            var clickY = input.MousePosition.Y;

            // Equipment items zone
            var equipItemsY = Bounds.Y + PaddingTop + LineHeight + LineHeight + 8;
            var equipItemsEnd = equipItemsY + DisplaySlots.Length * ItemLineHeight;

            // Inventory items zone
            var invSectionTop = equipItemsEnd + 12; // gap + separator
            var invHeaderEnd = invSectionTop + LineHeight + 4;

            if (clickY >= equipItemsY && clickY < equipItemsEnd)
            {
                var clickedIndex = (clickY - equipItemsY) / ItemLineHeight;
                if (clickedIndex >= 0 && clickedIndex < DisplaySlots.Length)
                {
                    _inEquipmentSection = true;
                    _selectedIndex = clickedIndex;
                }
            }
            else if (clickY >= invHeaderEnd && _player.Inventory.Count > 0)
            {
                var clickedIndex = (clickY - invHeaderEnd) / ItemLineHeight;
                if (clickedIndex >= 0 && clickedIndex < _player.Inventory.Count)
                {
                    _inEquipmentSection = false;
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
            CombatCalculator.Unequip(_player, _player.Inventory, slot);
        }

    }

    private void UpdateInventorySection(InputManager input)
    {
        if (_player.Inventory.Count == 0) return;

        if (input.IsKeyPressed(Keys.Up) || input.ScrollDelta > 0)
            _selectedIndex = (_selectedIndex - 1 + _player.Inventory.Count) % _player.Inventory.Count;
        if (input.IsKeyPressed(Keys.Down) || input.ScrollDelta < 0)
            _selectedIndex = (_selectedIndex + 1) % _player.Inventory.Count;

        // E or Enter to equip or consume
        if (input.IsKeyPressed(Keys.E) || input.IsKeyPressed(Keys.Enter))
        {
            if (_selectedIndex >= 0 && _selectedIndex < _player.Inventory.Count)
            {
                var item = _player.Inventory[_selectedIndex].Item;
                if (item.Slot != EquipmentSlot.None)
                {
                    CombatCalculator.Equip(_player, _player.Inventory, item);
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

        DrawPanel(spriteBatch, new Color(20, 20, 40, 220), new Color(100, 80, 60));

        var x = Bounds.X + PaddingX;
        var y = Bounds.Y + PaddingTop;

        // Header with gold
        spriteBatch.DrawString(Font, "EQUIPMENT", new Vector2(x, y),
            _inEquipmentSection ? Color.Gold : Color.Gray);
        var goldText = $"Gold: {_player.Gold:F0}";
        var goldSize = Font.MeasureString(goldText);
        spriteBatch.DrawString(Font, goldText,
            new Vector2(Bounds.Right - PaddingX - goldSize.X, y), Color.Yellow);
        y += LineHeight;

        // Equipment totals summary
        var baseAttack = _player.EffectiveStrength * 1.5m + _player.TotalEquipmentAttackRating;
        var baseArmor = _player.EffectiveConstitution * 0.5m + _player.TotalEquipmentArmorRating;
        var summaryText = $"ATK: {baseAttack:F0}  ARM: {baseArmor:F0}";
        spriteBatch.DrawString(Font, summaryText, new Vector2(x, y), Color.LightGray);
        y += LineHeight;

        // Separator
        spriteBatch.Draw(PixelTexture,
            new Rectangle(Bounds.X + 4, y, Bounds.Width - 8, 1), Color.Gray * 0.3f);
        y += 4;

        // Equipment slots
        for (int i = 0; i < DisplaySlots.Length; i++)
        {
            var slot = DisplaySlots[i];
            var equipped = _player.Equipment.TryGetValue(slot, out var item);

            if (_inEquipmentSection && i == _selectedIndex)
            {
                spriteBatch.Draw(PixelTexture,
                    new Rectangle(Bounds.X + 2, y, Bounds.Width - 4, ItemLineHeight),
                    Color.White * 0.12f);
            }

            var slotName = slot.ToString();
            var textColor = _inEquipmentSection && i == _selectedIndex ? Color.Yellow : Color.White;
            var prefix = _inEquipmentSection && i == _selectedIndex ? "> " : "  ";

            if (equipped)
            {
                var itemColor = _inEquipmentSection && i == _selectedIndex ? Color.Yellow : Color.LightGreen;
                var statStr = BuildStatSuffix(item);
                var slotStr = $"{prefix}{slotName,-10}";
                spriteBatch.DrawString(Font, slotStr, new Vector2(x, y + 2), textColor);
                var afterSlot = x + Font.MeasureString(slotStr).X;
                spriteBatch.DrawString(Font, item.Name, new Vector2(afterSlot, y + 2), itemColor);

                if (statStr.Length > 0)
                {
                    var afterName = afterSlot + Font.MeasureString(item.Name).X;
                    var availableWidth = Bounds.Right - PaddingX - afterName;
                    var statDisplay = $" {statStr}";
                    if (Font.MeasureString(statDisplay).X <= availableWidth)
                    {
                        spriteBatch.DrawString(Font, statDisplay, new Vector2(afterName, y + 2), Color.Gray);
                    }
                }
            }
            else
            {
                spriteBatch.DrawString(Font, $"{prefix}{slotName,-10}", new Vector2(x, y + 2), textColor);
                var afterSlot = x + Font.MeasureString($"{prefix}{slotName,-10}").X;
                spriteBatch.DrawString(Font, "(empty)", new Vector2(afterSlot, y + 2), Color.DarkGray);
            }

            y += ItemLineHeight;
        }

        y += 4;

        // Separator
        spriteBatch.Draw(PixelTexture,
            new Rectangle(Bounds.X + 4, y, Bounds.Width - 8, 1), Color.Gray * 0.3f);
        y += 4;

        // Inventory section header
        spriteBatch.DrawString(Font, "INVENTORY", new Vector2(x, y),
            !_inEquipmentSection ? Color.Gold : Color.Gray);
        y += LineHeight + 4;

        // Item list
        if (_player.Inventory.Count == 0)
        {
            spriteBatch.DrawString(Font, "(empty)", new Vector2(x, y), Color.DarkGray);
        }
        else
        {
            // Clamp selected index
            if (!_inEquipmentSection && _selectedIndex >= _player.Inventory.Count)
                _selectedIndex = Math.Max(0, _player.Inventory.Count - 1);

            var maxItems = (Bounds.Height - (y - Bounds.Y) - 60) / ItemLineHeight;
            for (int i = 0; i < Math.Min(_player.Inventory.Count, maxItems); i++)
            {
                var inv = _player.Inventory[i];

                if (!_inEquipmentSection && i == _selectedIndex)
                {
                    spriteBatch.Draw(PixelTexture,
                        new Rectangle(Bounds.X + 2, y, Bounds.Width - 4, ItemLineHeight),
                        Color.White * 0.12f);
                }

                var textColor = !_inEquipmentSection && i == _selectedIndex ? Color.Yellow : Color.White;
                var prefix = !_inEquipmentSection && i == _selectedIndex ? "> " : "  ";
                var equipTag = inv.Item.Slot != EquipmentSlot.None ? " [E]" : "";
                var text = inv.Quantity > 1
                    ? $"{prefix}{inv.Item.Name} x{inv.Quantity}{equipTag}"
                    : $"{prefix}{inv.Item.Name}{equipTag}";

                spriteBatch.DrawString(Font, text, new Vector2(x, y + 2), textColor);
                y += ItemLineHeight;
            }

            // Selected item stat details
            if (!_inEquipmentSection && _selectedIndex >= 0 && _selectedIndex < _player.Inventory.Count)
            {
                var selectedItem = _player.Inventory[_selectedIndex].Item;
                if (selectedItem.Slot != EquipmentSlot.None)
                {
                    y += 2;
                    var detailText = BuildStatDetail(selectedItem);
                    if (detailText.Length > 0 && y + StatLineHeight <= Bounds.Bottom - LineHeight - 4)
                    {
                        spriteBatch.DrawString(Font, detailText, new Vector2(x + 16, y), Color.CornflowerBlue);
                        y += StatLineHeight;
                    }
                }
            }
        }

        // Status message
        if (!string.IsNullOrEmpty(_statusMessage))
        {
            var msgY = Bounds.Bottom - LineHeight * 2 - 8;
            var alpha = _statusTimer > 0.5 ? 1f : (float)(_statusTimer / 0.5);
            spriteBatch.DrawString(Font, _statusMessage, new Vector2(x, msgY), Color.LightGreen * alpha);
        }

        // Control hints at bottom
        var hintY = Bounds.Bottom - LineHeight - 4;
        var hint = "[E] Equip/Use  [Tab] Switch";
        spriteBatch.DrawString(Font, hint, new Vector2(x, hintY), Color.Gray * 0.7f);
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
}
