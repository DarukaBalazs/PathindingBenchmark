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
    /// Manhattan-távolság (4-irányú gridhez).
    /// h(n) = |dx| + |dy|
    /// </summary>
    public sealed class Manhattan : IHeuristic<GridNode>
    {
        public double Evaluate(GridNode current, GridNode goal)
        {
            int dx = current.X - goal.X;
            int dy = current.Y - goal.Y;
            return Math.Abs(dx) + Math.Abs(dy);
        }
    }
}