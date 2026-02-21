namespace Janthus.Game.Rendering;

public class ActorAnimator
{
    public AnimationType CurrentAnimation { get; private set; } = AnimationType.Idle;
    public int CurrentFrame { get; private set; }
    private float _elapsed;

    public void Play(AnimationType type)
    {
        if (type == CurrentAnimation) return;
        CurrentAnimation = type;
        CurrentFrame = 0;
        _elapsed = 0f;
    }

    public void Update(float deltaTime, SpriteAnimation animation = null)
    {
        if (animation == null) return;

        _elapsed += deltaTime;
        if (_elapsed >= animation.FrameDuration)
        {
            _elapsed -= animation.FrameDuration;
            CurrentFrame++;

            if (CurrentFrame >= animation.FrameCount)
            {
                CurrentFrame = animation.Loops ? 0 : animation.FrameCount - 1;
            }
        }
    }
}
