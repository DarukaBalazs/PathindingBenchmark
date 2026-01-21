using src.Algorithms;
using src.Algorithms.WeightedAStar;
using src.Core.Grids;
using src.Core.Heuristics;
using src.Core.Interfaces;
using System;

namespace Harness.Algorithms
{
    public static class AlgorithmFactory
    {
        public static IPathfinder<GridNode> Create(AlgorithmKind kind, double weightW)
        {
            return kind switch
            {
                AlgorithmKind.Dijkstra =>
                    new DijkstraPathfinder<GridNode>(),

                AlgorithmKind.WeightedAStar =>
                    new WeightedAStar<GridNode>(new Manhattan()),

                AlgorithmKind.JPS =>
                    new JpsPathfinder(new Euclidean()),

            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown algorithm kind.")
            };
        }

        public static string GetName(AlgorithmKind kind)
            => kind.ToString();
    }
}
