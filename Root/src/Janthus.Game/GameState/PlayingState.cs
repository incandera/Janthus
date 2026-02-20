using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Janthus.Game.Input;
using Janthus.Game.World;
using Janthus.Game.Actors;
using Janthus.Game.UI;

namespace Janthus.Game.GameState;

public class PlayingState : IGameState
{
    private readonly JanthusGame _game;
    private readonly InputManager _input;
    private readonly TileMap _tileMap;
    private readonly IsometricRenderer _renderer;
    private readonly Camera _camera;
    private readonly PlayerController _playerController;
    private readonly List<NpcController> _npcControllers;
    private readonly UIManager _uiManager;
    private bool _paused;

    public bool IsPaused => _paused;
    public TileMap TileMap => _tileMap;
    public Camera Camera => _camera;
    public IsometricRenderer Renderer => _renderer;
    public PlayerController PlayerController => _playerController;
    public List<NpcController> NpcControllers => _npcControllers;
    public UIManager UIManager => _uiManager;

    public PlayingState(JanthusGame game, InputManager input, SpriteFont font,
                        TileMap tileMap, IsometricRenderer renderer, Camera camera,
                        PlayerController playerController, List<NpcController> npcControllers,
                        UIManager uiManager)
    {
        _game = game;
        _input = input;
        _tileMap = tileMap;
        _renderer = renderer;
        _camera = camera;
        _playerController = playerController;
        _npcControllers = npcControllers;
        _uiManager = uiManager;
    }

    public void Enter() { }
    public void Exit() { }

    public void TogglePause()
    {
        _paused = !_paused;
    }

    public void Update(GameTime gameTime)
    {
        if (_input.IsKeyPressed(Keys.Space) || _input.IsKeyPressed(Keys.P))
        {
            TogglePause();
        }

        if (_input.IsKeyPressed(Keys.Escape))
        {
            if (_uiManager.IsContextMenuVisible)
                _uiManager.CloseContextMenu();
            else
                _uiManager.TogglePauseMenu();
        }

        if (_input.IsKeyPressed(Keys.C))
        {
            _uiManager.ToggleCharacterPanel();
        }

        _uiManager.Update(gameTime, _input);

        if (_uiManager.ResumeRequested)
        {
            _uiManager.ResumeRequested = false;
            _paused = false;
            _uiManager.ClosePauseMenu();
        }

        if (_uiManager.QuitRequested)
        {
            _game.Exit();
            return;
        }

        if (!_paused)
        {
            // Right-click always works — closes any open menu and opens a new one
            HandleRightClick();

            // Left-click only when context menu didn't consume this frame's input
            if (!_uiManager.IsContextMenuVisible && !_uiManager.ContextMenuConsumedInput)
            {
                HandleLeftClick();
            }

            if (!_uiManager.IsContextMenuVisible && !_uiManager.ContextMenuConsumedInput)
                _playerController.Update(gameTime, _input);
            foreach (var npc in _npcControllers)
            {
                npc.Update(gameTime);
            }
            _camera.Follow(_playerController.Sprite.ScreenPosition, _renderer);
        }

        // Camera zoom always works
        if (_input.ScrollDelta != 0)
        {
            _camera.AdjustZoom(_input.ScrollDelta > 0 ? 0.1f : -0.1f);
        }
    }

    private void HandleLeftClick()
    {
        if (!_input.IsLeftClickPressed()) return;

        var worldPos = _camera.ScreenToWorld(_input.MousePosition.ToVector2());
        var tilePos = _renderer.ScreenToTile(worldPos);

        // Build list of all actor sprites for pathfinding (excluding player)
        var actorSprites = new List<ActorSprite>();
        foreach (var npc in _npcControllers)
            actorSprites.Add(npc.Sprite);

        var start = new Point(_playerController.Sprite.TileX, _playerController.Sprite.TileY);

        // Check if clicking an actor (hit-test against rendered bounds)
        var clickedNpc = FindNpcAtWorldPos(worldPos);
        if (clickedNpc != null)
        {
            // Walk to adjacent tile of the NPC's actual tile
            var npcTile = new Point(clickedNpc.TileX, clickedNpc.TileY);
            var path = Pathfinder.FindPathAdjacentTo(_tileMap, start, npcTile, actorSprites);
            if (path != null)
            {
                _playerController.ClearPath();
                _playerController.SetPath(path);
            }
            return;
        }

        // Walk to tile (walkable or nearest walkable)
        {
            var path = Pathfinder.FindPath(_tileMap, start, tilePos, actorSprites);
            if (path != null)
            {
                _playerController.ClearPath();
                _playerController.SetPath(path);
            }
        }
    }

