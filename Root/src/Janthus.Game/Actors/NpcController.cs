using Microsoft.Xna.Framework;
using Janthus.Model.Enums;
using Janthus.Game.Audio;
using Janthus.Game.World;

namespace Janthus.Game.Actors;

public class NpcController
{
    public ActorSprite Sprite { get; }
    private readonly ChunkManager _chunkManager;
    private readonly AudioManager _audioManager;
    private float _wanderTimer;
    private readonly Random _random = new();

    public NpcController(ActorSprite sprite, ChunkManager chunkManager, AudioManager audioManager)
    {
        Sprite = sprite;
        _chunkManager = chunkManager;
        _audioManager = audioManager;
        _wanderTimer = _random.Next(2, 6);
    }

    public void Update(GameTime gameTime, bool isInCombat = false, int playerTileX = 0, int playerTileY = 0)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Always update visual interpolation
        Sprite.UpdateVisual(deltaTime, 1.0f);

        // Skip wander logic when dead or in combat
        if (Sprite.DomainActor.Status == ActorStatus.Dead) return;
        if (isInCombat) return;

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
            var tileDist = (float)Math.Sqrt(
                Math.Pow(newX - playerTileX, 2) + Math.Pow(newY - playerTileY, 2));
            var stepTile = _chunkManager.GetTile(newX, newY);
            _audioManager.PlayFootstepAtDistance(stepTile?.Name ?? "Dirt", tileDist);
        }
    }
}
