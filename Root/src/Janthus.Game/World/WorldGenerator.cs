using Janthus.Model.Entities;
using Janthus.Model.Services;

namespace Janthus.Game.World;

public static class WorldGenerator
{
    // Tile definition IDs matching seed data
    private const byte Grass = 1;
    private const byte Water = 2;
    private const byte Stone = 3;
    private const byte Sand = 4;
    private const byte DarkGrass = 5;

    public static void Generate(WorldMap worldMap, IGameDataProvider provider)
    {
        var random = new Random(worldMap.Seed);
        var chunkSize = worldMap.ChunkSize;
        var totalWidth = worldMap.ChunkCountX * chunkSize;
        var totalHeight = worldMap.ChunkCountY * chunkSize;

        // Generate full world terrain grid
        var terrain = new byte[totalWidth, totalHeight];

        for (int wx = 0; wx < totalWidth; wx++)
        {
            for (int wy = 0; wy < totalHeight; wy++)
            {
                // Water border at world edges
                if (wx == 0 || wy == 0 || wx == totalWidth - 1 || wy == totalHeight - 1)
                {
                    terrain[wx, wy] = Water;
                    continue;
                }

                // Pond at world center (radius ~5)
                var cx = totalWidth / 2;
                var cy = totalHeight / 2;
                var dx = wx - cx;
                var dy = wy - cy;
                if (dx * dx + dy * dy < 25)
                {
                    terrain[wx, wy] = Water;
                    continue;
                }

                // Stone path across middle row
                if (wy == totalHeight / 2 && wx > 3 && wx < totalWidth - 4)
                {
                    terrain[wx, wy] = Stone;
                    continue;
                }

                // Random terrain distribution: 60% Grass, 20% DarkGrass, 15% Sand, 5% Stone
                var roll = random.Next(100);
                if (roll < 60)
                    terrain[wx, wy] = Grass;
                else if (roll < 80)
                    terrain[wx, wy] = DarkGrass;
                else if (roll < 95)
                    terrain[wx, wy] = Sand;
                else
                    terrain[wx, wy] = Stone;
            }
        }

        // Generate height map using multi-octave sine waves, values 0–8
        var heightMap = new byte[totalWidth, totalHeight];
        GenerateHeightMap(heightMap, totalWidth, totalHeight, terrain, worldMap.Seed);

        // First pass: save chunks (get DB IDs)
        var savedChunks = new MapChunk[worldMap.ChunkCountX, worldMap.ChunkCountY];
        for (int cy = 0; cy < worldMap.ChunkCountY; cy++)
        {
            for (int cx = 0; cx < worldMap.ChunkCountX; cx++)
            {
                var groundData = new byte[chunkSize * chunkSize];
                var heightData = new byte[chunkSize * chunkSize];

                for (int ly = 0; ly < chunkSize; ly++)
                {
                    for (int lx = 0; lx < chunkSize; lx++)
                    {
                        var wx = cx * chunkSize + lx;
                        var wy = cy * chunkSize + ly;
                        groundData[ly * chunkSize + lx] = terrain[wx, wy];
                        heightData[ly * chunkSize + lx] = heightMap[wx, wy];
                    }
                }

                var chunk = new MapChunk
                {
                    WorldMapId = worldMap.Id,
                    ChunkX = cx,
                    ChunkY = cy,
                    GroundData = groundData,
                    HeightData = heightData
                };

                provider.SaveChunk(chunk);
                savedChunks[cx, cy] = chunk;
            }
        }

        // Second pass: place objects
        // Use a separate Random seeded deterministically for object placement
        var objRng = new Random(worldMap.Seed + 1000);

        // ObjectDefinition IDs: Tree=1, Boulder=2
        const int TreeId = 1;
        const int BoulderId = 2;

        for (int cy = 0; cy < worldMap.ChunkCountY; cy++)
        {
            for (int cx = 0; cx < worldMap.ChunkCountX; cx++)
            {
                var chunk = savedChunks[cx, cy];

                for (int ly = 0; ly < chunkSize; ly++)
                {
                    for (int lx = 0; lx < chunkSize; lx++)
                    {
                        var wx = cx * chunkSize + lx;
                        var wy = cy * chunkSize + ly;
                        var tileId = terrain[wx, wy];

                        // Trees on grass/dark grass (10% chance)
                        if ((tileId == Grass || tileId == DarkGrass) && objRng.Next(100) < 10)
                        {
                            provider.SaveMapObject(new MapObject
                            {
                                MapChunkId = chunk.Id,
                                LocalX = lx,
                                LocalY = ly,
                                ObjectDefinitionId = TreeId
                            });
                        }
                        // Boulders on stone (8% chance)
                        else if (tileId == Stone && objRng.Next(100) < 8)
                        {
                            provider.SaveMapObject(new MapObject
                            {
                                MapChunkId = chunk.Id,
                                LocalX = lx,
                                LocalY = ly,
                                ObjectDefinitionId = BoulderId
                            });
                        }
                    }
                }
            }
        }
    }

    private static void GenerateHeightMap(byte[,] heightMap, int width, int height, byte[,] terrain, int seed)
    {
        // Multi-octave sine waves for gentle hills/valleys
        var freq1 = 0.05 + (seed % 10) * 0.002;
        var freq2 = 0.12;
        var freq3 = 0.03;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Water tiles forced to elevation 0
                if (terrain[x, y] == Water)
                {
                    heightMap[x, y] = 0;
                    continue;
                }

                var val = Math.Sin(x * freq1 + y * freq3) * 3.0
                        + Math.Sin(y * freq2 + x * 0.04) * 2.0
                        + Math.Sin((x + y) * freq3) * 1.5;

                // Normalize to 0–8 range
                var normalized = (val + 6.5) / 13.0; // roughly maps [-6.5, 6.5] to [0, 1]
                var elevation = (byte)Math.Clamp((int)(normalized * 9), 0, 8);

                heightMap[x, y] = elevation;
            }
        }
    }
}
