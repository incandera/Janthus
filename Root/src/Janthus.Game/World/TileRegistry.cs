using Microsoft.Xna.Framework;
using Janthus.Model.Entities;
using Janthus.Model.Services;

namespace Janthus.Game.World;

public class TileRegistry
{
    private readonly Dictionary<int, GameTile> _tiles = new();

    public TileRegistry(IGameDataProvider provider)
    {
        foreach (var def in provider.GetTileDefinitions())
        {
            var color = ParseHexColor(def.ColorHex);
            _tiles[def.Id] = new GameTile(def.Id, def.Name, color, def.IsWalkable, def.BaseMovementCost);
        }
    }

    public GameTile GetTile(int definitionId)
    {
        return _tiles.TryGetValue(definitionId, out var tile) ? tile : null;
    }

    private static Color ParseHexColor(string hex)
    {
        if (hex.StartsWith('#'))
            hex = hex[1..];

        var r = Convert.ToByte(hex[..2], 16);
        var g = Convert.ToByte(hex[2..4], 16);
        var b = Convert.ToByte(hex[4..6], 16);
        return new Color(r, g, b);
    }
}
