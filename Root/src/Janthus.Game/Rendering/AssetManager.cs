using Microsoft.Xna.Framework.Graphics;

namespace Janthus.Game.Rendering;

public class AssetManager
{
    public TileAtlas TileAtlas { get; private set; }
    public ObjectAtlas ObjectAtlas { get; private set; }
    public CharacterSpriteSheet PlayerSheet { get; private set; }
    public CharacterSpriteSheet DefaultNpcSheet { get; private set; }

    public void LoadAll(GraphicsDevice device, string contentRoot)
    {
        try
        {
            var tilesDir = Path.Combine(contentRoot, "tiles");
            TileAtlas = TileAtlas.Load(device,
                Path.Combine(tilesDir, "terrain.png"),
                Path.Combine(tilesDir, "terrain.json"));
        }
        catch { /* Missing assets — use programmatic fallback */ }

        try
        {
            var objectsDir = Path.Combine(contentRoot, "objects");
            ObjectAtlas = ObjectAtlas.Load(device,
                Path.Combine(objectsDir, "objects.png"),
                Path.Combine(objectsDir, "objects.json"));
        }
        catch { /* Missing assets — use programmatic fallback */ }

        try
        {
            var spritesDir = Path.Combine(contentRoot, "sprites");
            PlayerSheet = LoadCharacterSheet(device, Path.Combine(spritesDir, "player.png"));
        }
        catch { /* Missing assets — use programmatic fallback */ }

        try
        {
            var spritesDir = Path.Combine(contentRoot, "sprites");
            DefaultNpcSheet = LoadCharacterSheet(device, Path.Combine(spritesDir, "npc.png"));
        }
        catch { /* Missing assets — use programmatic fallback */ }
    }

    private static CharacterSpriteSheet LoadCharacterSheet(GraphicsDevice device, string texturePath)
    {
        if (!File.Exists(texturePath))
            return null;

        Texture2D texture;
        using (var stream = File.OpenRead(texturePath))
        {
            texture = Texture2D.FromStream(device, stream);
        }

        var sheet = new CharacterSpriteSheet(texture);

        // Bellanger format: 128x128 frames, 8 directions per animation block
        const int frameSize = 128;

        // Idle: row 0-7 (8 directions), 1 frame
        sheet.AddAnimation(AnimationType.Idle, new SpriteAnimation(
            frameCount: 1, frameDuration: 1.0f, frameWidth: frameSize, frameHeight: frameSize,
            loops: true, startRow: 0));

        // Walk: row 8-15 (8 directions), 8 frames
        sheet.AddAnimation(AnimationType.Walk, new SpriteAnimation(
            frameCount: 8, frameDuration: 0.1f, frameWidth: frameSize, frameHeight: frameSize,
            loops: true, startRow: 8));

        // Attack: row 16-23 (8 directions), 4 frames
        sheet.AddAnimation(AnimationType.Attack, new SpriteAnimation(
            frameCount: 4, frameDuration: 0.15f, frameWidth: frameSize, frameHeight: frameSize,
            loops: false, startRow: 16));

        // Death: row 24-31 (8 directions), 6 frames
        sheet.AddAnimation(AnimationType.Death, new SpriteAnimation(
            frameCount: 6, frameDuration: 0.2f, frameWidth: frameSize, frameHeight: frameSize,
            loops: false, startRow: 24));

        return sheet;
    }
}
