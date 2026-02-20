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

    public void StartPlaying()
    {
        var rng = new Random();

        // Create player character with random alignment
        var playerAlignment = RandomAlignment(rng);
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

        // Create map
        var tileMap = TileMap.GenerateDefault(30, 30);

        // Create renderer and camera
        var renderer = new IsometricRenderer(_pixelTexture);
        renderer.SetFont(_font);
        var camera = new Camera(GraphicsDevice.Viewport);

        // Place player in center
        var playerSprite = new ActorSprite(player, 10, 10, Color.Cyan, "Hero");
        var playerController = new PlayerController(playerSprite, tileMap);

        // Create NPCs with random alignments
        var npcControllers = new List<NpcController>();
        var npcPositions = new[] { (5, 5), (10, 8), (20, 12), (7, 20), (22, 22) };
        var npcNames = new[] { "Guard", "Merchant", "Mage", "Rogue", "Ranger" };
        var npcColors = new[] { Color.Orange, Color.Yellow, Color.Purple, Color.LimeGreen, Color.SaddleBrown };

        for (int i = 0; i < npcPositions.Length; i++)
        {
            var npcAlignment = RandomAlignment(rng);
            var npc = new NonPlayerCharacter(
                3 + i, 3, 3, 3, 3, 3, 3,
                npcAlignment)
            {
                Name = npcNames[i]
            };
            npc.CurrentHitPoints = (decimal)npc.MaximumHitPoints;

            var npcSprite = new ActorSprite(npc, npcPositions[i].Item1, npcPositions[i].Item2,
                npcColors[i], npcNames[i]);

            // Calculate adversary relationship
            var seed = player.Name.GetHashCode() ^ npc.Name.GetHashCode()
                       ^ npcPositions[i].Item1 ^ (npcPositions[i].Item2 << 16);
            npcSprite.IsAdversary = AdversaryCalculator.IsAdversary(playerAlignment, npcAlignment, seed);

            npcControllers.Add(new NpcController(npcSprite, tileMap));
        }

        // Create UI
        var uiManager = new UIManager(_pixelTexture, _font, player, GraphicsDevice.Viewport);

        // Create playing state
        _playingState = new PlayingState(this, _input, _font, tileMap, renderer, camera,
            playerController, npcControllers, uiManager);

        // Center camera on player
        var playerScreen = renderer.TileToScreen(10, 10);
        camera.Position = playerScreen;

        _stateManager.ChangeState(_playingState);
    }

    private void ApplyResolution()
    {
        var (width, height) = Resolutions[_resolutionIndex];
        _graphics.PreferredBackBufferWidth = width;
        _graphics.PreferredBackBufferHeight = height;
        _graphics.ApplyChanges();

        Window.Title = $"Janthus - {CurrentResolutionLabel}";

        // Update UI layout and camera for new viewport
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
        GraphicsDevice.Clear(new Color(15, 15, 25));

        // World rendering with camera transform
        if (_stateManager.CurrentState is PlayingState playing)
        {
            _spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null, null, null,
                playing.Camera.GetTransformMatrix());

            var renderer = playing.Renderer;
            playing.Camera.Follow(playing.PlayerController.Sprite.ScreenPosition, renderer);

            // Draw map and actors
            renderer.DrawMap(_spriteBatch, playing.TileMap, playing.Camera);

            // Draw actors depth-sorted
            var allSprites = new List<ActorSprite> { playing.PlayerController.Sprite };
            foreach (var npc in playing.NpcControllers)
            {
                allSprites.Add(npc.Sprite);
            }
            allSprites.Sort((a, b) => (a.TileX + a.TileY).CompareTo(b.TileX + b.TileY));

            foreach (var sprite in allSprites)
            {
                var isPlayer = sprite == playing.PlayerController.Sprite;
                renderer.DrawActor(_spriteBatch, sprite, playing.Camera, isPlayer);
            }

            _spriteBatch.End();

            // UI without camera transform
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp);
            playing.UIManager.Draw(_spriteBatch);

            if (playing.IsPaused)
            {
                var viewport = GraphicsDevice.Viewport;
                _spriteBatch.Draw(_pixelTexture,
                    new Rectangle(0, 0, viewport.Width, viewport.Height),
                    Color.Black * 0.3f);
            }

            _spriteBatch.End();
        }
        else
        {
            // Menu/other states
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp);
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
