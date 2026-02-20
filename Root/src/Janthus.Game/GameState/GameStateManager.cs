using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Janthus.Game.GameState;

public class GameStateManager
{
    private readonly Stack<IGameState> _states = new();

    public IGameState CurrentState => _states.Count > 0 ? _states.Peek() : null;

    public void PushState(IGameState state)
    {
        CurrentState?.Exit();
        _states.Push(state);
        state.Enter();
    }

    public void PopState()
    {
        if (_states.Count > 0)
        {
            var state = _states.Pop();
            state.Exit();
            CurrentState?.Enter();
        }
    }

    public void ChangeState(IGameState state)
    {
        while (_states.Count > 0)
        {
            _states.Pop().Exit();
        }
        _states.Push(state);
        state.Enter();
    }

    public void Update(GameTime gameTime) => CurrentState?.Update(gameTime);

    public void Draw(SpriteBatch spriteBatch) => CurrentState?.Draw(spriteBatch);
}
