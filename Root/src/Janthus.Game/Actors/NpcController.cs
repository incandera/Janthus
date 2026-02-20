using Microsoft.Xna.Framework;
using Janthus.Game.World;

namespace Janthus.Game.Actors;

public class NpcController
{
    public ActorSprite Sprite { get; }
    private readonly ChunkManager _chunkManager;
    private float _wanderTimer;
    private readonly Random _random = new();

    public NpcController(ActorSprite sprite, ChunkManager chunkManager)
    {
        Sprite = sprite;
        _chunkManager = chunkManager;
        _wanderTimer = _random.Next(2, 6);
    }

    public void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Always update visual interpolation
        Sprite.UpdateVisual(deltaTime, 1.0f);

        _wanderTimer -= deltaTime;
        if (_wanderTimer > 0) return;

        _wanderTimer = _random.Next(2, 6);

        // Random wander: pick a random adjacent tile
        var dx = _random.Next(-1, 2);
        var dy = _random.Next(-1, 2);
        var newX = Sprite.TileX + dx;
        var newY = Sprite.TileY + dy;

        if (_chunkManager.IsWalkable(newX, newY))
        {
            Sprite.SetTilePosition(newX, newY, _chunkManager);
        }
    }
}
