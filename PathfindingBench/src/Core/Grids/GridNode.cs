using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.Core.Grids
{
    public readonly record struct GridNode(int X, int Y)
    {
        public override string ToString() => $"({X},{Y})";
    }
}
