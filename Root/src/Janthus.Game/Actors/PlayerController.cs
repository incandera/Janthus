using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Janthus.Game.Input;
using Janthus.Game.World;

namespace Janthus.Game.Actors;

public class PlayerController
{
    public ActorSprite Sprite { get; }
    private readonly TileMap _tileMap;
    private float _moveTimer;
    private const float MoveInterval = 0.15f;
    private List<Point> _path;

    public PlayerController(ActorSprite sprite, TileMap tileMap)
    {
        Sprite = sprite;
        _tileMap = tileMap;
    }

    public void SetPath(List<Point> path)
    {
        _path = path;
    }

    public void ClearPath()
    {
        _path = null;
    }

    public void Update(GameTime gameTime, InputManager input)
    {
        _moveTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_moveTimer > 0) return;

        var dx = 0;
        var dy = 0;

        if (input.IsKeyDown(Keys.W) || input.IsKeyDown(Keys.Up)) { dx--; dy--; }
        if (input.IsKeyDown(Keys.S) || input.IsKeyDown(Keys.Down)) { dx++; dy++; }
        if (input.IsKeyDown(Keys.A) || input.IsKeyDown(Keys.Left)) { dx--; dy++; }
        if (input.IsKeyDown(Keys.D) || input.IsKeyDown(Keys.Right)) { dx++; dy--; }

        // Keyboard takes priority — clears any active path
        if (dx != 0 || dy != 0)
        {
            _path = null;

            var newX = Sprite.TileX + dx;
            var newY = Sprite.TileY + dy;

            if (_tileMap.IsWalkable(newX, newY))
            {
                Sprite.TileX = newX;
                Sprite.TileY = newY;
                _moveTimer = MoveInterval;
            }
            return;
        }

        // Follow path if no keyboard input
        if (_path is { Count: > 0 })
        {
            var next = _path[0];
            _path.RemoveAt(0);

            if (_tileMap.IsWalkable(next.X, next.Y))
            {
                Sprite.TileX = next.X;
                Sprite.TileY = next.Y;
                _moveTimer = MoveInterval;
            }
            else
            {
                // Path blocked — cancel
                _path = null;
            }

            if (_path != null && _path.Count == 0)
                _path = null;
        }
    }
}
