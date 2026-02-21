using Microsoft.Xna.Framework.Graphics;

namespace Janthus.Game.Rendering;

public class CharacterSpriteSheet
{
    public Texture2D Texture { get; }
    private readonly Dictionary<AnimationType, SpriteAnimation> _animations = new();

    public CharacterSpriteSheet(Texture2D texture)
    {
        Texture = texture;
    }

    public void AddAnimation(AnimationType type, SpriteAnimation animation)
    {
        _animations[type] = animation;
    }

    public SpriteAnimation GetAnimation(AnimationType type)
    {
        _animations.TryGetValue(type, out var anim);
        return anim;
    }
}
