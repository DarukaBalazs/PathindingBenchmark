using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.Core.Interfaces
{
    public interface IGraph<TNode> where TNode : notnull
    {
        /// <summary>
        /// Visszaadja a csúcs szomszédait a költséggel együtt.
        /// </summary>
        IEnumerable<(TNode neighbor, double cost)> GetNeighbors(TNode node);
        /// <summary>
        /// Igaz, ha a csúcs a gráf érvényes része (nem "out of bounds", nem tiltott stb.).
        /// </summary>
        bool IsValid(TNode node);
    }
}
