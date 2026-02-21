using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Janthus.Model.Entities;
using Janthus.Model.Enums;
using Janthus.Model.Interfaces;
using Janthus.Game.Actors;
using Janthus.Game.Rendering;

namespace Janthus.Game.World;

public class IsometricRenderer
{
    public static int TileWidth => RenderConstants.TileWidth;
    public static int TileHeight => RenderConstants.TileHeight;
    public static int HeightStep => RenderConstants.HeightStep;

    private readonly Texture2D _pixelTexture;
    private SpriteFont _font;
    private TileAtlas _tileAtlas;
    private ObjectAtlas _objectAtlas;

    public IsometricRenderer(Texture2D pixelTexture)
    {
        _pixelTexture = pixelTexture;
    }

    public void SetFont(SpriteFont font)
    {
        _font = font;
    }

    public void SetTileAtlas(TileAtlas atlas)
    {
        _tileAtlas = atlas;
    }

    public void SetObjectAtlas(ObjectAtlas atlas)
    {
        _objectAtlas = atlas;
    }

    public Vector2 TileToScreen(int tileX, int tileY)
    {
        return RenderConstants.TileToScreen(tileX, tileY);
    }

    public Vector2 TileToScreenWithElevation(int tileX, int tileY, int elevation)
    {
        return RenderConstants.TileToScreen(tileX, tileY, elevation);
    }

    public Point ScreenToTile(Vector2 screenPos)
    {
        return RenderConstants.ScreenToTile(screenPos);
    }

    public Point ScreenToTile(Vector2 screenPos, ChunkManager chunkManager)
    {
        // First pass: flat pick ignoring elevation
        var tile = RenderConstants.ScreenToTile(screenPos);

        // Iteratively refine: the tile at (tileX, tileY) may have elevation that shifts
        // its visual position upward. We compensate by adding the elevation offset back
        // to the screen position and re-picking.
        for (int i = 0; i < 3; i++)
        {
            if (!chunkManager.IsInBounds(tile.X, tile.Y))
                break;

            var elevation = chunkManager.GetElevation(tile.X, tile.Y);
            if (elevation == 0)
                break;

            var adjusted = new Vector2(screenPos.X, screenPos.Y + elevation * HeightStep);
            var newTile = RenderConstants.ScreenToTile(adjusted);
            if (newTile == tile)
                break;
            tile = newTile;
        }

        return tile;
    }

    public (int minX, int minY, int maxX, int maxY) GetVisibleTileRange(Camera camera, int worldWidth, int worldHeight)
    {
        var viewport = camera.GetViewport();

        var topLeft = camera.ScreenToWorld(Vector2.Zero);
        var topRight = camera.ScreenToWorld(new Vector2(viewport.Width, 0));
        var bottomLeft = camera.ScreenToWorld(new Vector2(0, viewport.Height));
        var bottomRight = camera.ScreenToWorld(new Vector2(viewport.Width, viewport.Height));

        var tl = ScreenToTile(topLeft);
        var tr = ScreenToTile(topRight);
        var bl = ScreenToTile(bottomLeft);
        var br = ScreenToTile(bottomRight);

        const int margin = 5;
        var minX = Math.Min(Math.Min(tl.X, tr.X), Math.Min(bl.X, br.X)) - margin;
        var minY = Math.Min(Math.Min(tl.Y, tr.Y), Math.Min(bl.Y, br.Y)) - margin;
        var maxX = Math.Max(Math.Max(tl.X, tr.X), Math.Max(bl.X, br.X)) + margin;
        var maxY = Math.Max(Math.Max(tl.Y, tr.Y), Math.Max(bl.Y, br.Y)) + margin;

        minX = Math.Max(0, minX);
        minY = Math.Max(0, minY);
        maxX = Math.Min(worldWidth - 1, maxX);
        maxY = Math.Min(worldHeight - 1, maxY);

        return (minX, minY, maxX, maxY);
    }

