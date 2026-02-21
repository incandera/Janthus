using Microsoft.Xna.Framework;

namespace Janthus.Game.Rendering;

public static class RenderConstants
{
    public const int TileWidth = 128;
    public const int TileHeight = 64;
    public const int HeightStep = 8;

    public const int ActorWidth = 32;
    public const int ActorHeight = 48;

    public static int CalculateDepth(int tileX, int tileY, int elevation)
    {
        return (tileX + tileY) * 100 - elevation * 10;
    }

    public static Vector2 TileToScreen(int tileX, int tileY)
    {
        var screenX = (tileX - tileY) * (TileWidth / 2);
        var screenY = (tileX + tileY) * (TileHeight / 2);
        return new Vector2(screenX, screenY);
    }

    public static Vector2 TileToScreen(int tileX, int tileY, int elevation)
    {
        var screenX = (tileX - tileY) * (TileWidth / 2);
        var screenY = (tileX + tileY) * (TileHeight / 2) - elevation * HeightStep;
        return new Vector2(screenX, screenY);
    }

    public static Point ScreenToTile(Vector2 screenPos)
    {
        // The inverse of TileToScreen. Since TileToScreen returns the top-left
        // of the tile's 128x64 bounding box, we offset the screen position to
        // the diamond center before applying the inverse transform.
        var px = screenPos.X - TileWidth / 2f;
        var py = screenPos.Y - TileHeight / 2f;
        var halfW = TileWidth / 2f;
        var halfH = TileHeight / 2f;
        var fx = (px / halfW + py / halfH) / 2f;
        var fy = (py / halfH - px / halfW) / 2f;
        var tileX = (int)Math.Round(fx);
        var tileY = (int)Math.Round(fy);
        return new Point(tileX, tileY);
    }
}
