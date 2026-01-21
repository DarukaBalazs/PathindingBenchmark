using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using src.Core.Models;

namespace src.Core.Interfaces
{
    /// <summary>
    /// Pathfinding algoritmus interfész. 
    /// </summary>
    public interface IPathfinder<TNode> where TNode : notnull
    {
        Result<TNode> Solve(TNode start, TNode goal, IGraph<TNode> graph, PathfinderConfig config);

    }
}
