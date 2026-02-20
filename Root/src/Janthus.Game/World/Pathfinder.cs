using Microsoft.Xna.Framework;
using Janthus.Game.Actors;

namespace Janthus.Game.World;

public static class Pathfinder
{
    private static readonly (int dx, int dy)[] Neighbors =
    {
        (-1, -1), (-1, 0), (-1, 1),
        (0, -1),           (0, 1),
        (1, -1),  (1, 0),  (1, 1)
    };

    public static List<Point> FindPath(ChunkManager map, Point start, Point goal, List<ActorSprite> actors)
    {
        // If the goal is walkable and not actor-blocked (or is start), use it directly.
        // If the goal is not walkable, find nearest walkable tile via BFS from goal.
        var effectiveGoal = goal;
        var goalIsActorTile = false;

        if (!IsPassable(map, goal, actors, goal))
        {
            effectiveGoal = FindNearestWalkable(map, goal, actors);
            if (effectiveGoal == Point.Zero && !map.IsWalkable(0, 0))
                return null;
            if (effectiveGoal == Point.Zero)
                effectiveGoal = Point.Zero; // fallback
        }
        else
        {
            // Check if goal tile has an actor on it (we allow pathing TO actor tiles)
            goalIsActorTile = IsActorAt(actors, goal, start);
        }

        if (effectiveGoal == start)
            return new List<Point>();

        return AStar(map, start, effectiveGoal, actors, goalIsActorTile ? goal : effectiveGoal);
    }

    public static List<Point> FindPathAdjacentTo(ChunkManager map, Point start, Point target, List<ActorSprite> actors)
    {
        // Find the best adjacent walkable tile to the target
        Point? bestAdj = null;
        var bestDist = int.MaxValue;

        foreach (var (dx, dy) in Neighbors)
        {
            var adj = new Point(target.X + dx, target.Y + dy);
            if (adj == start)
                return new List<Point>();

            if (IsPassable(map, adj, actors, Point.Zero))
            {
                var dist = ChebyshevDistance(start, adj);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestAdj = adj;
                }
            }
        }

        if (bestAdj == null)
            return null;

        return AStar(map, start, bestAdj.Value, actors, bestAdj.Value);
    }

    private static List<Point> AStar(ChunkManager map, Point start, Point goal, List<ActorSprite> actors, Point allowedActorTile)
    {
        // Costs scaled by 10 for integer precision
        var openSet = new PriorityQueue<Point, int>();
        var cameFrom = new Dictionary<Point, Point>();
        var gScore = new Dictionary<Point, int> { [start] = 0 };

        openSet.Enqueue(start, ChebyshevDistance(start, goal) * 10);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            var currentElevation = map.GetElevation(current.X, current.Y);

            foreach (var (dx, dy) in Neighbors)
            {
                var neighbor = new Point(current.X + dx, current.Y + dy);

                if (!map.IsWalkable(neighbor.X, neighbor.Y))
                    continue;

                // Block actor-occupied tiles, except the allowed target tile
                if (neighbor != allowedActorTile && IsActorAt(actors, neighbor, start))
                    continue;

                // Cliff check: elevation difference > 3 is impassable
                var neighborElevation = map.GetElevation(neighbor.X, neighbor.Y);
                var elevDiff = Math.Abs(neighborElevation - currentElevation);
                if (elevDiff > 3)
                    continue;

                // Cost = base tile movement cost + climb cost (elevation diff * 0.5), scaled by 10
                var baseCost = (int)(map.GetMovementCost(neighbor.X, neighbor.Y) * 10);
                var climbCost = (int)(elevDiff * 5); // 0.5 * 10
                var tentativeG = gScore[current] + baseCost + climbCost;

                if (!gScore.TryGetValue(neighbor, out var existingG) || tentativeG < existingG)
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    var fScore = tentativeG + ChebyshevDistance(neighbor, goal) * 10;
                    openSet.Enqueue(neighbor, fScore);
                }
            }
        }

        return null;
    }

    private static Point FindNearestWalkable(ChunkManager map, Point origin, List<ActorSprite> actors)
    {
        var visited = new HashSet<Point> { origin };
        var queue = new Queue<Point>();
        queue.Enqueue(origin);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var (dx, dy) in Neighbors)
            {
                var neighbor = new Point(current.X + dx, current.Y + dy);
                if (!visited.Add(neighbor))
                    continue;

                if (!map.IsInBounds(neighbor.X, neighbor.Y))
                    continue;

                if (map.IsWalkable(neighbor.X, neighbor.Y) && !IsActorAt(actors, neighbor, Point.Zero))
                    return neighbor;

                queue.Enqueue(neighbor);
            }
        }

        return origin;
    }

    private static List<Point> ReconstructPath(Dictionary<Point, Point> cameFrom, Point current)
    {
        var path = new List<Point> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }

        path.Reverse();
        // Remove the start tile (path is exclusive of start)
        if (path.Count > 0)
            path.RemoveAt(0);

        return path;
    }

    private static bool IsPassable(ChunkManager map, Point tile, List<ActorSprite> actors, Point allowedActorTile)
    {
        if (!map.IsWalkable(tile.X, tile.Y))
            return false;

        if (tile != allowedActorTile && IsActorAt(actors, tile, Point.Zero))
            return false;

        return true;
    }

    private static bool IsActorAt(List<ActorSprite> actors, Point tile, Point excludeTile)
    {
        foreach (var actor in actors)
        {
            if (actor.TileX == tile.X && actor.TileY == tile.Y &&
                !(actor.TileX == excludeTile.X && actor.TileY == excludeTile.Y))
                return true;
        }
        return false;
    }

    private static int ChebyshevDistance(Point a, Point b)
    {
        return Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
    }
}
