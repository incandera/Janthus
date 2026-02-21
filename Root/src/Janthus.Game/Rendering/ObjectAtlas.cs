using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Janthus.Game.Rendering;

public class ObjectAtlas
{
    public Texture2D Texture { get; }
    private readonly Dictionary<int, Rectangle> _sourceRects = new();

    public ObjectAtlas(Texture2D texture, Dictionary<int, Rectangle> sourceRects)
    {
        Texture = texture;
        _sourceRects = sourceRects;
    }

    public bool TryGetSourceRect(int objectDefId, out Rectangle rect)
    {
        return _sourceRects.TryGetValue(objectDefId, out rect);
    }

    public static ObjectAtlas Load(GraphicsDevice device, string texturePath, string mappingPath)
    {
        if (!File.Exists(texturePath) || !File.Exists(mappingPath))
            return null;

        Texture2D texture;
        using (var stream = File.OpenRead(texturePath))
        {
            texture = Texture2D.FromStream(device, stream);
        }

        var json = File.ReadAllText(mappingPath);
        var entries = JsonSerializer.Deserialize<Dictionary<string, TileAtlasEntry>>(json);

        var sourceRects = new Dictionary<int, Rectangle>();
        if (entries != null)
        {
            foreach (var kvp in entries)
            {
                if (int.TryParse(kvp.Key, out var id))
                {
                    sourceRects[id] = new Rectangle(kvp.Value.X, kvp.Value.Y, kvp.Value.W, kvp.Value.H);
                }
            }
        }

        return new ObjectAtlas(texture, sourceRects);
    }
}
