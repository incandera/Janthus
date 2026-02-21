using Microsoft.Xna.Framework;
using Janthus.Model.Enums;

namespace Janthus.Game.Rendering;

public enum AnimationType
{
    Idle,
    Walk,
    Attack,
    Death
}

public class SpriteAnimation
{
    public int FrameCount { get; }
    public float FrameDuration { get; }
    public int FrameWidth { get; }
    public int FrameHeight { get; }
    public bool Loops { get; }
    public int StartRow { get; }

    public SpriteAnimation(int frameCount, float frameDuration, int frameWidth, int frameHeight, bool loops, int startRow)
    {
        FrameCount = frameCount;
        FrameDuration = frameDuration;
        FrameWidth = frameWidth;
        FrameHeight = frameHeight;
        Loops = loops;
        StartRow = startRow;
    }

    public Rectangle GetSourceRect(FacingDirection facing, int frameIndex)
    {
        // Bellanger format: 8 rows per animation block (one per direction)
        var row = StartRow + (int)facing;
        var col = frameIndex % FrameCount;
        return new Rectangle(col * FrameWidth, row * FrameHeight, FrameWidth, FrameHeight);
    }
}
