using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.Core.Models
{
    /// <summary>
    /// Egy pathfinding futás eredménye (kimenet + metrikák).
    /// </summary>
    public sealed class Result<TNode>
    {
        public bool Found { get; set; }
        public double PathCost { get; set; }
        public int PathLength => Path?.Count ?? 0;
        public IReadOnlyList<TNode>? Path { get; init; }
        public long Expansions { get; init; }
        public long Relaxations { get; init; }
        public double ElapsedMs { get; set; }
        public long AllocBytes { get; init; }
    }
}
