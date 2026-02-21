using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.EntityFrameworkCore;
using Janthus.Model.Entities;
using Janthus.Model.Enums;
using Janthus.Model.Services;
using Janthus.Data;
using Janthus.Game.Input;
using Janthus.Game.GameState;
using Janthus.Game.Settings;
using Janthus.Game.World;
using Janthus.Game.Actors;
using Janthus.Game.Combat;
using Janthus.Game.Conversation;
using Janthus.Game.Rendering;
using Janthus.Game.Saving;
using Janthus.Game.UI;

namespace Janthus.Game;

public class JanthusGame : Microsoft.Xna.Framework.Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;
    private Texture2D _pixelTexture;

    private readonly InputManager _input = new();
    private readonly GameStateManager _stateManager = new();
    private readonly GameSettings _settings;

    private GameDataRepository _repository;
    private PlayingState _playingState;
    private AssetManager _assetManager;

    private RenderTarget2D _sceneTarget;
    private RenderTarget2D _lightmapTarget;

    private static readonly BlendState MultiplyBlend = new()
    {
        ColorSourceBlend = Blend.DestinationColor,
        ColorDestinationBlend = Blend.Zero,
        AlphaSourceBlend = Blend.DestinationAlpha,
        AlphaDestinationBlend = Blend.Zero
    };

    public static readonly (int Width, int Height)[] Resolutions =
    {
        (960, 540),
        (1280, 720),
        (1366, 768),
        (1600, 900),
        (1920, 1080),
        (2560, 1440),
        (3840, 2160),
        (7680, 4320),
    };
    private int _resolutionIndex = 1; // Default 1280x720

    public Texture2D PixelTexture => _pixelTexture;
    public GameStateManager StateManager => _stateManager;
    public int ResolutionIndex => _resolutionIndex;
    public bool IsFullScreen => _graphics.IsFullScreen;
    public int ResolutionCount => Resolutions.Length;
    public string CurrentResolutionLabel =>
        $"{Resolutions[_resolutionIndex].Width}x{Resolutions[_resolutionIndex].Height}" +
        (_graphics.IsFullScreen ? " (Fullscreen)" : " (Windowed)");

    public JanthusGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.Title = "Janthus";
        Window.AllowUserResizing = false;

        _settings = GameSettings.Load();
        _resolutionIndex = Math.Clamp(_settings.ResolutionIndex, 0, Resolutions.Length - 1);

        _graphics.PreferredBackBufferWidth = Resolutions[_resolutionIndex].Width;
        _graphics.PreferredBackBufferHeight = Resolutions[_resolutionIndex].Height;
        _graphics.IsFullScreen = _settings.IsFullScreen;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        _font = CreateDefaultFont();

        // Initialize database
        var options = new DbContextOptionsBuilder<JanthusDbContext>()
            .UseSqlite("Data Source=janthus_data.db")
            .Options;
        var context = new JanthusDbContext(options);
        context.Database.EnsureCreated();
        _repository = new GameDataRepository(context);

        // Create render targets for lightmap pipeline
        CreateRenderTargets();

        // Load sprite assets (graceful fallback if missing)
        _assetManager = new AssetManager();
        _assetManager.LoadAll(GraphicsDevice, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content"));

        // Start at menu
        var menuState = new MenuState(this, _input, _font);
        _stateManager.PushState(menuState);
    }

    private static readonly LawfulnessType[] LawfulnessValues = Enum.GetValues<LawfulnessType>();
    private static readonly DispositionType[] DispositionValues = Enum.GetValues<DispositionType>();

    private static Alignment RandomAlignment(Random rng)
    {
        return new Alignment(
            LawfulnessValues[rng.Next(LawfulnessValues.Length)],
            DispositionValues[rng.Next(DispositionValues.Length)]);
    }

    private (ChunkManager chunkManager, IsometricRenderer renderer, Camera camera) SetupWorld()
    {
        var tileRegistry = new TileRegistry(_repository);
        var worldMap = _repository.GetWorldMap("Default");

        var existingChunks = _repository.GetChunksForWorld(worldMap.Id);
        if (existingChunks.Count == 0)
        {
            WorldGenerator.Generate(worldMap, _repository);
        }

        var chunkManager = new ChunkManager(worldMap, tileRegistry, _repository);
        for (int cy = 0; cy < worldMap.ChunkCountY; cy++)
        {
            for (int cx = 0; cx < worldMap.ChunkCountX; cx++)
            {
                chunkManager.LoadChunk(cx, cy);
            }
        }

        var renderer = new IsometricRenderer(_pixelTexture);
        renderer.SetFont(_font);
        if (_assetManager != null)
        {
            if (_assetManager.TileAtlas != null) renderer.SetTileAtlas(_assetManager.TileAtlas);
            if (_assetManager.ObjectAtlas != null) renderer.SetObjectAtlas(_assetManager.ObjectAtlas);
        }
        var camera = new Camera(GraphicsDevice.Viewport);

        return (chunkManager, renderer, camera);
    }

    public void StartPlaying()
    {
        // Create player character with Lawful Good alignment (ensures Guard is friendly)
        var playerAlignment = new Alignment(LawfulnessType.Lawful, DispositionType.Good);
        var player = new PlayerCharacter
        {
            Name = "Hero",
            Alignment = playerAlignment
        };
        player.Constitution.Value = 6;
        player.Strength.Value = 5;
        player.Dexterity.Value = 4;
        player.Intelligence.Value = 4;
        player.Willpower.Value = 5;
        player.Attunement.Value = 3;
        player.Luck.Value = 3;
        player.CurrentHitPoints = (decimal)player.MaximumHitPoints;
        player.CurrentMana = (decimal)player.MaximumMana;

        var (chunkManager, renderer, camera) = SetupWorld();

        // Place player near center, offset from the pond
        var worldCenterX = chunkManager.WorldWidth / 2;
        var worldCenterY = chunkManager.WorldHeight / 2;
        var playerStartX = worldCenterX + 8;
        var playerStartY = worldCenterY + 8;

        // Validate spawn — BFS to nearest walkable tile if blocked
        if (!chunkManager.IsWalkable(playerStartX, playerStartY))
        {
            var validSpawn = FindNearestWalkableTile(chunkManager, playerStartX, playerStartY);
            playerStartX = validSpawn.X;
            playerStartY = validSpawn.Y;
        }

        var playerSprite = new ActorSprite(player, playerStartX, playerStartY, Color.Cyan, "Hero");
        if (_assetManager?.PlayerSheet != null) playerSprite.SpriteSheet = _assetManager.PlayerSheet;
        playerSprite.SnapVisualToTile(chunkManager);
        var playerController = new PlayerController(playerSprite, chunkManager);

        // Create scenario NPCs
        var npcControllers = new List<NpcController>();

        // NPC definitions: (position, name, color, alignment, stats[7], isScripted)
        var npcDefs = new (
            (int x, int y) pos,
            string name,
            Color color,
            Alignment alignment,
            int[] stats // Con, Dex, Int, Luck, Att, Str, Will
        )[]
        {
            // Guard — nearby, friendly
            ((worldCenterX - 5, worldCenterY - 5), "Guard", Color.Orange,
                new Alignment(LawfulnessType.Lawful, DispositionType.Good),
                new[] { 5, 4, 3, 3, 3, 5, 4 }),

            // Merchant — nearby, neutral
            ((worldCenterX + 10, worldCenterY - 3), "Merchant", Color.Yellow,
                new Alignment(LawfulnessType.Neutral, DispositionType.Neutral),
                new[] { 3, 3, 5, 4, 3, 3, 4 }),

            // Mage — to the east, wounded
            ((worldCenterX + 20, worldCenterY + 5), "Mage", Color.Purple,
                new Alignment(LawfulnessType.Neutral, DispositionType.Good),
                new[] { 3, 3, 7, 4, 6, 3, 5 }),

            // Mercenary Captain — further out, strong adversary, carries Key
            ((worldCenterX + 28, worldCenterY + 15), "Mercenary", Color.Red,
                new Alignment(LawfulnessType.Chaotic, DispositionType.Evil),
                new[] { 7, 5, 3, 4, 3, 7, 5 }),

            // Bandit — near the Captain
            ((worldCenterX + 25, worldCenterY + 20), "Bandit", Color.DarkRed,
                new Alignment(LawfulnessType.Chaotic, DispositionType.Evil),
                new[] { 6, 6, 3, 3, 3, 6, 4 }),
        };

        foreach (var def in npcDefs)
        {
            var npc = new NonPlayerCharacter(
                def.stats[0], def.stats[1], def.stats[2], def.stats[3],
                def.stats[4], def.stats[5], def.stats[6],
                def.alignment)
            {
                Name = def.name
            };
            npc.CurrentHitPoints = (decimal)npc.MaximumHitPoints;
            npc.CurrentMana = (decimal)npc.MaximumMana;

            // Mage starts wounded (half HP)
            if (def.name == "Mage")
                npc.CurrentHitPoints = Math.Ceiling(npc.CurrentHitPoints / 2);

            // Validate NPC spawn position
            var npcX = def.pos.x;
            var npcY = def.pos.y;
            if (!chunkManager.IsWalkable(npcX, npcY))
            {
                var validPos = FindNearestWalkableTile(chunkManager, npcX, npcY);
                npcX = validPos.X;
                npcY = validPos.Y;
            }

            var npcSprite = new ActorSprite(npc, npcX, npcY, def.color, def.name);
            if (_assetManager?.DefaultNpcSheet != null) npcSprite.SpriteSheet = _assetManager.DefaultNpcSheet;
            npcSprite.SnapVisualToTile(chunkManager);

            // Calculate adversary relationship — force adversary for scenario combatants
            if (def.name == "Mercenary" || def.name == "Bandit")
            {
                npcSprite.IsAdversary = true;
            }
            else
            {
                var seed = player.Name.GetHashCode() ^ npc.Name.GetHashCode()
                           ^ def.pos.x ^ (def.pos.y << 16);
                npcSprite.IsAdversary = AdversaryCalculator.IsAdversary(playerAlignment, def.alignment, seed);
            }

            npcControllers.Add(new NpcController(npcSprite, chunkManager));
        }

        // Give player starting gold and equipment
        player.Gold = 500;
        var startingSword = _repository.GetItemByName("Short Sword");
        if (startingSword != null)
        {
            player.Inventory.Add(new InventoryItem(startingSword));
            CombatCalculator.Equip(player, player.Inventory, startingSword);
        }
        var startingArmor = _repository.GetItemByName("Leather Armor");
        if (startingArmor != null)
        {
            player.Inventory.Add(new InventoryItem(startingArmor));
            CombatCalculator.Equip(player, player.Inventory, startingArmor);
        }

        // Give Merchant NPC starting gold
        // Equip NPCs based on their roles
        var combatSkillType = _repository.GetSkillTypes().Find(s => s.Name == "Combat");
        var apprenticeSkillLevel = _repository.GetSkillLevels().Find(s => s.Name == "Apprentice");

        foreach (var npcCtrl in npcControllers)
        {
            var npcActor = npcCtrl.Sprite.DomainActor as NonPlayerCharacter;
            if (npcActor == null) continue;

            switch (npcCtrl.Sprite.Label)
            {
                case "Merchant":
                    npcActor.Gold = 1000;
                    break;

                case "Mercenary":
                    // Mercenary Captain: Mace + Iron Cuirass + Leather Helmet + Key of Stratholme
                    EquipNpc(npcActor, "Mace");
                    EquipNpc(npcActor, "Iron Cuirass");
                    EquipNpc(npcActor, "Leather Helmet");
                    EquipNpc(npcActor, "Iron Gauntlets");
                    GiveNpcItem(npcActor, "Key of Stratholme");
                    if (combatSkillType != null && apprenticeSkillLevel != null)
                        npcActor.Skills.Add(new Skill { Id = 1, Type = combatSkillType, Level = apprenticeSkillLevel });
                    npcActor.Gold = 50;
                    break;

                case "Bandit":
                    // Bandit: Short Sword + Leather Armor + Leather Boots
                    EquipNpc(npcActor, "Short Sword");
                    EquipNpc(npcActor, "Leather Armor");
                    EquipNpc(npcActor, "Leather Boots");
                    if (combatSkillType != null && apprenticeSkillLevel != null)
                        npcActor.Skills.Add(new Skill { Id = 1, Type = combatSkillType, Level = apprenticeSkillLevel });
                    npcActor.Gold = 25;
                    break;
            }
        }

        // Create combat manager
        var combatManager = new CombatManager(_repository);

        // Create UI
        var uiManager = new UIManager(_pixelTexture, _font, player, GraphicsDevice.Viewport, combatManager);

        // Create conversation runner
        var conversationRunner = new ConversationRunner(_repository, player, "Soldier");

        // Create playing state
        _playingState = new PlayingState(this, _input, _font, chunkManager, renderer, camera,
            playerController, npcControllers, uiManager, conversationRunner, _repository, combatManager);
        _playingState.Font = _font;

        // Center camera on player
        var playerScreen = renderer.TileToScreen(playerStartX, playerStartY);
        camera.Position = playerScreen;

        _stateManager.ChangeState(_playingState);
    }

    public void StartFromSave(GameSaveData saveData)
    {
        var (chunkManager, renderer, camera) = SetupWorld();

        // Clear existing game flags and restore saved ones
        _repository.ClearAllGameFlags();
        foreach (var flag in saveData.GameFlags)
        {
            _repository.SetGameFlag(flag.Name, flag.Value);
        }

        // Restore player
        var playerData = saveData.Player;
        var playerAlignment = new Alignment(
            Enum.Parse<LawfulnessType>(playerData.Lawfulness),
            Enum.Parse<DispositionType>(playerData.Disposition));
        var player = new PlayerCharacter
        {
            Name = playerData.Name,
            Alignment = playerAlignment
        };
        player.Constitution.Value = playerData.Constitution;
        player.Dexterity.Value = playerData.Dexterity;
        player.Intelligence.Value = playerData.Intelligence;
        player.Luck.Value = playerData.Luck;
        player.Attunement.Value = playerData.Attunement;
        player.Strength.Value = playerData.Strength;
        player.Willpower.Value = playerData.Willpower;
        player.CurrentHitPoints = playerData.CurrentHitPoints;
        player.CurrentMana = playerData.CurrentMana;
        player.Gold = playerData.Gold;
        player.Status = Enum.Parse<ActorStatus>(playerData.Status);

        // Restore player inventory
        foreach (var inv in playerData.Inventory)
        {
            var item = _repository.GetItem(inv.ItemId);
            if (item != null)
                player.Inventory.Add(new InventoryItem(item, inv.Quantity));
        }

        // Restore player equipment
        foreach (var eq in playerData.Equipment)
        {
            var item = _repository.GetItem(eq.ItemId);
            if (item != null)
                CombatCalculator.Equip(player, player.Inventory, item);
        }

        // Restore player skills
        RestoreSkills(player.Skills, playerData.Skills);

        var playerColor = new Color { PackedValue = playerData.Color };
        var playerSprite = new ActorSprite(player, playerData.TileX, playerData.TileY, playerColor, playerData.Name);
        playerSprite.Facing = (FacingDirection)playerData.Facing;
        if (_assetManager?.PlayerSheet != null) playerSprite.SpriteSheet = _assetManager.PlayerSheet;
        playerSprite.SnapVisualToTile(chunkManager);
        var playerController = new PlayerController(playerSprite, chunkManager);

        // Restore NPCs
        var npcControllers = new List<NpcController>();
        foreach (var npcData in saveData.Npcs)
        {
            var npcAlignment = new Alignment(
                Enum.Parse<LawfulnessType>(npcData.Lawfulness),
                Enum.Parse<DispositionType>(npcData.Disposition));
            var npc = new NonPlayerCharacter(
                npcData.Constitution, npcData.Dexterity, npcData.Intelligence,
                npcData.Luck, npcData.Attunement, npcData.Strength, npcData.Willpower,
                npcAlignment)
            {
                Name = npcData.Name
            };
            npc.CurrentHitPoints = npcData.CurrentHitPoints;
            npc.CurrentMana = npcData.CurrentMana;
            npc.Gold = npcData.Gold;
            npc.Status = Enum.Parse<ActorStatus>(npcData.Status);

            // Restore NPC inventory
            foreach (var inv in npcData.Inventory)
            {
                var item = _repository.GetItem(inv.ItemId);
                if (item != null)
                    npc.Inventory.Add(new InventoryItem(item, inv.Quantity));
            }

            // Restore NPC equipment
            foreach (var eq in npcData.Equipment)
            {
                var item = _repository.GetItem(eq.ItemId);
                if (item != null)
                    CombatCalculator.Equip(npc, npc.Inventory, item);
            }

            // Restore NPC skills
            RestoreSkills(npc.Skills, npcData.Skills);

            var npcColor = new Color { PackedValue = npcData.Color };
            var npcSprite = new ActorSprite(npc, npcData.TileX, npcData.TileY, npcColor, npcData.Name);
            npcSprite.IsAdversary = npcData.IsAdversary;
            npcSprite.Facing = (FacingDirection)npcData.Facing;
            if (_assetManager?.DefaultNpcSheet != null) npcSprite.SpriteSheet = _assetManager.DefaultNpcSheet;
            npcSprite.SnapVisualToTile(chunkManager);
            npcControllers.Add(new NpcController(npcSprite, chunkManager));
        }

        // Restore camera
        camera.Position = new Vector2(saveData.Camera.X, saveData.Camera.Y);
        camera.Zoom = saveData.Camera.Zoom;

        // Create combat manager
        var combatManager = new CombatManager(_repository);

        // Create UI
        var uiManager = new UIManager(_pixelTexture, _font, player, GraphicsDevice.Viewport, combatManager);

        // Create conversation runner
        var conversationRunner = new ConversationRunner(_repository, player, "Soldier");

        // Create playing state
        _playingState = new PlayingState(this, _input, _font, chunkManager, renderer, camera,
            playerController, npcControllers, uiManager, conversationRunner, _repository, combatManager);
        _playingState.Font = _font;

        // Restore time of day
        _playingState.SetTimeOfDay(saveData.TimeOfDay);

        // Restore visibility
        if (saveData.ChunkVisibility != null && _playingState.Visibility != null)
        {
            foreach (var kvp in saveData.ChunkVisibility)
            {
                var parts = kvp.Key.Split(',');
                if (parts.Length == 2 && int.TryParse(parts[0], out var cx) && int.TryParse(parts[1], out var cy))
                {
                    _playingState.Visibility.LoadChunkVisibility(cx, cy, chunkManager.ChunkSize, kvp.Value);
                }
            }
        }

        _stateManager.ChangeState(_playingState);
    }

    private void RestoreSkills(List<Skill> targetSkills, List<SkillSaveData> savedSkills)
    {
        var skillTypes = _repository.GetSkillTypes();
        var skillLevels = _repository.GetSkillLevels();
        foreach (var saved in savedSkills)
        {
            var type = skillTypes.Find(t => t.Id == saved.SkillTypeId);
            var level = skillLevels.Find(l => l.Id == saved.SkillLevelId);
            if (type != null && level != null)
                targetSkills.Add(new Skill { Id = targetSkills.Count + 1, Type = type, Level = level });
        }
    }

    private void EquipNpc(NonPlayerCharacter npc, string itemName)
    {
        var item = _repository.GetItemByName(itemName);
        if (item == null) return;
        npc.Inventory.Add(new InventoryItem(item));
        CombatCalculator.Equip(npc, npc.Inventory, item);
    }

    private void GiveNpcItem(NonPlayerCharacter npc, string itemName)
    {
        var item = _repository.GetItemByName(itemName);
        if (item == null) return;
        npc.Inventory.Add(new InventoryItem(item));
    }

    private static Point FindNearestWalkableTile(ChunkManager chunkManager, int startX, int startY)
    {
        var visited = new HashSet<Point> { new Point(startX, startY) };
        var queue = new Queue<Point>();
        queue.Enqueue(new Point(startX, startY));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    var neighbor = new Point(current.X + dx, current.Y + dy);
                    if (!visited.Add(neighbor)) continue;
                    if (!chunkManager.IsInBounds(neighbor.X, neighbor.Y)) continue;

                    if (chunkManager.IsWalkable(neighbor.X, neighbor.Y))
                        return neighbor;

                    queue.Enqueue(neighbor);
                }
            }
        }

        return new Point(startX, startY);
    }

    private void CreateRenderTargets()
    {
        _sceneTarget?.Dispose();
        _lightmapTarget?.Dispose();
        var vp = GraphicsDevice.Viewport;
        _sceneTarget = new RenderTarget2D(GraphicsDevice, vp.Width, vp.Height);
        _lightmapTarget = new RenderTarget2D(GraphicsDevice, vp.Width, vp.Height);
    }

    private void ApplyResolution()
    {
        var (width, height) = Resolutions[_resolutionIndex];
        _graphics.PreferredBackBufferWidth = width;
        _graphics.PreferredBackBufferHeight = height;
        _graphics.ApplyChanges();
        CreateRenderTargets();

        Window.Title = $"Janthus - {CurrentResolutionLabel}";

        if (_playingState != null)
        {
            _playingState.UIManager.UpdateLayout(GraphicsDevice.Viewport);
            _playingState.Camera.UpdateViewport(GraphicsDevice.Viewport);
        }
    }

    private void SaveSettings()
    {
        _settings.ResolutionIndex = _resolutionIndex;
        _settings.IsFullScreen = _graphics.IsFullScreen;
        _settings.Save();
    }

    private void CycleResolution(int direction)
    {
        _resolutionIndex = (_resolutionIndex + direction + Resolutions.Length) % Resolutions.Length;
        ApplyResolution();
        SaveSettings();
    }

    public void SetResolution(int index)
    {
        _resolutionIndex = Math.Clamp(index, 0, Resolutions.Length - 1);
        ApplyResolution();
    }

    public void SetFullScreen(bool value)
    {
        _graphics.IsFullScreen = value;
        _graphics.ApplyChanges();
        CreateRenderTargets();

        Window.Title = $"Janthus - {CurrentResolutionLabel}";

        if (_playingState != null)
        {
            _playingState.UIManager.UpdateLayout(GraphicsDevice.Viewport);
            _playingState.Camera.UpdateViewport(GraphicsDevice.Viewport);
        }
    }

    private void ToggleFullScreen()
    {
        _graphics.IsFullScreen = !_graphics.IsFullScreen;
        _graphics.ApplyChanges();
        CreateRenderTargets();

        Window.Title = $"Janthus - {CurrentResolutionLabel}";

        if (_playingState != null)
        {
            _playingState.UIManager.UpdateLayout(GraphicsDevice.Viewport);
            _playingState.Camera.UpdateViewport(GraphicsDevice.Viewport);
        }

        SaveSettings();
    }

    protected override void Update(GameTime gameTime)
    {
        _input.Update();

        // ALT+Enter toggles fullscreen
        if (_input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Enter) &&
            (_input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftAlt) ||
             _input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightAlt)))
        {
            ToggleFullScreen();
        }

        // F11 cycles resolution up, Shift+F11 cycles down
        if (_input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.F11))
        {
            if (_input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) ||
                _input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift))
                CycleResolution(-1);
            else
                CycleResolution(1);
        }

        // Update pause indicator on HUD
        if (_playingState != null)
        {
            _playingState.UIManager.SetPaused(_playingState.IsPaused);
        }

        _stateManager.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        if (_stateManager.CurrentState is PlayingState playing)
        {
            var renderer = playing.Renderer;
            var cm = playing.ChunkManager;
            var vp = GraphicsDevice.Viewport;

            playing.Camera.Follow(playing.PlayerController.Sprite.VisualPosition, renderer);

            // Phase 1: Scene → _sceneTarget
            GraphicsDevice.SetRenderTarget(_sceneTarget);
            GraphicsDevice.Clear(new Color(15, 15, 25));

            _spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null, null, null,
                playing.Camera.GetTransformMatrix());

            renderer.DrawMap(_spriteBatch, cm, playing.Camera, playing.Visibility);

            var renderItems = new List<(int depth, Action draw)>();

            var allSprites = new List<ActorSprite> { playing.PlayerController.Sprite };
            foreach (var npc in playing.NpcControllers)
                allSprites.Add(npc.Sprite);

            foreach (var sprite in allSprites)
            {
                var s = sprite;

                // Skip actors on unexplored tiles
                if (playing.Visibility != null)
                {
                    var vis = playing.Visibility.GetVisibility(s.TileX, s.TileY);
                    if (vis == TileVisibility.Unexplored) continue;
                    if (vis == TileVisibility.Explored && s != playing.PlayerController.Sprite) continue;
                }

                var elev = cm.GetElevation(s.TileX, s.TileY);
                var depth = RenderConstants.CalculateDepth(s.TileX, s.TileY, elev);
                var isPlayer = s == playing.PlayerController.Sprite;
                renderItems.Add((depth, () => renderer.DrawActor(_spriteBatch, s, cm, playing.Camera, isPlayer)));
            }

            var (visMinX, visMinY, visMaxX, visMaxY) = renderer.GetVisibleTileRange(playing.Camera, cm.WorldWidth, cm.WorldHeight);
            foreach (var chunk in cm.LoadedChunks)
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

                    if (playing.Visibility != null)
                    {
                        var vis = playing.Visibility.GetVisibility(wx, wy);
                        if (vis == TileVisibility.Unexplored) continue;
                    }

                    var elev = chunk.GetElevation(o.LocalX, o.LocalY);
                    var depth = RenderConstants.CalculateDepth(wx, wy, elev);
                    renderItems.Add((depth, () => renderer.DrawObject(_spriteBatch, wx, wy, o.ObjectDefinitionId, elev)));
                }
            }

            renderItems.Sort((a, b) => a.depth.CompareTo(b.depth));
            foreach (var item in renderItems)
                item.draw();

            _spriteBatch.End();

            // Phase 2: Lightmap → _lightmapTarget
            GraphicsDevice.SetRenderTarget(_lightmapTarget);
            if (playing.LightmapRenderer != null)
            {
                playing.LightmapRenderer.Draw(_spriteBatch, playing.Camera, vp, playing.Lights);
            }
            else
            {
                GraphicsDevice.Clear(Color.White);
            }

            // Phase 3: Composite → backbuffer
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(new Color(15, 15, 25));

            // Draw scene
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp);
            _spriteBatch.Draw(_sceneTarget, Vector2.Zero, Color.White);
            _spriteBatch.End();

            // Multiply lightmap over scene
            _spriteBatch.Begin(SpriteSortMode.Deferred, MultiplyBlend, SamplerState.PointClamp);
            _spriteBatch.Draw(_lightmapTarget, Vector2.Zero, Color.White);
            _spriteBatch.End();

            // Phase 4: UI → backbuffer (not affected by lighting)
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            playing.UIManager.Draw(_spriteBatch);

            if (playing.IsPaused)
            {
                _spriteBatch.Draw(_pixelTexture,
                    new Rectangle(0, 0, vp.Width, vp.Height),
                    Color.Black * 0.3f);
            }

            _spriteBatch.End();
        }
        else
        {
            GraphicsDevice.Clear(new Color(15, 15, 25));
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            _stateManager.Draw(_spriteBatch);
            _spriteBatch.End();
        }

        base.Draw(gameTime);
    }

    private SpriteFont CreateDefaultFont()
    {
        // Create a basic runtime font using MonoGame's built-in capabilities
        // We'll generate a simple bitmap font from a texture atlas
        return BuildRuntimeFont();
    }

    private SpriteFont BuildRuntimeFont()
    {
        // Build a simple monospace bitmap font at runtime
        var charWidth = 8;
        var charHeight = 14;
        var chars = new List<char>();
        var glyphBounds = new List<Rectangle>();
        var croppings = new List<Rectangle>();
        var kerning = new List<Vector3>();

        // ASCII printable characters 32-126
        for (char c = (char)32; c <= (char)126; c++)
        {
            chars.Add(c);
        }

        var columns = 16;
        var rows = (int)Math.Ceiling(chars.Count / (float)columns);
        var textureWidth = columns * charWidth;
        var textureHeight = rows * charHeight;

        var fontTexture = new Texture2D(GraphicsDevice, textureWidth, textureHeight);
        var pixels = new Color[textureWidth * textureHeight];

        // Generate simple pixel font glyphs
        for (int i = 0; i < chars.Count; i++)
        {
            var col = i % columns;
            var row = i / columns;
            var gx = col * charWidth;
            var gy = row * charHeight;

            DrawCharGlyph(pixels, textureWidth, gx, gy, charWidth, charHeight, chars[i]);

            glyphBounds.Add(new Rectangle(gx, gy, charWidth, charHeight));
            croppings.Add(Rectangle.Empty);
            kerning.Add(new Vector3(0, charWidth, 1));
        }

        fontTexture.SetData(pixels);

        return new SpriteFont(fontTexture, glyphBounds, croppings, chars, charHeight, 0, kerning, '?');
    }

    private void DrawCharGlyph(Color[] pixels, int texWidth, int gx, int gy, int w, int h, char c)
    {
        // Simple 8x14 pixel font rendering for basic ASCII
        var glyphData = GetGlyphPattern(c);
        if (glyphData == null) return;

        for (int row = 0; row < Math.Min(glyphData.Length, h); row++)
        {
            var bits = glyphData[row];
            for (int col = 0; col < w; col++)
            {
                if ((bits & (1 << (7 - col))) != 0)
                {
                    var px = gx + col;
                    var py = gy + row;
                    if (px < texWidth && py < (pixels.Length / texWidth))
                        pixels[py * texWidth + px] = Color.White;
                }
            }
        }
    }

    private byte[] GetGlyphPattern(char c)
    {
        // Minimal 8x14 bitmap font patterns for essential characters
        // Each byte represents 8 horizontal pixels (MSB = leftmost)
        return c switch
        {
            ' ' => new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            '!' => new byte[] { 0, 0, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x00, 0x18, 0x18, 0, 0, 0 },
            '"' => new byte[] { 0, 0x66, 0x66, 0x66, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            '\'' => new byte[] { 0, 0x18, 0x18, 0x18, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            '#' => new byte[] { 0, 0, 0x6C, 0x6C, 0xFE, 0x6C, 0x6C, 0xFE, 0x6C, 0x6C, 0, 0, 0, 0 },
            '(' => new byte[] { 0, 0x0C, 0x18, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x18, 0x0C, 0, 0, 0 },
            ')' => new byte[] { 0, 0x30, 0x18, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x18, 0x30, 0, 0, 0 },
            '+' => new byte[] { 0, 0, 0, 0, 0x18, 0x18, 0x7E, 0x18, 0x18, 0, 0, 0, 0, 0 },
            ',' => new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x18, 0x18, 0x30, 0, 0 },
            '-' => new byte[] { 0, 0, 0, 0, 0, 0, 0x7E, 0, 0, 0, 0, 0, 0, 0 },
            '.' => new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x18, 0x18, 0, 0, 0 },
            '/' => new byte[] { 0, 0x06, 0x06, 0x0C, 0x0C, 0x18, 0x18, 0x30, 0x30, 0x60, 0x60, 0, 0, 0 },
            '0' => new byte[] { 0, 0, 0x3C, 0x66, 0x6E, 0x76, 0x66, 0x66, 0x66, 0x3C, 0, 0, 0, 0 },
            '1' => new byte[] { 0, 0, 0x18, 0x38, 0x18, 0x18, 0x18, 0x18, 0x18, 0x7E, 0, 0, 0, 0 },
            '2' => new byte[] { 0, 0, 0x3C, 0x66, 0x06, 0x0C, 0x18, 0x30, 0x60, 0x7E, 0, 0, 0, 0 },
            '3' => new byte[] { 0, 0, 0x3C, 0x66, 0x06, 0x1C, 0x06, 0x06, 0x66, 0x3C, 0, 0, 0, 0 },
            '4' => new byte[] { 0, 0, 0x0C, 0x1C, 0x3C, 0x6C, 0x7E, 0x0C, 0x0C, 0x0C, 0, 0, 0, 0 },
            '5' => new byte[] { 0, 0, 0x7E, 0x60, 0x7C, 0x06, 0x06, 0x06, 0x66, 0x3C, 0, 0, 0, 0 },
            '6' => new byte[] { 0, 0, 0x3C, 0x60, 0x60, 0x7C, 0x66, 0x66, 0x66, 0x3C, 0, 0, 0, 0 },
            '7' => new byte[] { 0, 0, 0x7E, 0x06, 0x0C, 0x18, 0x18, 0x18, 0x18, 0x18, 0, 0, 0, 0 },
            '8' => new byte[] { 0, 0, 0x3C, 0x66, 0x66, 0x3C, 0x66, 0x66, 0x66, 0x3C, 0, 0, 0, 0 },
            '9' => new byte[] { 0, 0, 0x3C, 0x66, 0x66, 0x3E, 0x06, 0x06, 0x0C, 0x38, 0, 0, 0, 0 },
            ':' => new byte[] { 0, 0, 0, 0, 0x18, 0x18, 0, 0, 0x18, 0x18, 0, 0, 0, 0 },
            '=' => new byte[] { 0, 0, 0, 0, 0x7E, 0, 0x7E, 0, 0, 0, 0, 0, 0, 0 },
            '>' => new byte[] { 0, 0, 0x60, 0x30, 0x18, 0x0C, 0x18, 0x30, 0x60, 0, 0, 0, 0, 0 },
            '<' => new byte[] { 0, 0, 0x06, 0x0C, 0x18, 0x30, 0x18, 0x0C, 0x06, 0, 0, 0, 0, 0 },
            '?' => new byte[] { 0, 0, 0x3C, 0x66, 0x06, 0x0C, 0x18, 0x18, 0x00, 0x18, 0, 0, 0, 0 },
            'A' => new byte[] { 0, 0, 0x18, 0x3C, 0x66, 0x66, 0x7E, 0x66, 0x66, 0x66, 0, 0, 0, 0 },
            'B' => new byte[] { 0, 0, 0x7C, 0x66, 0x66, 0x7C, 0x66, 0x66, 0x66, 0x7C, 0, 0, 0, 0 },
            'C' => new byte[] { 0, 0, 0x3C, 0x66, 0x60, 0x60, 0x60, 0x60, 0x66, 0x3C, 0, 0, 0, 0 },
            'D' => new byte[] { 0, 0, 0x78, 0x6C, 0x66, 0x66, 0x66, 0x66, 0x6C, 0x78, 0, 0, 0, 0 },
            'E' => new byte[] { 0, 0, 0x7E, 0x60, 0x60, 0x7C, 0x60, 0x60, 0x60, 0x7E, 0, 0, 0, 0 },
            'F' => new byte[] { 0, 0, 0x7E, 0x60, 0x60, 0x7C, 0x60, 0x60, 0x60, 0x60, 0, 0, 0, 0 },
            'G' => new byte[] { 0, 0, 0x3C, 0x66, 0x60, 0x60, 0x6E, 0x66, 0x66, 0x3E, 0, 0, 0, 0 },
            'H' => new byte[] { 0, 0, 0x66, 0x66, 0x66, 0x7E, 0x66, 0x66, 0x66, 0x66, 0, 0, 0, 0 },
            'I' => new byte[] { 0, 0, 0x3C, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x3C, 0, 0, 0, 0 },
            'J' => new byte[] { 0, 0, 0x1E, 0x06, 0x06, 0x06, 0x06, 0x66, 0x66, 0x3C, 0, 0, 0, 0 },
            'K' => new byte[] { 0, 0, 0x66, 0x6C, 0x78, 0x70, 0x78, 0x6C, 0x66, 0x66, 0, 0, 0, 0 },
            'L' => new byte[] { 0, 0, 0x60, 0x60, 0x60, 0x60, 0x60, 0x60, 0x60, 0x7E, 0, 0, 0, 0 },
            'M' => new byte[] { 0, 0, 0xC6, 0xEE, 0xFE, 0xD6, 0xC6, 0xC6, 0xC6, 0xC6, 0, 0, 0, 0 },
            'N' => new byte[] { 0, 0, 0x66, 0x76, 0x7E, 0x6E, 0x66, 0x66, 0x66, 0x66, 0, 0, 0, 0 },
            'O' => new byte[] { 0, 0, 0x3C, 0x66, 0x66, 0x66, 0x66, 0x66, 0x66, 0x3C, 0, 0, 0, 0 },
            'P' => new byte[] { 0, 0, 0x7C, 0x66, 0x66, 0x7C, 0x60, 0x60, 0x60, 0x60, 0, 0, 0, 0 },
            'Q' => new byte[] { 0, 0, 0x3C, 0x66, 0x66, 0x66, 0x66, 0x66, 0x6E, 0x3C, 0x0E, 0, 0, 0 },
            'R' => new byte[] { 0, 0, 0x7C, 0x66, 0x66, 0x7C, 0x78, 0x6C, 0x66, 0x66, 0, 0, 0, 0 },
            'S' => new byte[] { 0, 0, 0x3C, 0x66, 0x60, 0x3C, 0x06, 0x06, 0x66, 0x3C, 0, 0, 0, 0 },
            'T' => new byte[] { 0, 0, 0x7E, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0, 0, 0, 0 },
            'U' => new byte[] { 0, 0, 0x66, 0x66, 0x66, 0x66, 0x66, 0x66, 0x66, 0x3C, 0, 0, 0, 0 },
            'V' => new byte[] { 0, 0, 0x66, 0x66, 0x66, 0x66, 0x66, 0x3C, 0x3C, 0x18, 0, 0, 0, 0 },
            'W' => new byte[] { 0, 0, 0xC6, 0xC6, 0xC6, 0xD6, 0xFE, 0xEE, 0xC6, 0xC6, 0, 0, 0, 0 },
            'X' => new byte[] { 0, 0, 0x66, 0x66, 0x3C, 0x18, 0x3C, 0x66, 0x66, 0x66, 0, 0, 0, 0 },
            'Y' => new byte[] { 0, 0, 0x66, 0x66, 0x66, 0x3C, 0x18, 0x18, 0x18, 0x18, 0, 0, 0, 0 },
            'Z' => new byte[] { 0, 0, 0x7E, 0x06, 0x0C, 0x18, 0x30, 0x60, 0x60, 0x7E, 0, 0, 0, 0 },
            'a' => new byte[] { 0, 0, 0, 0, 0x3C, 0x06, 0x3E, 0x66, 0x66, 0x3E, 0, 0, 0, 0 },
            'b' => new byte[] { 0, 0, 0x60, 0x60, 0x7C, 0x66, 0x66, 0x66, 0x66, 0x7C, 0, 0, 0, 0 },
            'c' => new byte[] { 0, 0, 0, 0, 0x3C, 0x66, 0x60, 0x60, 0x66, 0x3C, 0, 0, 0, 0 },
            'd' => new byte[] { 0, 0, 0x06, 0x06, 0x3E, 0x66, 0x66, 0x66, 0x66, 0x3E, 0, 0, 0, 0 },
            'e' => new byte[] { 0, 0, 0, 0, 0x3C, 0x66, 0x7E, 0x60, 0x60, 0x3C, 0, 0, 0, 0 },
            'f' => new byte[] { 0, 0, 0x1C, 0x30, 0x7C, 0x30, 0x30, 0x30, 0x30, 0x30, 0, 0, 0, 0 },
            'g' => new byte[] { 0, 0, 0, 0, 0x3E, 0x66, 0x66, 0x66, 0x3E, 0x06, 0x3C, 0, 0, 0 },
            'h' => new byte[] { 0, 0, 0x60, 0x60, 0x7C, 0x66, 0x66, 0x66, 0x66, 0x66, 0, 0, 0, 0 },
            'i' => new byte[] { 0, 0, 0x18, 0x00, 0x38, 0x18, 0x18, 0x18, 0x18, 0x3C, 0, 0, 0, 0 },
            'j' => new byte[] { 0, 0, 0x06, 0x00, 0x06, 0x06, 0x06, 0x06, 0x06, 0x66, 0x3C, 0, 0, 0 },
            'k' => new byte[] { 0, 0, 0x60, 0x60, 0x66, 0x6C, 0x78, 0x6C, 0x66, 0x66, 0, 0, 0, 0 },
            'l' => new byte[] { 0, 0, 0x38, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x3C, 0, 0, 0, 0 },
            'm' => new byte[] { 0, 0, 0, 0, 0x6C, 0xFE, 0xD6, 0xC6, 0xC6, 0xC6, 0, 0, 0, 0 },
            'n' => new byte[] { 0, 0, 0, 0, 0x7C, 0x66, 0x66, 0x66, 0x66, 0x66, 0, 0, 0, 0 },
            'o' => new byte[] { 0, 0, 0, 0, 0x3C, 0x66, 0x66, 0x66, 0x66, 0x3C, 0, 0, 0, 0 },
            'p' => new byte[] { 0, 0, 0, 0, 0x7C, 0x66, 0x66, 0x66, 0x7C, 0x60, 0x60, 0, 0, 0 },
            'q' => new byte[] { 0, 0, 0, 0, 0x3E, 0x66, 0x66, 0x66, 0x3E, 0x06, 0x06, 0, 0, 0 },
            'r' => new byte[] { 0, 0, 0, 0, 0x7C, 0x66, 0x60, 0x60, 0x60, 0x60, 0, 0, 0, 0 },
            's' => new byte[] { 0, 0, 0, 0, 0x3E, 0x60, 0x3C, 0x06, 0x06, 0x7C, 0, 0, 0, 0 },
            't' => new byte[] { 0, 0, 0x30, 0x30, 0x7C, 0x30, 0x30, 0x30, 0x30, 0x1C, 0, 0, 0, 0 },
            'u' => new byte[] { 0, 0, 0, 0, 0x66, 0x66, 0x66, 0x66, 0x66, 0x3E, 0, 0, 0, 0 },
            'v' => new byte[] { 0, 0, 0, 0, 0x66, 0x66, 0x66, 0x3C, 0x3C, 0x18, 0, 0, 0, 0 },
            'w' => new byte[] { 0, 0, 0, 0, 0xC6, 0xC6, 0xD6, 0xFE, 0x6C, 0x6C, 0, 0, 0, 0 },
            'x' => new byte[] { 0, 0, 0, 0, 0x66, 0x3C, 0x18, 0x3C, 0x66, 0x66, 0, 0, 0, 0 },
            'y' => new byte[] { 0, 0, 0, 0, 0x66, 0x66, 0x66, 0x3E, 0x06, 0x06, 0x3C, 0, 0, 0 },
            'z' => new byte[] { 0, 0, 0, 0, 0x7E, 0x0C, 0x18, 0x30, 0x60, 0x7E, 0, 0, 0, 0 },
            '|' => new byte[] { 0, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0, 0, 0 },
            '_' => new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x7E, 0, 0, 0 },
            '[' => new byte[] { 0, 0x3C, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x3C, 0, 0, 0, 0 },
            ']' => new byte[] { 0, 0x3C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x3C, 0, 0, 0, 0 },
            _ => null
        };
    }
}
