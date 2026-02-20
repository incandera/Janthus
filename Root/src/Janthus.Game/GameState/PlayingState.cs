using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Janthus.Model.Entities;
using Janthus.Model.Enums;
using Janthus.Model.Services;
using Janthus.Game.Input;
using Janthus.Game.World;
using Janthus.Game.Actors;
using Janthus.Game.Combat;
using Janthus.Game.Conversation;
using Janthus.Game.Saving;
using Janthus.Game.UI;

namespace Janthus.Game.GameState;

public class PlayingState : IGameState
{
    private readonly JanthusGame _game;
    private readonly InputManager _input;
    private readonly ChunkManager _chunkManager;
    private readonly IsometricRenderer _renderer;
    private readonly Camera _camera;
    private readonly PlayerController _playerController;
    private readonly List<NpcController> _npcControllers;
    private readonly UIManager _uiManager;
    private readonly ConversationRunner _conversationRunner;
    private readonly IGameDataProvider _dataProvider;
    private readonly CombatManager _combatManager;
    private bool _paused;
    private bool _gameOver;

    public bool IsPaused => _paused;
    public ChunkManager ChunkManager => _chunkManager;
    public Camera Camera => _camera;
    public IsometricRenderer Renderer => _renderer;
    public PlayerController PlayerController => _playerController;
    public List<NpcController> NpcControllers => _npcControllers;
    public UIManager UIManager => _uiManager;

    public PlayingState(JanthusGame game, InputManager input, SpriteFont font,
                        ChunkManager chunkManager, IsometricRenderer renderer, Camera camera,
                        PlayerController playerController, List<NpcController> npcControllers,
                        UIManager uiManager, ConversationRunner conversationRunner,
                        IGameDataProvider dataProvider, CombatManager combatManager)
    {
        _game = game;
        _input = input;
        _chunkManager = chunkManager;
        _renderer = renderer;
        _camera = camera;
        _playerController = playerController;
        _npcControllers = npcControllers;
        _uiManager = uiManager;
        _conversationRunner = conversationRunner;
        _dataProvider = dataProvider;
        _combatManager = combatManager;

        // Wire save/load callbacks
        _uiManager.OnSaveSlot = slot =>
        {
            var data = SaveManager.CaptureState(this, _dataProvider);
            SaveManager.SaveToSlot(slot, data);
        };
        _uiManager.OnLoadSlot = slot =>
        {
            var data = SaveManager.LoadFromSlot(slot);
            if (data != null)
                _game.StartFromSave(data);
        };
    }

    public void Enter() { }
    public void Exit() { }

    public void TogglePause()
    {
        _paused = !_paused;
    }