    public void DrawMap(SpriteBatch spriteBatch, ChunkManager chunkManager, Camera camera, VisibilityMap visibility = null)
    {
        var (minX, minY, maxX, maxY) = GetVisibleTileRange(camera, chunkManager.WorldWidth, chunkManager.WorldHeight);

        for (int wy = minY; wy <= maxY; wy++)
        {
            for (int wx = minX; wx <= maxX; wx++)
            {
                if (visibility != null)
                {
                    var vis = visibility.GetVisibility(wx, wy);
                    if (vis == TileVisibility.Unexplored)
                        continue;
                }

                var tile = chunkManager.GetTile(wx, wy);
                if (tile == null) continue;

                var elevation = chunkManager.GetElevation(wx, wy);
                var screenPos = TileToScreenWithElevation(wx, wy, elevation);

                var isExplored = visibility != null && visibility.GetVisibility(wx, wy) == TileVisibility.Explored;

                // Draw elevation side face to fill vertical gaps
                if (elevation > 0)
                {
                    var sideColor = isExplored ? new Color(
                        (int)(tile.Color.R * 0.4f),
                        (int)(tile.Color.G * 0.4f),
                        (int)(tile.Color.B * 0.4f)) : tile.Color;
                    DrawElevationSideFace(spriteBatch, screenPos, elevation, sideColor);
                }

                if (_tileAtlas != null && _tileAtlas.TryGetSourceRect(tile.TileDefinitionId, out var srcRect))
                {
                    var tint = isExplored ? new Color(100, 100, 100) : Color.White;
                    spriteBatch.Draw(_tileAtlas.Texture, screenPos, srcRect, tint);
                }
                else
                {
                    DrawIsoDiamond(spriteBatch, screenPos, tile.Color);
                    if (isExplored)
                        DrawIsoDiamond(spriteBatch, screenPos, Color.Black * 0.6f);
                }
            }
        }
    }

