using src.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.Core.Heuristics
{
    /// <summary>
    /// Nulla heurisztika: h(n) = 0. A*-ból Dijkstra-t csinál.
    /// </summary>
    public sealed class ZeroHeuristic<TNode> : IHeuristic<TNode> where TNode : notnull
    {
        public double Evaluate(TNode current, TNode goal) => 0.0;
    }
}