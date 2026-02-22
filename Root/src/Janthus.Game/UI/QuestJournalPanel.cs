using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Janthus.Model.Entities;
using Janthus.Model.Enums;
using Janthus.Model.Services;
using Janthus.Game.Input;

namespace Janthus.Game.UI;

public class QuestJournalPanel : UIPanel
{
    private readonly IGameDataProvider _dataProvider;
    private List<QuestDefinition> _visibleQuests = new();
    private int _selectedIndex;
    private float _refreshTimer;

    private const float RefreshInterval = 0.5f;
    private const int LeftPaneWidth = 220;
    private const int Padding = 15;
    private const int LineHeight = 20;

    private static readonly Color BgColor = new(20, 20, 40, 220);
    private static readonly Color BorderColor = new(100, 80, 60);
    private static readonly Color GoldColor = Color.Gold;
    private static readonly Color GreenColor = new(100, 200, 100);
    private static readonly Color RedColor = new(200, 100, 100);
    private static readonly Color HighlightColor = Color.White * 0.12f;

    public QuestJournalPanel(Texture2D pixelTexture, SpriteFont font, IGameDataProvider dataProvider,
                             Rectangle bounds)
        : base(pixelTexture, font, bounds)
    {
        _dataProvider = dataProvider;
        IsVisible = false;
    }

    public override void Update(GameTime gameTime, InputManager input)
    {
        if (!IsVisible) return;

        _refreshTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_refreshTimer <= 0)
        {
            RefreshQuests();
            _refreshTimer = RefreshInterval;
        }

        if (_visibleQuests.Count == 0) return;

        if (input.IsKeyPressed(Keys.Up))
            _selectedIndex = (_selectedIndex - 1 + _visibleQuests.Count) % _visibleQuests.Count;
        if (input.IsKeyPressed(Keys.Down))
            _selectedIndex = (_selectedIndex + 1) % _visibleQuests.Count;

