using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.Core.Models
{
    public sealed class PathfinderConfig
    {
        /// <summary>
        /// Weighted A* súly (w >= 1.0). w=1.0 → klasszikus A*.
        /// Dijkstra esetén fixen 1.0 + ZeroHeuristic.
        /// </summary>
        public double WeightW { get; init; } = 1.0;

        public int TimeLineitMs { get; init; } = 0;
        public int MaxExpansions { get; init; } = 0;
        /// <summary>
        /// Ha true, akkor azonos f érték esetén az alacsonyabb g értékű csomópontot preferálja
        /// </summary>
        public bool TieBreakLowG {  get; init; } = false;
    }
}
