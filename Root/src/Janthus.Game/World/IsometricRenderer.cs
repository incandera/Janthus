using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Janthus.Model.Entities;
using Janthus.Model.Enums;
using Janthus.Model.Interfaces;
using Janthus.Game.Actors;

namespace Janthus.Game.World;

public class IsometricRenderer
{
    public const int TileWidth = 64;
    public const int TileHeight = 32;

    private readonly Texture2D _pixelTexture;
    private SpriteFont _font;

    public IsometricRenderer(Texture2D pixelTexture)
    {
        _pixelTexture = pixelTexture;
    }

    public void SetFont(SpriteFont font)
    {
        _font = font;
    }

    public Vector2 TileToScreen(int tileX, int tileY)
    {
        var screenX = (tileX - tileY) * (TileWidth / 2);
        var screenY = (tileX + tileY) * (TileHeight / 2);
        return new Vector2(screenX, screenY);
    }

    public Point ScreenToTile(Vector2 screenPos)
    {
        var halfW = TileWidth / 2f;
        var halfH = TileHeight / 2f;
        var tileX = (int)Math.Floor((screenPos.X / halfW + screenPos.Y / halfH) / 2);
        var tileY = (int)Math.Floor((screenPos.Y / halfH - screenPos.X / halfW) / 2);
        return new Point(tileX, tileY);
    }

    public void DrawMap(SpriteBatch spriteBatch, TileMap map, Camera camera)
    {
        // Back-to-front render for correct depth
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                var tile = map.GroundLayer.Tiles[x, y];
                if (tile == null) continue;

                var screenPos = TileToScreen(x, y);
                DrawIsoDiamond(spriteBatch, screenPos, tile.Color);
            }
        }
    }

    public void DrawActor(SpriteBatch spriteBatch, ActorSprite sprite, Camera camera, bool isPlayer = false)
    {
        var screenPos = TileToScreen(sprite.TileX, sprite.TileY);

        // Draw actor as a colored rectangle centered on the tile
        var actorWidth = 16;
        var actorHeight = 24;
        var rect = new Rectangle(
            (int)(screenPos.X + TileWidth / 2 - actorWidth / 2),
            (int)(screenPos.Y - actorHeight + TileHeight / 2),
            actorWidth,
            actorHeight);

        spriteBatch.Draw(_pixelTexture, rect, sprite.Color);

        // Draw a small shadow under the actor
        var shadowRect = new Rectangle(
            (int)(screenPos.X + TileWidth / 2 - 8),
            (int)(screenPos.Y + TileHeight / 2 - 2),
            16, 4);
        spriteBatch.Draw(_pixelTexture, shadowRect, Color.Black * 0.3f);

        // Draw name label with alignment abbreviation
        if (_font != null)
        {
            var labelText = sprite.Label;

            // Add alignment abbreviation if actor is IAligned
            if (sprite.DomainActor is IAligned aligned)
            {
                var abbr = GetAlignmentAbbreviation(aligned.Alignment);
                labelText = $"{sprite.Label} [{abbr}]";
            }

            var labelSize = _font.MeasureString(labelText);
            var labelX = screenPos.X + TileWidth / 2 - labelSize.X / 2;
            var labelY = screenPos.Y - actorHeight + TileHeight / 2 - labelSize.Y - 4;

            Color labelColor;
            if (isPlayer)
                labelColor = Color.Cyan;
            else if (sprite.IsAdversary)
                labelColor = Color.Red;
            else
                labelColor = Color.LimeGreen;

            spriteBatch.DrawString(_font, labelText, new Vector2(labelX, labelY), labelColor);
        }
    }

    private static string GetAlignmentAbbreviation(Alignment alignment)
    {
        var lawful = alignment.Lawfulness switch
        {
            LawfulnessType.Lawful => "L",
            LawfulnessType.Chaotic => "C",
            _ => "N"
        };
        var disposition = alignment.Disposition switch
        {
            DispositionType.Good => "G",
            DispositionType.Evil => "E",
            _ => "N"
        };
        return lawful + disposition;
    }

    private void DrawIsoDiamond(SpriteBatch spriteBatch, Vector2 pos, Color color)
    {
        var halfW = TileWidth / 2;
        var halfH = TileHeight / 2;

        // Draw as a filled diamond using horizontal lines
        for (int row = 0; row < TileHeight; row++)
        {
            int width;
            int startX;

            if (row < halfH)
            {
                width = (int)((row / (float)halfH) * halfW) * 2;
                startX = (int)(pos.X + halfW - width / 2);
            }
            else
            {
                var invertedRow = TileHeight - 1 - row;
                width = (int)((invertedRow / (float)halfH) * halfW) * 2;
                startX = (int)(pos.X + halfW - width / 2);
            }

            if (width > 0)
            {
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(startX, (int)(pos.Y + row), width, 1),
                    color);
            }
        }

        // Draw diamond outline for visual separation
        DrawDiamondOutline(spriteBatch, pos, Color.Black * 0.2f);
    }

    private void DrawDiamondOutline(SpriteBatch spriteBatch, Vector2 pos, Color color)
    {
        var halfW = TileWidth / 2;
        var halfH = TileHeight / 2;

        // Top half outline
        for (int row = 0; row <= halfH; row++)
        {
            var width = (int)((row / (float)halfH) * halfW);
            var leftX = (int)(pos.X + halfW - width);
            var rightX = (int)(pos.X + halfW + width - 1);
            var y = (int)(pos.Y + row);

            spriteBatch.Draw(_pixelTexture, new Rectangle(leftX, y, 1, 1), color);
            if (width > 0)
                spriteBatch.Draw(_pixelTexture, new Rectangle(rightX, y, 1, 1), color);
        }

        // Bottom half outline
        for (int row = 0; row <= halfH; row++)
        {
            var invertedRow = halfH - row;
            var width = (int)((invertedRow / (float)halfH) * halfW);
            var leftX = (int)(pos.X + halfW - width);
            var rightX = (int)(pos.X + halfW + width - 1);
            var y = (int)(pos.Y + halfH + row);

            spriteBatch.Draw(_pixelTexture, new Rectangle(leftX, y, 1, 1), color);
            if (width > 0)
                spriteBatch.Draw(_pixelTexture, new Rectangle(rightX, y, 1, 1), color);
        }
    }
}
