using src.Core.Grids;
using src.Core.Interfaces;
using src.Core.Models;
using src.Algorithms.WeightedAStar;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace src.Algorithms
{
    public sealed class JpsPathfinder : IPathfinder<GridNode>
    {
        private readonly IHeuristic<GridNode> _heuristic;

        public JpsPathfinder(IHeuristic<GridNode> heuristic)
        {
            _heuristic = heuristic ?? throw new ArgumentNullException(nameof(heuristic));
        }

        public Result<GridNode> Solve(
            GridNode start,
            GridNode goal,
            IGraph<GridNode> graph,
            PathfinderConfig config)
        {
            if (!(graph is GridMap map))
                throw new ArgumentException("JPS only works with GridMap", nameof(graph));

            config ??= new PathfinderConfig();

            double w = Math.Max(1.0, config.WeightW);
            bool allowDiagonal = map.AllowDiagonal;

            var open = new MinPriorityQueue<WeightedAStar.WeightedAStar<GridNode>.QueueKey, GridNode>();
            var gScore = new Dictionary<GridNode, double>();
            var cameFrom = new Dictionary<GridNode, GridNode>();
            var closed = new HashSet<GridNode>();

            gScore[start] = 0;
            double hStart = _heuristic.Evaluate(start, goal);
            open.Enqueue(new WeightedAStar.WeightedAStar<GridNode>.QueueKey(w * hStart, 0), start);

            long expansions = 0;
            var stopwatch = Stopwatch.StartNew();
            bool found = false;

            while (open.Count > 0)
            {
                if (config.TimeLineitMs > 0 && stopwatch.ElapsedMilliseconds > config.TimeLineitMs) break;
                if (config.MaxExpansions > 0 && expansions > config.MaxExpansions) break;

                if (!open.TryDequeue(out var key, out var current)) break;

                if (current.Equals(goal))
                {
                    found = true;
                    break;
                }

                if (gScore.TryGetValue(current, out double gCurVal) && key.G > gCurVal + 1e-9)
                    continue;

                if (!closed.Add(current)) continue;

                expansions++;

                // Minden érvényes irányba próbálunk ugrani
                foreach (var neighbor in IdentifySuccessors(current, start, goal, map, closed, allowDiagonal))
                {
                    double dist = GetDistance(current, neighbor);
                    double newG = gScore[current] + dist;

                    if (gScore.TryGetValue(neighbor, out double oldG) && newG >= oldG)
                        continue;

                    gScore[neighbor] = newG;
                    cameFrom[neighbor] = current;

                    double h = _heuristic.Evaluate(neighbor, goal);
                    double f = newG + (w * h);

                    open.Enqueue(new WeightedAStar.WeightedAStar<GridNode>.QueueKey(f, newG), neighbor);
                }
            }

            stopwatch.Stop();

            IReadOnlyList<GridNode>? path = null;
            double pathCost = 0;

            if (found)
            {
                path = ReconstructPath(cameFrom, start, goal);
                if (gScore.TryGetValue(goal, out double gc)) pathCost = gc;
            }

            return new Result<GridNode>
            {
                Found = found,
                Path = path,
                PathCost = pathCost,
                Expansions = expansions,
                ElapsedMs = stopwatch.Elapsed.TotalMilliseconds,
                AllocBytes = 0
            };
        }

        private IEnumerable<GridNode> IdentifySuccessors(
            GridNode current,
            GridNode start,
            GridNode goal,
            GridMap map,
            HashSet<GridNode> closed,
            bool allowDiagonal)
        {
            var directions = new List<(int x, int y)>();

            foreach (var (neighbor, cost) in map.GetNeighbors(current))
            {
                directions.Add((neighbor.X - current.X, neighbor.Y - current.Y));
            }

            foreach (var (dx, dy) in directions)
            {
                var jumpPoint = Jump(current, dx, dy, start, goal, map, allowDiagonal);

                if (jumpPoint != null)
                {
                    var jp = jumpPoint.Value;
                    if (!closed.Contains(jp))
                    {
                        yield return jp;
                    }
                }
            }
        }

        private GridNode? Jump(GridNode startNode, int dx, int dy, GridNode start, GridNode goal, GridMap map, bool allowDiagonal)
        {
            int x = startNode.X;
            int y = startNode.Y;

            while (true)
            {
                x += dx;
                y += dy;

                // Fal vagy pálya vége
                if (map.IsBlocked(new GridNode(x, y)))
                    return null;

                var currentNode = new GridNode(x, y);

                // Cél megtalálva
                if (x == goal.X && y == goal.Y)
                    return currentNode;

                // Kanyar (Forced Neighbor) megtalálva
                if (HasForcedNeighbor(x, y, dx, dy, map))
                    return currentNode;

                // Átlós lépés esetén (ha engedélyezve van)
                if (allowDiagonal && dx != 0 && dy != 0)
                {
                    if (Jump(currentNode, dx, 0, start, goal, map, allowDiagonal) != null ||
                        Jump(currentNode, 0, dy, start, goal, map, allowDiagonal) != null)
                    {
                        return currentNode;
                    }
                }
            }
        }

        private bool HasForcedNeighbor(int x, int y, int dx, int dy, GridMap map)
        {
            if (dx != 0 && dy == 0) // Vízszintes
            {
                // Fal van fent, és a következő lépésben (x+dx) fent már nincs? -> Fordulópont
                if (map.IsBlocked(new GridNode(x, y - 1)) && !map.IsBlocked(new GridNode(x + dx, y - 1)))
                    return true;

                // Fal van lent, és a következő lépésben lent már nincs? -> Fordulópont
                if (map.IsBlocked(new GridNode(x, y + 1)) && !map.IsBlocked(new GridNode(x + dx, y + 1)))
                    return true;
            }
            else if (dx == 0 && dy != 0) // Függőleges
            {
                // Fal balra...
                if (map.IsBlocked(new GridNode(x - 1, y)) && !map.IsBlocked(new GridNode(x - 1, y + dy)))
                    return true;

                // Fal jobbra...
                if (map.IsBlocked(new GridNode(x + 1, y)) && !map.IsBlocked(new GridNode(x + 1, y + dy)))
                    return true;
            }
            return false;
        }

        private double GetDistance(GridNode a, GridNode b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private List<GridNode> ReconstructPath(Dictionary<GridNode, GridNode> cameFrom, GridNode start, GridNode goal)
        {
            var path = new List<GridNode>();
            var current = goal;
            path.Add(current);
            while (!current.Equals(start))
            {
                if (!cameFrom.TryGetValue(current, out var prev)) break;
                current = prev;
                path.Add(current);
            }
            path.Reverse();
            return path;
        }
    }
}