        // Mouse click on left pane quest list
        if (input.IsLeftClickPressed())
        {
            var mousePos = input.MousePosition;
            var listX = Bounds.X + Padding;
            var listY = Bounds.Y + Padding + LineHeight + 10;
            var listRight = Bounds.X + Padding + LeftPaneWidth;

            if (mousePos.X >= listX && mousePos.X <= listRight)
            {
                var relY = mousePos.Y - listY;
                if (relY >= 0)
                {
                    var clickedIndex = relY / LineHeight;
                    if (clickedIndex >= 0 && clickedIndex < _visibleQuests.Count)
                        _selectedIndex = clickedIndex;
                }
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        DrawPanel(spriteBatch, BgColor, BorderColor);

        var x = Bounds.X + Padding;
        var y = Bounds.Y + Padding;

        // Title
        spriteBatch.DrawString(Font, "QUEST JOURNAL", new Vector2(x, y), GoldColor);
        y += LineHeight + 10;

        if (_visibleQuests.Count == 0)
        {
            var emptyText = "No active quests. Press [J] to close.";
            var emptySize = Font.MeasureString(emptyText);
            spriteBatch.DrawString(Font, emptyText,
                new Vector2(Bounds.X + Bounds.Width / 2 - emptySize.X / 2,
                            Bounds.Y + Bounds.Height / 2 - emptySize.Y / 2),
                Color.Gray);
            return;
        }

        // Divider line between left and right panes
        var dividerX = Bounds.X + Padding + LeftPaneWidth + 5;
        spriteBatch.Draw(PixelTexture,
            new Rectangle(dividerX, Bounds.Y + Padding, 1, Bounds.Height - Padding * 2),
            new Color(80, 80, 100));

        // Left pane: quest list
        var listY = y;
        for (int i = 0; i < _visibleQuests.Count; i++)
        {
            var quest = _visibleQuests[i];
            var status = QuestEvaluator.GetQuestStatus(quest, _dataProvider);
            var isSelected = i == _selectedIndex;

            if (isSelected)
            {
                spriteBatch.Draw(PixelTexture,
                    new Rectangle(x - 2, listY - 1, LeftPaneWidth + 4, LineHeight),
                    HighlightColor);
            }

            var indicator = status switch
            {
                QuestStatus.Active => "[*]",
                QuestStatus.Completed => "[v]",
                QuestStatus.Failed => "[x]",
                _ => "[ ]"
            };
            var indicatorColor = status switch
            {
                QuestStatus.Active => GoldColor,
                QuestStatus.Completed => GreenColor,
                QuestStatus.Failed => RedColor,
                _ => Color.Gray
            };

            var prefix = isSelected ? "> " : "  ";
            var nameColor = isSelected ? Color.Yellow : Color.White;

            // Truncate quest name to fit left pane
            var displayName = TruncateText(quest.Name, LeftPaneWidth - 50);
            spriteBatch.DrawString(Font, $"{prefix}{displayName}", new Vector2(x, listY), nameColor);

            var indicatorX = x + LeftPaneWidth - Font.MeasureString(indicator).X;
            spriteBatch.DrawString(Font, indicator, new Vector2(indicatorX, listY), indicatorColor);

            listY += LineHeight;
        }

        // Right pane: selected quest details
        if (_selectedIndex >= 0 && _selectedIndex < _visibleQuests.Count)
        {
            var quest = _visibleQuests[_selectedIndex];
            var status = QuestEvaluator.GetQuestStatus(quest, _dataProvider);
            var rightX = dividerX + 15;
            var rightWidth = Bounds.Right - rightX - Padding;
            var ry = y;

            // Quest title
            spriteBatch.DrawString(Font, quest.Name, new Vector2(rightX, ry), GoldColor);
            ry += LineHeight;

            // Status
            var statusText = $"Status: {status}";
            var statusColor = status switch
            {
                QuestStatus.Active => GoldColor,
                QuestStatus.Completed => GreenColor,
                QuestStatus.Failed => RedColor,
                _ => Color.Gray
            };
            spriteBatch.DrawString(Font, statusText, new Vector2(rightX, ry), statusColor);
            ry += LineHeight + 10;

            // Description (wrapped)
            var wrappedDesc = WrapText(quest.Description, rightWidth);
            foreach (var line in wrappedDesc)
            {
                if (ry > Bounds.Bottom - Padding - LineHeight * 2) break;
                spriteBatch.DrawString(Font, line, new Vector2(rightX, ry), Color.LightGray);
                ry += LineHeight;
            }

            ry += 10;

            // Goals header
            if (quest.Goals.Count > 0 && ry < Bounds.Bottom - Padding - LineHeight)
            {
                spriteBatch.DrawString(Font, "GOALS", new Vector2(rightX, ry), GoldColor);
                ry += LineHeight + 4;

                foreach (var goal in quest.Goals)
                {
                    if (ry > Bounds.Bottom - Padding - LineHeight) break;

                    var isComplete = QuestEvaluator.IsGoalComplete(goal, _dataProvider);
                    var checkmark = isComplete ? "[v]" : "[-]";
                    var checkColor = isComplete ? GreenColor : Color.White;
                    var textColor = isComplete ? Color.Gray : Color.White;

                    var goalText = goal.Description;
                    if (goal.IsOptional)
                        goalText += " (optional)";

                    spriteBatch.DrawString(Font, checkmark, new Vector2(rightX, ry), checkColor);
                    var goalTextColor = goal.IsOptional && !isComplete ? Color.LightGray : textColor;
                    spriteBatch.DrawString(Font, goalText, new Vector2(rightX + 30, ry), goalTextColor);
                    ry += LineHeight;
                }
            }
        }

        // Close hint
        var closeText = "[J] Close";
        var closeSize = Font.MeasureString(closeText);
        spriteBatch.DrawString(Font, closeText,
            new Vector2(Bounds.Right - closeSize.X - Padding, Bounds.Bottom - closeSize.Y - Padding),
            Color.Gray);
    }

    private void RefreshQuests()
    {
        var allQuests = _dataProvider.GetQuestDefinitions();
        _visibleQuests = QuestEvaluator.GetVisibleQuests(allQuests, _dataProvider);

        if (_selectedIndex >= _visibleQuests.Count)
            _selectedIndex = Math.Max(0, _visibleQuests.Count - 1);
    }

    private string TruncateText(string text, int maxWidth)
    {
        if (Font.MeasureString(text).X <= maxWidth) return text;

        for (int i = text.Length - 1; i > 0; i--)
        {
            var truncated = text[..i] + "..";
            if (Font.MeasureString(truncated).X <= maxWidth)
                return truncated;
        }
        return text;
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