    private void HandleRightClick()
    {
        if (!_input.IsRightClickPressed()) return;

        // Close any existing context menu before opening a new one
        if (_uiManager.IsContextMenuVisible)
            _uiManager.CloseContextMenu();

        var worldPos = _camera.ScreenToWorld(_input.MousePosition.ToVector2());
        var tilePos = _renderer.ScreenToTile(worldPos);
        var screenPos = _input.MousePosition;

        // Check if clicking an actor (hit-test against rendered bounds)
        var clickedNpc = FindNpcAtWorldPos(worldPos);

        if (clickedNpc != null)
        {
            if (clickedNpc.IsAdversary)
            {
                _uiManager.ShowContextMenu(screenPos,
                    new List<string> { "Inspect", "Attack" },
                    index =>
                    {
                        if (index == 1) // Attack
                        {
                            System.Console.WriteLine($"Attack {clickedNpc.Label}");
                        }
                    });
            }
            else
            {
                _uiManager.ShowContextMenu(screenPos,
                    new List<string> { "Inspect", "Talk", "Trade" },
                    index =>
                    {
                        // Placeholder — all items just close the menu
                    });
            }
        }
        else
        {
            _uiManager.ShowContextMenu(screenPos,
                new List<string> { "Inspect", "Move Here" },
                index =>
                {
                    if (index == 1) // Move Here
                    {
                        var actorSprites = new List<ActorSprite>();
                        foreach (var npc in _npcControllers)
                            actorSprites.Add(npc.Sprite);

                        var start = new Point(_playerController.Sprite.TileX, _playerController.Sprite.TileY);
                        var path = Pathfinder.FindPath(_tileMap, start, tilePos, actorSprites);
                        if (path != null)
                        {
                            _playerController.ClearPath();
                            _playerController.SetPath(path);
                        }
                    }
                });
        }
    }

    private ActorSprite FindNpcAtWorldPos(Vector2 worldPos)
    {
        // Hit-test against each NPC's rendered bounding rect in world space.
        // The actor body is a 16x24 rect drawn relative to TileToScreen:
        //   X: screenPos.X + TileWidth/2 - 8  ..  + 8   (width 16)
        //   Y: screenPos.Y - 24 + TileHeight/2  ..  screenPos.Y + TileHeight/2  (height 24)
        // We widen the hit area slightly for easier clicking.
        const int hitW = 24;
        const int hitH = 30;

        foreach (var npc in _npcControllers)
        {
            var screenPos = _renderer.TileToScreen(npc.Sprite.TileX, npc.Sprite.TileY);
            var cx = screenPos.X + IsometricRenderer.TileWidth / 2f;
            var bottom = screenPos.Y + IsometricRenderer.TileHeight / 2f;

            var hitRect = new Rectangle(
                (int)(cx - hitW / 2),
                (int)(bottom - hitH),
                hitW,
                hitH);

            if (hitRect.Contains((int)worldPos.X, (int)worldPos.Y))
                return npc.Sprite;
        }
        return null;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _renderer.DrawMap(spriteBatch, _tileMap, _camera);

        // Draw actors depth-sorted
        var allSprites = new List<ActorSprite> { _playerController.Sprite };
        foreach (var npc in _npcControllers)
        {
            allSprites.Add(npc.Sprite);
        }

        allSprites.Sort((a, b) =>
        {
            var depthA = a.TileX + a.TileY;
            var depthB = b.TileX + b.TileY;
            return depthA.CompareTo(depthB);
        });

        foreach (var sprite in allSprites)
        {
            var isPlayer = sprite == _playerController.Sprite;
            _renderer.DrawActor(spriteBatch, sprite, _camera, isPlayer);
        }

        _uiManager.Draw(spriteBatch);

        // Pause overlay
        if (_paused)
        {
            var viewport = spriteBatch.GraphicsDevice.Viewport;
            var pauseTexture = _game.PixelTexture;
            spriteBatch.Draw(pauseTexture,
                new Rectangle(0, 0, viewport.Width, viewport.Height),
                Color.Black * 0.3f);
        }
    }
}
