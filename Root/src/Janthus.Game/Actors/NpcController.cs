using Microsoft.Xna.Framework;
using Janthus.Game.World;

namespace Janthus.Game.Actors;

public class NpcController
{
    public ActorSprite Sprite { get; }
    private readonly TileMap _tileMap;
    private float _wanderTimer;
    private readonly Random _random = new();

    public NpcController(ActorSprite sprite, TileMap tileMap)
    {
        Sprite = sprite;
        _tileMap = tileMap;
        _wanderTimer = _random.Next(2, 6);
    }

    public void Update(GameTime gameTime)
    {
        _wanderTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_wanderTimer > 0) return;

        _wanderTimer = _random.Next(2, 6);

        // Random wander: pick a random adjacent tile
        var dx = _random.Next(-1, 2);
        var dy = _random.Next(-1, 2);
        var newX = Sprite.TileX + dx;
        var newY = Sprite.TileY + dy;

        if (_tileMap.IsWalkable(newX, newY))
        {
            Sprite.TileX = newX;
            Sprite.TileY = newY;
        }
    }
}
