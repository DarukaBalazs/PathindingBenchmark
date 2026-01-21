using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.Core.Interfaces
{
    /// <summary>
    /// Heurisztika interfész kereső algoritmusokhoz (pl. A*, Weighted A*).
    /// </summary>
    public interface IHeuristic<TNode> where TNode : notnull
    {
        /// <summary>
        /// Becsült minimális hátralévő költség a current csúcsból a goal csúcsba.
        /// </summary>
        double Evaluate(TNode current, TNode goal);
    }
}
