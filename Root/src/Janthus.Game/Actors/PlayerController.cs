using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Janthus.Game.Audio;
using Janthus.Game.Input;
using Janthus.Game.World;

namespace Janthus.Game.Actors;

public class PlayerController
{
    public ActorSprite Sprite { get; }
    private readonly ChunkManager _chunkManager;
    private readonly AudioManager _audioManager;
    private float _moveTimer;
    private const float MoveInterval = 0.15f;
    private List<Point> _path;

    public PlayerController(ActorSprite sprite, ChunkManager chunkManager, AudioManager audioManager)
    {
        Sprite = sprite;
        _chunkManager = chunkManager;
        _audioManager = audioManager;
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
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Compute speed modifiers
        var tile = _chunkManager.GetTile(Sprite.TileX, Sprite.TileY);
        var terrainModifier = tile != null && tile.BaseMovementCost > 0 ? 1.0f / tile.BaseMovementCost : 1.0f;
        var dexterity = 5;
        if (Sprite.DomainActor is Model.Entities.LeveledActor leveled)
            dexterity = leveled.Dexterity.Value;
        var attributeModifier = 1.0f + (dexterity - 5) * 0.05f;
        var speedModifier = terrainModifier * attributeModifier;

        // Always update visual interpolation
        Sprite.UpdateVisual(deltaTime, speedModifier);

        // Only allow a new tile advance once the sprite has visually arrived
        if (!Sprite.HasReachedTarget) return;

        _moveTimer -= deltaTime;
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

            if (_chunkManager.IsWalkable(newX, newY))
            {
                Sprite.SetTilePosition(newX, newY, _chunkManager);
                _moveTimer = MoveInterval;
                var stepTile = _chunkManager.GetTile(newX, newY);
                _audioManager.PlayFootstep(stepTile?.Name ?? "Dirt");
            }
            return;
        }

        // Follow path if no keyboard input
        if (_path is { Count: > 0 })
        {
            var next = _path[0];
            _path.RemoveAt(0);

            if (_chunkManager.IsWalkable(next.X, next.Y))
            {
                Sprite.SetTilePosition(next.X, next.Y, _chunkManager);
                _moveTimer = MoveInterval;
                var stepTile = _chunkManager.GetTile(next.X, next.Y);
                _audioManager.PlayFootstep(stepTile?.Name ?? "Dirt");
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