    public void DrawActor(SpriteBatch spriteBatch, ActorSprite sprite, ChunkManager chunkManager, Camera camera, bool isPlayer = false)
    {
        var screenPos = sprite.VisualPosition;

        if (sprite.SpriteSheet != null)
        {
            var anim = sprite.SpriteSheet.GetAnimation(sprite.Animator.CurrentAnimation);
            if (anim != null)
            {
                var srcRect = anim.GetSourceRect(sprite.Facing, sprite.Animator.CurrentFrame);
                var destPos = new Vector2(
                    screenPos.X + TileWidth / 2 - anim.FrameWidth / 2,
                    screenPos.Y + TileHeight / 2 - anim.FrameHeight);
                spriteBatch.Draw(sprite.SpriteSheet.Texture, destPos, srcRect, sprite.EffectiveColor);
            }
        }
        else
        {
            DrawActorFallback(spriteBatch, sprite, screenPos, isPlayer);
        }

        // Draw name label with alignment abbreviation
        if (_font != null)
        {
            var labelText = sprite.Label;

            if (sprite.DomainActor is IAligned aligned)
            {
                var abbr = GetAlignmentAbbreviation(aligned.Alignment);
                labelText = $"{sprite.Label} [{abbr}]";
            }

            var actorHeight = sprite.SpriteSheet != null
                ? sprite.SpriteSheet.GetAnimation(sprite.Animator.CurrentAnimation)?.FrameHeight ?? RenderConstants.ActorHeight
                : RenderConstants.ActorHeight;

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

    private void DrawActorFallback(SpriteBatch spriteBatch, ActorSprite sprite, Vector2 screenPos, bool isPlayer)
    {
        var actorWidth = RenderConstants.ActorWidth;
        var actorHeight = RenderConstants.ActorHeight;
        var rect = new Rectangle(
            (int)(screenPos.X + TileWidth / 2 - actorWidth / 2),
            (int)(screenPos.Y - actorHeight + TileHeight / 2),
            actorWidth,
            actorHeight);

        spriteBatch.Draw(_pixelTexture, rect, sprite.EffectiveColor);

        // Shadow
        var shadowRect = new Rectangle(
            (int)(screenPos.X + TileWidth / 2 - 16),
            (int)(screenPos.Y + TileHeight / 2 - 4),
            32, 8);
        spriteBatch.Draw(_pixelTexture, shadowRect, Color.Black * 0.3f);
    }

    public void DrawObject(SpriteBatch spriteBatch, int worldX, int worldY, int objectDefId, int elevation)
    {
        var screenPos = TileToScreenWithElevation(worldX, worldY, elevation);
        var centerX = (int)(screenPos.X + TileWidth / 2);
        var baseY = (int)(screenPos.Y + TileHeight / 2);

        if (_objectAtlas != null && _objectAtlas.TryGetSourceRect(objectDefId, out var srcRect))
        {
            // Anchor sprite so its bottom edge sits at the tile's base center,
            // offset upward by a quarter tile height so objects appear grounded
            var destPos = new Vector2(
                centerX - srcRect.Width / 2,
                baseY - srcRect.Height + TileHeight / 4);
            spriteBatch.Draw(_objectAtlas.Texture, destPos, srcRect, Color.White);
            return;
        }

        switch (objectDefId)
        {
            case 1: // Tree
                DrawTreeShape(spriteBatch, centerX, baseY);
                break;
            case 2: // Boulder
                DrawBoulderShape(spriteBatch, centerX, baseY);
                break;
            case 3: // Wall
                var wallRect = new Rectangle(centerX - 24, baseY - 40, 48, 40);
                spriteBatch.Draw(_pixelTexture, wallRect, new Color(60, 60, 60));
                break;
        }
    }

    private void DrawTreeShape(SpriteBatch spriteBatch, int cx, int baseY)
    {
        // Trunk
        var trunkRect = new Rectangle(cx - 4, baseY - 16, 8, 16);
        spriteBatch.Draw(_pixelTexture, trunkRect, new Color(101, 67, 33));

        // Canopy
        var canopyHeight = 36;
        var canopyWidth = 32;
        for (int row = 0; row < canopyHeight; row++)
        {
            var w = (int)((1.0f - row / (float)canopyHeight) * canopyWidth);
            if (w > 0)
            {
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(cx - w / 2, baseY - 16 - canopyHeight + row, w, 1),
                    new Color(34, 120, 34));
            }
        }
    }

    private void DrawBoulderShape(SpriteBatch spriteBatch, int cx, int baseY)
    {
        var radius = 14;
        for (int row = -radius; row <= radius; row++)
        {
            var halfWidth = (int)Math.Sqrt(radius * radius - row * row);
            if (halfWidth > 0)
            {
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(cx - halfWidth, baseY - radius + row - 8, halfWidth * 2, 1),
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

        // Outline only for opaque tiles (not fog overlay)
        if (color.A == 255)
            DrawDiamondOutline(spriteBatch, pos, Color.Black * 0.2f);
    }

    private void DrawDiamondOutline(SpriteBatch spriteBatch, Vector2 pos, Color color)
    {
        var halfW = TileWidth / 2;
        var halfH = TileHeight / 2;

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

    private void DrawElevationSideFace(SpriteBatch spriteBatch, Vector2 tileScreenPos, int elevation, Color tileColor)
    {
        // Draw a darkened trapezoid beneath the tile diamond to fill the vertical gap
        var sideHeight = elevation * HeightStep;
        var halfW = TileWidth / 2;
        var halfH = TileHeight / 2;
        var darkColor = new Color(
            (int)(tileColor.R * 0.4f),
            (int)(tileColor.G * 0.4f),
            (int)(tileColor.B * 0.4f));

        // Left side face: from bottom-left edge of diamond downward
        for (int row = 0; row < sideHeight; row++)
        {
            // Bottom-left edge of diamond runs from center-bottom to left corner
            // At diamond bottom (halfH), the left edge x = pos.X, right edge x = pos.X + halfW
            var y = (int)(tileScreenPos.Y + halfH + row);
            var leftX = (int)(tileScreenPos.X);
            var midX = (int)(tileScreenPos.X + halfW);
            if (midX > leftX)
                spriteBatch.Draw(_pixelTexture, new Rectangle(leftX, y, midX - leftX, 1), darkColor);
        }

        // Right side face: from bottom-right edge of diamond downward
        var darkColor2 = new Color(
            (int)(tileColor.R * 0.3f),
            (int)(tileColor.G * 0.3f),
            (int)(tileColor.B * 0.3f));
        for (int row = 0; row < sideHeight; row++)
        {
            var y = (int)(tileScreenPos.Y + halfH + row);
            var midX = (int)(tileScreenPos.X + halfW);
            var rightX = (int)(tileScreenPos.X + TileWidth);
            if (rightX > midX)
                spriteBatch.Draw(_pixelTexture, new Rectangle(midX, y, rightX - midX, 1), darkColor2);
        }
    }
}