    public void Update(GameTime gameTime)
    {
        // Game over: only Escape to return to menu
        if (_gameOver)
        {
            if (_input.IsKeyPressed(Keys.Escape))
            {
                var menuState = new MenuState(_game, _input, _font);
                _game.StateManager.ChangeState(menuState);
            }
            return;
        }

        // Check for player death
        if (_playerController.Sprite.DomainActor.Status == ActorStatus.Dead && !_gameOver)
        {
            _gameOver = true;
            return;
        }

        if (_input.IsKeyPressed(Keys.Space) || _input.IsKeyPressed(Keys.P))
        {
            TogglePause();
        }

        if (_input.IsKeyPressed(Keys.Escape))
        {
            if (_uiManager.IsSaveLoadVisible)
                _uiManager.HideSaveLoadPanel();
            else if (_uiManager.IsTradeVisible)
                _uiManager.HideTrade();
            else if (_uiManager.IsDialogVisible)
                _uiManager.HideDialog();
            else if (_uiManager.IsContextMenuVisible)
                _uiManager.CloseContextMenu();
            else if (_uiManager.IsInventoryVisible)
                _uiManager.HideInventory();
            else
                _uiManager.TogglePauseMenu();
        }

        if (_input.IsKeyPressed(Keys.C))
        {
            _uiManager.ToggleCharacterPanel();
        }

        if (_input.IsKeyPressed(Keys.I) && !_uiManager.IsTradeVisible && !_uiManager.IsDialogVisible)
        {
            _uiManager.ToggleInventory();
        }

        _uiManager.Update(gameTime, _input);

        if (_uiManager.ResumeRequested)
        {
            _uiManager.ResumeRequested = false;
            _paused = false;
            _uiManager.ClosePauseMenu();
        }

        if (_uiManager.SaveRequested)
        {
            _uiManager.SaveRequested = false;
            _uiManager.ClosePauseMenu();
            _paused = false;
            _uiManager.ShowSavePanel();
        }

        if (_uiManager.LoadRequested)
        {
            _uiManager.LoadRequested = false;
            _uiManager.ClosePauseMenu();
            _paused = false;
            _uiManager.ShowLoadPanel();
        }

        if (_uiManager.QuitRequested)
        {
            _game.Exit();
            return;
        }

        if (!_paused)
        {
            // Block all game-world interaction when any menu panel is open
            if (!_uiManager.IsAnyMenuVisible)
            {
                // Right-click always works — closes any open menu and opens a new one
                HandleRightClick();

                // Left-click only when context menu didn't consume this frame's input
                if (!_uiManager.ContextMenuConsumedInput)
                {
                    HandleLeftClick();
                }

                if (!_uiManager.ContextMenuConsumedInput)
                    _playerController.Update(gameTime, _input);
            }
            if (!_uiManager.IsDialogVisible && !_uiManager.IsTradeVisible)
            {
                foreach (var npc in _npcControllers)
                {
                    npc.Update(gameTime, _combatManager.IsInCombat(npc.Sprite));
                }
            }

            // Update combat system
            _combatManager.Update(gameTime, _playerController.Sprite, _npcControllers);

            // Check for quest item pickups (Key of Stratholme → key_retrieved flag)
            CheckQuestItemFlags();

            // Update chunk loading based on player position
            _chunkManager.UpdatePlayerPosition(_playerController.Sprite.TileX, _playerController.Sprite.TileY);

            _camera.Follow(_playerController.Sprite.VisualPosition, _renderer);
        }

        // Camera zoom — only when no UI panel is consuming the scroll
        if (_input.ScrollDelta != 0 && !_uiManager.IsAnyMenuVisible)
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
            var path = Pathfinder.FindPathAdjacentTo(_chunkManager, start, npcTile, actorSprites);
            if (path != null)
            {
                _playerController.ClearPath();
                _playerController.SetPath(path);
            }
            return;
        }

