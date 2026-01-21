using src.Core.Grids;
using src.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.Core.Heuristics
{
    /// <summary>
    /// Euklideszi távolság.
    /// h(n) = sqrt(dx^2 + dy^2)
    /// </summary>
    public sealed class Euclidean : IHeuristic<GridNode>
    {
        public double Evaluate(GridNode current, GridNode goal)
        {
            int dx = current.X - goal.X;
            int dy = current.Y - goal.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}