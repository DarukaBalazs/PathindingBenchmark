using src.Core.Heuristics;
using src.Core.Interfaces;
using src.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.Algorithms
{
    public sealed class DijkstraPathfinder<TNode> : IPathfinder<TNode> where TNode : notnull
    {
        private readonly WeightedAStar.WeightedAStar<TNode> _inner;

        public DijkstraPathfinder()
        {
            _inner = new WeightedAStar.WeightedAStar<TNode>(new ZeroHeuristic<TNode>());
        }

        public Result<TNode> Solve(
            TNode start,
            TNode goal,
            IGraph<TNode> graph,
            PathfinderConfig config)
        {
            config ??= new PathfinderConfig();

            var cfg = new PathfinderConfig
            {
                WeightW = 1.0,
                TimeLineitMs = config.TimeLineitMs,
                MaxExpansions = config.MaxExpansions,
                TieBreakLowG = config.TieBreakLowG
            };

            var result = _inner.Solve(start, goal, graph, cfg);

            return new Result<TNode>
            {
                Found = result.Found,
                Path = result.Path,
                PathCost = result.PathCost,
                Expansions = result.Expansions,
                Relaxations = result.Relaxations,
                ElapsedMs = result.ElapsedMs,
                AllocBytes = result.AllocBytes
            };
        }
    }
}