        // Walk to tile (walkable or nearest walkable)
        {
            var path = Pathfinder.FindPath(_chunkManager, start, tilePos, actorSprites);
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
            if (clickedNpc.DomainActor.Status == ActorStatus.Dead)
            {
                // Dead NPCs — Inspect and Loot
                var deadNpc = clickedNpc;
                _uiManager.ShowContextMenu(screenPos,
                    new List<string> { "Inspect", "Loot" },
                    index =>
                    {
                        if (index == 1) // Loot
                        {
                            StartLoot(deadNpc);
                        }
                    });
            }
            else if (clickedNpc.IsAdversary)
            {
                _uiManager.ShowContextMenu(screenPos,
                    new List<string> { "Inspect", "Attack" },
                    index =>
                    {
                        if (index == 1) // Attack
                        {
                            _combatManager.InitiatePlayerAttack(_playerController.Sprite, clickedNpc);
                        }
                    });
            }
            else
            {
                var npcName = clickedNpc.Label;
                _uiManager.ShowContextMenu(screenPos,
                    new List<string> { "Inspect", "Talk", "Trade" },
                    index =>
                    {
                        if (index == 1) // Talk
                        {
                            StartConversation(npcName);
                        }
                        else if (index == 2) // Trade
                        {
                            StartTrade(npcName);
                        }
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
                        var path = Pathfinder.FindPath(_chunkManager, start, tilePos, actorSprites);
                        if (path != null)
                        {
                            _playerController.ClearPath();
                            _playerController.SetPath(path);
                        }
                    }
                });
        }
    }

    private void StartTrade(string npcName)
    {
        // Find the NPC controller for this NPC
        NpcController npcController = null;
        foreach (var npc in _npcControllers)
        {
            if (npc.Sprite.Label == npcName)
            {
                npcController = npc;
                break;
            }
        }

        if (npcController == null) return;

        var npcActor = npcController.Sprite.DomainActor as NonPlayerCharacter;
        if (npcActor == null) return;

        // Load merchant stock from DB
        var stockEntries = _dataProvider.GetMerchantStock(npcName);
        if (stockEntries.Count == 0)
        {
            _uiManager.ShowDialog(npcName, $"{npcName} has nothing to trade.",
                new List<string>(), null, isEndNode: true, onDismiss: () => { });
            return;
        }

        // Build merchant inventory from stock + any items already in NPC inventory
        var merchantInventory = new List<InventoryItem>();

        // Start with DB-defined stock
        foreach (var stock in stockEntries)
        {
            var item = _dataProvider.GetItem(stock.ItemId);
            if (item != null)
            {
                var existing = merchantInventory.Find(i => i.Item.Id == item.Id);
                if (existing != null)
                    existing.Quantity += stock.Quantity;
                else
                    merchantInventory.Add(new InventoryItem(item, stock.Quantity));
            }
        }

        // Merge in any items the NPC already has (from player selling)
        foreach (var inv in npcActor.Inventory)
        {
            var existing = merchantInventory.Find(i => i.Item.Id == inv.Item.Id);
            if (existing != null)
                existing.Quantity += inv.Quantity;
            else
                merchantInventory.Add(new InventoryItem(inv.Item, inv.Quantity));
        }

        // Sync NPC inventory to the merged list (so sells persist)
        npcActor.Inventory.Clear();
        foreach (var inv in merchantInventory)
            npcActor.Inventory.Add(new InventoryItem(inv.Item, inv.Quantity));

        var player = _playerController.Sprite.DomainActor as PlayerCharacter;
        _uiManager.ShowTrade(player, npcActor, npcActor.Inventory, _dataProvider);
    }

    private void StartLoot(ActorSprite deadNpcSprite)
    {
        var npcActor = deadNpcSprite.DomainActor as NonPlayerCharacter;
        if (npcActor == null) return;

        // Build corpse inventory from NPC inventory + equipped items
        var corpseInventory = new List<InventoryItem>();

        // Add equipped items to lootable inventory
        foreach (var kvp in npcActor.Equipment)
        {
            var existing = corpseInventory.Find(i => i.Item.Id == kvp.Value.Id);
            if (existing != null)
                existing.Quantity++;
            else
                corpseInventory.Add(new InventoryItem(kvp.Value));
        }
        npcActor.Equipment.Clear();

        // Add remaining inventory items
        foreach (var inv in npcActor.Inventory)
        {
            var existing = corpseInventory.Find(i => i.Item.Id == inv.Item.Id);
            if (existing != null)
                existing.Quantity += inv.Quantity;
            else
                corpseInventory.Add(new InventoryItem(inv.Item, inv.Quantity));
        }

        // Sync NPC inventory to the combined loot list
        npcActor.Inventory.Clear();
        foreach (var inv in corpseInventory)
            npcActor.Inventory.Add(new InventoryItem(inv.Item, inv.Quantity));

        if (npcActor.Inventory.Count == 0)
        {
            _uiManager.ShowDialog(deadNpcSprite.Label, $"{deadNpcSprite.Label} has nothing to loot.",
                new List<string>(), null, isEndNode: true, onDismiss: () => { });
            return;
        }

        var player = _playerController.Sprite.DomainActor as PlayerCharacter;
        _uiManager.ShowLoot(player, npcActor, npcActor.Inventory);
    }

    private void StartConversation(string npcName)
    {
        if (!_conversationRunner.TryStartConversation(npcName))
        {
            _uiManager.ShowDialog(npcName, $"{npcName} has nothing to say.",
                new List<string>(), null, isEndNode: true, onDismiss: () => { });
            return;
        }

        ShowCurrentNode();
    }

    private void ShowCurrentNode()
    {
        var runner = _conversationRunner;
        var responseTexts = new List<string>();
        foreach (var r in runner.AvailableResponses)
            responseTexts.Add(r.Text);

        _uiManager.ShowDialog(
            runner.SpeakerName,
            runner.Text,
            responseTexts,
            onSelect: index =>
            {
                var continues = runner.SelectResponse(index);
                if (continues)
                    ShowCurrentNode();
            },
            isEndNode: runner.IsEndNode,
            onDismiss: () =>
            {
                runner.EndConversation();
            });
    }

    private void CheckQuestItemFlags()
    {
        var player = _playerController.Sprite.DomainActor as PlayerCharacter;
        if (player == null) return;

        // Key of Stratholme → key_retrieved flag
        if (_dataProvider.GetGameFlag("key_retrieved") == null &&
            player.Inventory.Exists(i => i.Item.Name == "Key of Stratholme"))
        {
            _dataProvider.SetGameFlag("key_retrieved", "true");
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
            var screenPos = npc.Sprite.VisualPosition;
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
        _renderer.DrawMap(spriteBatch, _chunkManager, _camera);

        // Unified depth-sorted render list combining objects + actors
        var renderItems = new List<(int depth, Action draw)>();

        // Add actors
        var allSprites = new List<ActorSprite> { _playerController.Sprite };
        foreach (var npc in _npcControllers)
            allSprites.Add(npc.Sprite);

        foreach (var sprite in allSprites)
        {
            var s = sprite;
            var elev = _chunkManager.GetElevation(s.TileX, s.TileY);
            var depth = (s.TileX + s.TileY) * 100 - elev * 10;
            var isPlayer = s == _playerController.Sprite;
            renderItems.Add((depth, () => _renderer.DrawActor(spriteBatch, s, _chunkManager, _camera, isPlayer)));
        }

        // Add objects from loaded chunks (filtered by visible range)
        var (visMinX, visMinY, visMaxX, visMaxY) = _renderer.GetVisibleTileRange(_camera, _chunkManager.WorldWidth, _chunkManager.WorldHeight);
        foreach (var chunk in _chunkManager.LoadedChunks)
        {
            var worldOffsetX = chunk.ChunkX * chunk.Size;
            var worldOffsetY = chunk.ChunkY * chunk.Size;

            foreach (var obj in chunk.Objects)
            {
                var o = obj;
                var wx = worldOffsetX + o.LocalX;
                var wy = worldOffsetY + o.LocalY;
                if (wx < visMinX || wx > visMaxX || wy < visMinY || wy > visMaxY)
                    continue;
                var elev = chunk.GetElevation(o.LocalX, o.LocalY);
                var depth = (wx + wy) * 100 - elev * 10;
                renderItems.Add((depth, () => _renderer.DrawObject(spriteBatch, wx, wy, o.ObjectDefinitionId, elev)));
            }
        }

        renderItems.Sort((a, b) => a.depth.CompareTo(b.depth));
        foreach (var item in renderItems)
            item.draw();

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

        // Game over overlay
        if (_gameOver)
        {
            var viewport = spriteBatch.GraphicsDevice.Viewport;
            spriteBatch.Draw(_game.PixelTexture,
                new Rectangle(0, 0, viewport.Width, viewport.Height),
                Color.Black * 0.6f);

            var gameOverText = "YOU HAVE FALLEN";
            var subText = "Press Escape to return to menu";
            var font = _uiManager != null ? Font : null;
            if (font != null)
            {
                var goSize = font.MeasureString(gameOverText);
                var subSize = font.MeasureString(subText);
                spriteBatch.DrawString(font, gameOverText,
                    new Vector2(viewport.Width / 2 - goSize.X / 2, viewport.Height / 2 - goSize.Y),
                    Color.Red);
                spriteBatch.DrawString(font, subText,
                    new Vector2(viewport.Width / 2 - subSize.X / 2, viewport.Height / 2 + 10),
                    Color.Gray);
            }
        }
    }

    // Font accessor for game over text — gets font from any UIPanel via reflection or stored reference
    private SpriteFont _font;
    public SpriteFont Font
    {
        get => _font;
        set => _font = value;
    }
}
