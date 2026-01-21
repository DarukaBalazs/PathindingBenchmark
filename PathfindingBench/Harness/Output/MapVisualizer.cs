using src.Core.Grids;
using System;
using System.Collections.Generic;
using System.IO;

namespace Harness.Output
{
    public static class MapVisualizer
    {
        public static void SaveToBmp(string filePath, GridMap map, IReadOnlyList<GridNode> path, GridNode start, GridNode goal)
        {
            int width = map.Width;
            int height = map.Height;

            var colorWall = new byte[] { 0, 0, 0 };
            var colorEmpty = new byte[] { 255, 255, 255 };
            var colorPath = new byte[] { 255, 0, 0 };
            var colorStart = new byte[] { 0, 255, 0 };
            var colorGoal = new byte[] { 0, 0, 255 };

            var pathSet = new HashSet<GridNode>();
            if (path != null)
            {
                foreach (var node in path) pathSet.Add(node);
            }

            int rowSize = width * 3;
            int padding = (4 - (rowSize % 4)) % 4;
            int totalSize = 54 + (rowSize + padding) * height;

            using var stream = new FileStream(filePath, FileMode.Create);
            using var writer = new BinaryWriter(stream);

            writer.Write(new char[] { 'B', 'M' });
            writer.Write(totalSize);
            writer.Write((short)0);
            writer.Write((short)0);
            writer.Write(54);

            writer.Write(40);
            writer.Write(width);
            writer.Write(height);
            writer.Write((short)1);
            writer.Write((short)24);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);

            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    var node = new GridNode(x, y);
                    byte[] color;

                    if (x == start.X && y == start.Y) color = colorStart;
                    else if (x == goal.X && y == goal.Y) color = colorGoal;
                    else if (map.IsBlocked(node)) color = colorWall;
                    else if (pathSet.Contains(node)) color = colorPath;
                    else color = colorEmpty;

                    writer.Write(color);
                }
                for (int p = 0; p < padding; p++) writer.Write((byte)0);
            }
        }
    }
}