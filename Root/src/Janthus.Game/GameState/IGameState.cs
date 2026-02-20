using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Janthus.Game.GameState;

public interface IGameState
{
    void Enter();
    void Exit();
    void Update(GameTime gameTime);
    void Draw(SpriteBatch spriteBatch);
}
