using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.Core.Interfaces
{
    /// <summary>
    /// Rács-specifikus gráf interfész. Jelenleg marker interface,
    /// később bővíthető (pl. Reveal, CellType stb.).
    /// </summary>
    public interface IGrid<TNode> : IGraph<TNode> where TNode : notnull
    {

    }
}
