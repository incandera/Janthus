using Janthus.Model.Entities;
using Janthus.Model.Services;

namespace Janthus.Game.World;

public static class WorldGenerator
{
    // Tile definition IDs matching seed data
    private const byte Grass = 1;
    private const byte Water = 2;
    private const byte Sand = 3;
    private const byte Stone = 4;
    private const byte Dirt = 5;

    // Object definition IDs matching seed data
    private const int TreeId = 1;
    private const int BoulderId = 2;

    // Returns [0.0, 1.0] if inside (0=edge, 1=center), negative if outside
    private delegate float ShapeFunc(int x, int y);

    private struct Zone
    {
        public int Priority;
        public byte TerrainType;
        public byte ElevationMin;
        public byte ElevationMax;
        public bool ElevationGradient;
        public int TreeDensityPercent;
        public int BoulderDensityPercent;
        public ShapeFunc Contains;
    }

    public static void Generate(WorldMap worldMap, IGameDataProvider provider)
    {
        var random = new Random(worldMap.Seed);
        var chunkSize = worldMap.ChunkSize;
        var totalWidth = worldMap.ChunkCountX * chunkSize;
        var totalHeight = worldMap.ChunkCountY * chunkSize;
        var centerX = totalWidth / 2;
        var centerY = totalHeight / 2;

        // Allocate world arrays
        var terrain = new byte[totalWidth, totalHeight];
        var heightMap = new byte[totalWidth, totalHeight];
        var treeDensity = new float[totalWidth, totalHeight];
        var boulderDensity = new float[totalWidth, totalHeight];

        // Initialize defaults: Grass, elevation 1
        for (int x = 0; x < totalWidth; x++)
        {
            for (int y = 0; y < totalHeight; y++)
            {
                terrain[x, y] = Grass;
                heightMap[x, y] = 1;
            }
        }

        // Build and apply zones in priority order
        var zones = BuildZones(worldMap.Seed, centerX, centerY, totalWidth, totalHeight);
        zones.Sort((a, b) => a.Priority.CompareTo(b.Priority));

        foreach (var zone in zones)
        {
            for (int x = 0; x < totalWidth; x++)
            {
                for (int y = 0; y < totalHeight; y++)
                {
                    var influence = zone.Contains(x, y);
                    if (influence < 0f) continue;

                    terrain[x, y] = zone.TerrainType;

                    if (zone.ElevationGradient)
                    {
                        var lerped = zone.ElevationMin + (zone.ElevationMax - zone.ElevationMin) * (1f - influence);
                        heightMap[x, y] = (byte)Math.Clamp((int)Math.Round(lerped), 0, 8);
                    }
                    else
                    {
                        heightMap[x, y] = zone.ElevationMin;
                    }

                    treeDensity[x, y] = zone.TreeDensityPercent * influence;
                    boulderDensity[x, y] = zone.BoulderDensityPercent * influence;
                }
            }
        }

        // Subtle elevation noise on non-water tiles
        ApplyElevationNoise(heightMap, terrain, totalWidth, totalHeight, worldMap.Seed);

        // NPC exclusion zones (3-tile Chebyshev radius)
        var npcSpawns = new (int x, int y)[]
        {
            (centerX - 5, centerY - 5),   // Guard
            (centerX + 10, centerY - 3),   // Merchant
            (centerX + 20, centerY + 5),   // Mage
            (centerX + 28, centerY + 15),  // Mercenary
            (centerX + 25, centerY + 20),  // Bandit
            (centerX + 8, centerY + 8),    // Player
        };

        // First pass: save chunks
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

        // Second pass: place objects using density arrays
        var objRng = new Random(worldMap.Seed + 1000);
        var allObjects = new List<MapObject>();

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

                        // Skip water tiles
                        if (terrain[wx, wy] == Water) continue;

                        // Skip NPC exclusion zones
                        var nearNpc = false;
                        foreach (var spawn in npcSpawns)
                        {
                            if (Math.Abs(wx - spawn.x) <= 3 && Math.Abs(wy - spawn.y) <= 3)
                            {
                                nearNpc = true;
                                break;
                            }
                        }
                        if (nearNpc) continue;

                        var treeChance = treeDensity[wx, wy];
                        var boulderChance = boulderDensity[wx, wy];

                        if (treeChance > 0 && objRng.Next(100) < (int)treeChance)
                        {
                            allObjects.Add(new MapObject
                            {
                                MapChunkId = chunk.Id,
                                LocalX = lx,
                                LocalY = ly,
                                ObjectDefinitionId = TreeId
                            });
                        }
                        else if (boulderChance > 0 && objRng.Next(100) < (int)boulderChance)
                        {
                            allObjects.Add(new MapObject
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

        provider.SaveMapObjects(allObjects);
    }

    private static List<Zone> BuildZones(int seed, int centerX, int centerY, int width, int height)
    {
        var zones = new List<Zone>();

        // P0: Valley Floor — large noisy ellipse of flat grass
        zones.Add(new Zone
        {
            Priority = 0,
            TerrainType = Grass,
            ElevationMin = 1,
            ElevationMax = 2,
            ElevationGradient = false,
            TreeDensityPercent = 2,
            BoulderDensityPercent = 0,
            Contains = NoisyEllipse(centerX, centerY, 72, 72, 0.08f, seed + 100)
        });

        // P1: Valley Rim — everything outside the valley floor
        var valleyFloor = NoisyEllipse(centerX, centerY, 72, 72, 0.08f, seed + 100);
        zones.Add(new Zone
        {
            Priority = 1,
            TerrainType = Stone,
            ElevationMin = 5,
            ElevationMax = 8,
            ElevationGradient = true,
            TreeDensityPercent = 5,
            BoulderDensityPercent = 10,
            Contains = (int x, int y) =>
            {
                var inside = valleyFloor(x, y);
                if (inside >= 0f) return -1f; // inside the valley = outside the rim
                // Convert distance outside valley to 0..1 influence (closer to edge = 0, far away = 1)
                return Math.Clamp(-inside, 0f, 1f);
            }
        });

        // P2: SE Forest — dense trees southeast of lake
        zones.Add(new Zone
        {
            Priority = 2,
            TerrainType = Grass,
            ElevationMin = 1,
            ElevationMax = 3,
            ElevationGradient = true,
            TreeDensityPercent = 35,
            BoulderDensityPercent = 1,
            Contains = NoisyEllipse(centerX + 28, centerY + 24, 28, 26, 0.12f, seed + 200)
        });

        // P3: NW Boulder Field — landslide zone northwest
        zones.Add(new Zone
        {
            Priority = 3,
            TerrainType = Stone,
            ElevationMin = 2,
            ElevationMax = 5,
            ElevationGradient = true,
            TreeDensityPercent = 8,
            BoulderDensityPercent = 25,
            Contains = NoisyEllipse(centerX - 32, centerY - 30, 22, 20, 0.10f, seed + 300)
        });

        // P4: Shoreline Sand — ring around lake
        var lakeOuter = NoisyEllipse(centerX, centerY - 4, 16, 13, 0.06f, seed + 400);
        var lakeInner = NoisyEllipse(centerX, centerY - 4, 11, 8, 0.06f, seed + 401);
        zones.Add(new Zone
        {
            Priority = 4,
            TerrainType = Sand,
            ElevationMin = 0,
            ElevationMax = 1,
            ElevationGradient = false,
            TreeDensityPercent = 0,
            BoulderDensityPercent = 0,
            Contains = (int x, int y) =>
            {
                var outer = lakeOuter(x, y);
                var inner = lakeInner(x, y);
                if (outer < 0f) return -1f; // outside the larger ellipse
                if (inner >= 0f) return -1f; // inside the lake itself
                return outer; // in the ring between
            }
        });

        // River waypoints (lake south edge → SW world edge)
        var riverWaypoints = new float[][]
        {
            new[] { (float)centerX, centerY + 4f },
            new[] { centerX - 8f, centerY + 14f },
            new[] { centerX - 18f, centerY + 26f },
            new[] { centerX - 30f, centerY + 38f },
            new[] { centerX - 44f, centerY + 50f },
            new[] { centerX - 58f, centerY + 64f },
            new[] { centerX - 70f, centerY + 76f },
        };

        // P5: Riverbank Sand — wide sand strip along river
        zones.Add(new Zone
        {
            Priority = 5,
            TerrainType = Sand,
            ElevationMin = 0,
            ElevationMax = 1,
            ElevationGradient = false,
            TreeDensityPercent = 0,
            BoulderDensityPercent = 0,
            Contains = PathShape(riverWaypoints, 5f)
        });

        // Lake trail waypoints (loop around the lake)
        var lakeTrailWaypoints = new float[][]
        {
            new[] { centerX - 14f, centerY - 10f },
            new[] { centerX - 8f, centerY - 16f },
            new[] { centerX + 6f, centerY - 17f },
            new[] { centerX + 14f, centerY - 12f },
            new[] { centerX + 16f, centerY - 2f },
            new[] { centerX + 12f, centerY + 8f },
            new[] { centerX + 2f, centerY + 10f },
            new[] { centerX - 8f, centerY + 6f },
            new[] { centerX - 14f, centerY - 2f },
            new[] { centerX - 14f, centerY - 10f },
        };

        // P6: Dirt Trail — loop around lake
        zones.Add(new Zone
        {
            Priority = 6,
            TerrainType = Dirt,
            ElevationMin = 1,
            ElevationMax = 1,
            ElevationGradient = false,
            TreeDensityPercent = 0,
            BoulderDensityPercent = 0,
            Contains = PathShape(lakeTrailWaypoints, 1.8f)
        });

        // NE trail waypoints
        var neTrailWaypoints = new float[][]
        {
            new[] { centerX + 6f, centerY - 17f },
            new[] { centerX + 20f, centerY - 28f },
            new[] { centerX + 36f, centerY - 38f },
            new[] { centerX + 52f, centerY - 48f },
            new[] { centerX + 66f, centerY - 58f },
        };

        // P7: Stone Trail — NE branch
        zones.Add(new Zone
        {
            Priority = 7,
            TerrainType = Stone,
            ElevationMin = 1,
            ElevationMax = 2,
            ElevationGradient = false,
            TreeDensityPercent = 0,
            BoulderDensityPercent = 0,
            Contains = PathShape(neTrailWaypoints, 1.5f)
        });

        // P8: Central Lake — water
        zones.Add(new Zone
        {
            Priority = 8,
            TerrainType = Water,
            ElevationMin = 0,
            ElevationMax = 0,
            ElevationGradient = false,
            TreeDensityPercent = 0,
            BoulderDensityPercent = 0,
            Contains = NoisyEllipse(centerX, centerY - 4, 11, 8, 0.06f, seed + 401)
        });

        // P9: River — water path from lake south to SW edge
        zones.Add(new Zone
        {
            Priority = 9,
            TerrainType = Water,
            ElevationMin = 0,
            ElevationMax = 0,
            ElevationGradient = false,
            TreeDensityPercent = 0,
            BoulderDensityPercent = 0,
            Contains = PathShape(riverWaypoints, 2.5f)
        });

        // P10: World Edge — single-tile water border
        zones.Add(new Zone
        {
            Priority = 10,
            TerrainType = Water,
            ElevationMin = 0,
            ElevationMax = 0,
            ElevationGradient = false,
            TreeDensityPercent = 0,
            BoulderDensityPercent = 0,
            Contains = (int x, int y) =>
            {
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                    return 1f;
                return -1f;
            }
        });

        return zones;
    }

    private static ShapeFunc NoisyEllipse(float cxF, float cyF, float radiusX, float radiusY, float noiseAmp, int seed)
    {
        return (int x, int y) =>
        {
            var dx = x - cxF;
            var dy = y - cyF;
            var angle = (float)Math.Atan2(dy, dx);
            var noise = SeededNoise(angle, seed);
            var noisyRx = radiusX * (1f + noiseAmp * noise);
            var noisyRy = radiusY * (1f + noiseAmp * noise);

            // Ellipse distance: (dx/rx)^2 + (dy/ry)^2
            var dist = (dx * dx) / (noisyRx * noisyRx) + (dy * dy) / (noisyRy * noisyRy);

            if (dist > 1f)
                return -(dist - 1f); // negative = outside
            return 1f - dist; // 1.0 at center, 0.0 at boundary
        };
    }

    private static ShapeFunc PathShape(float[][] waypoints, float halfWidth)
    {
        return (int x, int y) =>
        {
            var minDist = float.MaxValue;
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                var d = DistanceToSegment(x, y, waypoints[i][0], waypoints[i][1], waypoints[i + 1][0], waypoints[i + 1][1]);
                if (d < minDist) minDist = d;
            }

            if (minDist > halfWidth)
                return -(minDist - halfWidth) / halfWidth; // negative = outside
            return 1f - (minDist / halfWidth); // 1.0 at center, 0.0 at edge
        };
    }

    private static float DistanceToSegment(float px, float py, float ax, float ay, float bx, float by)
    {
        var abx = bx - ax;
        var aby = by - ay;
        var apx = px - ax;
        var apy = py - ay;
        var ab2 = abx * abx + aby * aby;

        if (ab2 < 0.0001f)
            return (float)Math.Sqrt(apx * apx + apy * apy);

        var t = Math.Clamp((apx * abx + apy * aby) / ab2, 0f, 1f);
        var closestX = ax + t * abx;
        var closestY = ay + t * aby;
        var dx = px - closestX;
        var dy = py - closestY;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    private static float SeededNoise(float angle, int seed)
    {
        // Deterministic pseudo-noise from angle + seed using high-frequency sine composition
        var s = seed * 0.7131f;
        var v = (float)(
            Math.Sin(angle * 5.0 + s) * 0.3 +
            Math.Sin(angle * 11.0 + s * 1.3) * 0.25 +
            Math.Sin(angle * 23.0 + s * 0.7) * 0.2 +
            Math.Sin(angle * 37.0 + s * 2.1) * 0.15 +
            Math.Sin(angle * 53.0 + s * 0.3) * 0.1
        );
        return Math.Clamp(v * 2f, -0.5f, 0.5f);
    }

    private static void ApplyElevationNoise(byte[,] heightMap, byte[,] terrain, int width, int height, int seed)
    {
        var freq1 = 0.08 + (seed % 10) * 0.003;
        var freq2 = 0.13;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (terrain[x, y] == Water) continue;

                var noise = Math.Sin(x * freq1 + y * freq2) + Math.Sin((x + y) * 0.05 + seed * 0.1);
                var delta = (int)Math.Round(noise * 0.6);
                var newElev = heightMap[x, y] + delta;
                heightMap[x, y] = (byte)Math.Clamp(newElev, 0, 8);
            }
        }
    }
}
