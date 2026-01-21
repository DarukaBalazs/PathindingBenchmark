using src.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.Core.Grids
{
    public sealed class GridMap : IGrid<GridNode>
    {
        private readonly bool[,] _blocked;
        private readonly int _width;
        private readonly int _height;
        private readonly bool _allowDiagonal;

        public GridMap(int width, int height, bool allowDiagonal = false)
        {
            if (width < 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 0)throw new ArgumentOutOfRangeException(nameof(height));
            _width = width;
            _height = height;
            _allowDiagonal = allowDiagonal;
            _blocked = new bool[width, height];
        }

        public int Width => _width;
        public int Height => _height;
        public bool AllowDiagonal => _allowDiagonal;

        public void SetBlocked(int x, int y, bool blocked = true)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height) return;

            _blocked[x, y] = blocked;
        }

        public bool IsBlocked(GridNode node)
        {
            var (x, y) = node;
            return x < 0 || x >= _width || y < 0 || y >= _height || _blocked[x, y];
        }

        public bool IsValid(GridNode node) => !IsBlocked(node);

        public IEnumerable<(GridNode neighbor, double cost)> GetNeighbors(GridNode node)
        {
            if (IsBlocked(node)) yield break;

            var dirs = new (int dx, int dy, double cost)[]
            {
                (1,0,1.0),
                (-1,0,1.0),
                (0,1,1.0),
                (0,-1,1.0)
            };

            foreach (var (dx,dy,cost) in dirs)
            {
                var nx = node.X + dx;
                var ny = node.Y + dy;

                if (nx >= 0 && nx < _width && ny >= 0 && ny < _height && !_blocked[nx,ny]) yield return (new GridNode(nx,ny), cost);
            }

            if (_allowDiagonal)
            {
                double diagCost = Math.Sqrt(2.0);
                var dirsD = new (int dx, int dy, double cost)[]
                {
                    ( 1, 1, diagCost),
                    ( 1,-1, diagCost),
                    (-1, 1, diagCost),
                    (-1,-1, diagCost)
                };

                foreach (var (dx, dy, cost) in dirsD)
                {
                    var nx = node.X + dx;
                    var ny = node.Y + dy;

                    if (nx >= 0 && nx < _width && ny >= 0 && ny < _height && !_blocked[nx, ny])
                        yield return (new GridNode(nx, ny), cost);
                }
            }
        }
    }
}
