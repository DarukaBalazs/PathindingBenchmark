using src.Core.Interfaces;
using src.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.Algorithms.WeightedAStar
{
    public sealed class WeightedAStar<TNode> : IPathfinder<TNode> where TNode : notnull
    {
        private readonly IHeuristic<TNode> _heuristic;

        public WeightedAStar(IHeuristic<TNode> heuristic)
        {
            _heuristic = heuristic ?? throw new ArgumentNullException(nameof(heuristic));
        }

        internal readonly struct QueueKey : IComparable<QueueKey>
        {
            public QueueKey(double f, double g)
            {
                F = f;
                G = g;
            }

            public double F { get; }
            public double G { get; }
            public int CompareTo(QueueKey other)
            {
                int c = F.CompareTo(other.F);
                if (c != 0) return c;
                return G.CompareTo(other.G);
            }
        }

        public Result<TNode> Solve(
            TNode start,
            TNode goal,
            IGraph<TNode> graph,
            PathfinderConfig config)
        {
            if (graph is null) throw new ArgumentNullException(nameof(graph));
            config ??= new PathfinderConfig();

            double w = Math.Max(1.0,config.WeightW);
            int timeLimitMs = config.TimeLineitMs;
            int maxExpansions = config.MaxExpansions;
            bool tieBreakLowG = config.TieBreakLowG;

            var stopwatch = Stopwatch.StartNew();

            long expansions = 0;
            long relaxations = 0;

            int gc0Before = GC.CollectionCount(0);
            int gc1Before = GC.CollectionCount(1);
            int gc2Before = GC.CollectionCount(2);

            var open = new MinPriorityQueue<QueueKey, TNode>();
            var gScore = new Dictionary<TNode, double>();
            var cameFrom = new Dictionary<TNode, TNode>();
            var closed = new HashSet<TNode>();

            gScore[start] = 0.0;

            double hStart = _heuristic.Evaluate(start,goal);
            double fStart = w * hStart;
            var keyStart = MakeKey(fStart, 0.0, tieBreakLowG);

            open.Enqueue(keyStart, start);

            bool found = false;
            TNode current = start;

            while (open.Count > 0)
            {
                if (!open.TryDequeue(out var key, out current)) break;

                if (!gScore.TryGetValue(current, out double gCurrent)) continue;

                if (key.G > gCurrent + 1e-9) continue;

                if (EqualityComparer<TNode>.Default.Equals(current, goal))
                {
                    found = true;
                    break;
                }

                if (!closed.Add(current))
                    continue;

                expansions++;
                if (maxExpansions > 0 && expansions > maxExpansions)
                    break;

                if (timeLimitMs > 0 && stopwatch.ElapsedMilliseconds > timeLimitMs)
                    break;

                foreach (var (neighbor, cost) in graph.GetNeighbors(current))
                {
                    double tentativeG = gCurrent + cost;

                    if (gScore.TryGetValue(neighbor, out double gOld) && tentativeG >= gOld)
                    {
                        continue;
                    }

                    relaxations++;
                    gScore[neighbor] = tentativeG;
                    cameFrom[neighbor] = current;

                    double h = _heuristic.Evaluate(neighbor, goal);
                    double f = tentativeG + w * h;

                    var keyNeighbor = MakeKey(f, tentativeG, tieBreakLowG);
                    open.Enqueue(keyNeighbor, neighbor);
                }
            }
            stopwatch.Stop();

            IReadOnlyList<TNode>? path = null;
            double pathCost = 0.0;

            if (found && gScore.TryGetValue(goal, out double gGoal))
            {
                path = ReconstructPath(cameFrom, start, goal);
                pathCost = gGoal;
            }

            int gc0After = GC.CollectionCount(0);
            int gc1After = GC.CollectionCount(1);
            int gc2After = GC.CollectionCount(2);

            return new Result<TNode>
            {
                Found = found,
                Path = path,
                PathCost = found ? pathCost : 0.0,
                Expansions = expansions,
                Relaxations = relaxations,
                ElapsedMs = stopwatch.Elapsed.TotalMilliseconds,
                AllocBytes = 0
            };


        }

        private static QueueKey MakeKey(double f, double g, bool tieBreakLowG)
        {
            double secondary = tieBreakLowG ? g : -g;
            return new QueueKey(f, secondary);  
        }

        private static List<TNode> ReconstructPath(
            Dictionary<TNode, TNode> cameFrom,
            TNode start,
            TNode goal)
        {
            var path = new List<TNode>();
            var current = goal;
            path.Add(current);

            var comparer = EqualityComparer<TNode>.Default;

            while (!comparer.Equals(current, start))
            {
                if (!cameFrom.TryGetValue(current, out var prev))
                {
                    // Belső hiba / hiányos cameFrom – ilyenkor visszaadjuk, ami van.
                    break;
                }
                current = prev;
                path.Add(current);
            }

            path.Reverse();
            return path;
        }
    }
}
