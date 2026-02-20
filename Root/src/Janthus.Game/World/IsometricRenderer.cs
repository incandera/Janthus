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
    public const int HeightStep = 4;

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

    public Vector2 TileToScreenWithElevation(int tileX, int tileY, int elevation)
    {
        var screenX = (tileX - tileY) * (TileWidth / 2);
        var screenY = (tileX + tileY) * (TileHeight / 2) - elevation * HeightStep;
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

    public (int minX, int minY, int maxX, int maxY) GetVisibleTileRange(Camera camera, int worldWidth, int worldHeight)
    {
        var viewport = camera.GetViewport();

        // Transform viewport corners to world space, then to tile coords
        var topLeft = camera.ScreenToWorld(Vector2.Zero);
        var topRight = camera.ScreenToWorld(new Vector2(viewport.Width, 0));
        var bottomLeft = camera.ScreenToWorld(new Vector2(0, viewport.Height));
        var bottomRight = camera.ScreenToWorld(new Vector2(viewport.Width, viewport.Height));

        var tl = ScreenToTile(topLeft);
        var tr = ScreenToTile(topRight);
        var bl = ScreenToTile(bottomLeft);
        var br = ScreenToTile(bottomRight);

        // 3-tile margin for elevation offset
        const int margin = 3;
        var minX = Math.Min(Math.Min(tl.X, tr.X), Math.Min(bl.X, br.X)) - margin;
        var minY = Math.Min(Math.Min(tl.Y, tr.Y), Math.Min(bl.Y, br.Y)) - margin;
        var maxX = Math.Max(Math.Max(tl.X, tr.X), Math.Max(bl.X, br.X)) + margin;
        var maxY = Math.Max(Math.Max(tl.Y, tr.Y), Math.Max(bl.Y, br.Y)) + margin;

        // Clamp to world bounds
        minX = Math.Max(0, minX);
        minY = Math.Max(0, minY);
        maxX = Math.Min(worldWidth - 1, maxX);
        maxY = Math.Min(worldHeight - 1, maxY);

        return (minX, minY, maxX, maxY);
    }

    public void DrawMap(SpriteBatch spriteBatch, ChunkManager chunkManager, Camera camera)
    {
        var (minX, minY, maxX, maxY) = GetVisibleTileRange(camera, chunkManager.WorldWidth, chunkManager.WorldHeight);

        // Iterate only visible tile range
        for (int wy = minY; wy <= maxY; wy++)
        {
            for (int wx = minX; wx <= maxX; wx++)
            {
                var tile = chunkManager.GetTile(wx, wy);
                if (tile == null) continue;

                var elevation = chunkManager.GetElevation(wx, wy);
                var screenPos = TileToScreenWithElevation(wx, wy, elevation);
                DrawIsoDiamond(spriteBatch, screenPos, tile.Color);
            }
        }
    }

    public void DrawActor(SpriteBatch spriteBatch, ActorSprite sprite, ChunkManager chunkManager, Camera camera, bool isPlayer = false)
    {
        var screenPos = sprite.VisualPosition;

        // Draw actor as a colored rectangle centered on the tile
        var actorWidth = 16;
        var actorHeight = 24;
        var rect = new Rectangle(
            (int)(screenPos.X + TileWidth / 2 - actorWidth / 2),
            (int)(screenPos.Y - actorHeight + TileHeight / 2),
            actorWidth,
            actorHeight);

        spriteBatch.Draw(_pixelTexture, rect, sprite.EffectiveColor);

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

    public void DrawObject(SpriteBatch spriteBatch, int worldX, int worldY, int objectDefId, int elevation)
    {
        var screenPos = TileToScreenWithElevation(worldX, worldY, elevation);
        var centerX = (int)(screenPos.X + TileWidth / 2);
        var baseY = (int)(screenPos.Y + TileHeight / 2);

        switch (objectDefId)
        {
            case 1: // Tree — green triangle
                DrawTreeShape(spriteBatch, centerX, baseY);
                break;
            case 2: // Boulder — gray circle
                DrawBoulderShape(spriteBatch, centerX, baseY);
                break;
            case 3: // Wall — dark rectangle
                var wallRect = new Rectangle(centerX - 12, baseY - 20, 24, 20);
                spriteBatch.Draw(_pixelTexture, wallRect, new Color(60, 60, 60));
                break;
        }
    }

    private void DrawTreeShape(SpriteBatch spriteBatch, int cx, int baseY)
    {
        // Trunk
        var trunkRect = new Rectangle(cx - 2, baseY - 8, 4, 8);
        spriteBatch.Draw(_pixelTexture, trunkRect, new Color(101, 67, 33));

        // Canopy — triangle using horizontal scanlines
        var canopyHeight = 18;
        var canopyWidth = 16;
        for (int row = 0; row < canopyHeight; row++)
        {
            var w = (int)((1.0f - row / (float)canopyHeight) * canopyWidth);
            if (w > 0)
            {
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(cx - w / 2, baseY - 8 - canopyHeight + row, w, 1),
                    new Color(34, 120, 34));
            }
        }
    }

    private void DrawBoulderShape(SpriteBatch spriteBatch, int cx, int baseY)
    {
        // Approximate circle with horizontal scanlines
        var radius = 7;
        for (int row = -radius; row <= radius; row++)
        {
            var halfWidth = (int)Math.Sqrt(radius * radius - row * row);
            if (halfWidth > 0)
            {
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(cx - halfWidth, baseY - radius + row - 4, halfWidth * 2, 1),
                    new Color(140, 140, 140));
            }
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
