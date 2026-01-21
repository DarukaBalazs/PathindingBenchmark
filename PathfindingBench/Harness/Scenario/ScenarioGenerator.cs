using Harness.Scenario;
using src.Core.Grids;
using System;
using System.Collections.Generic;

namespace Harness.Scenario
{
    public static class ScenarioGenerator
    {
        public static GridMap BuildGrid(ScenarioConfig config)
        {
            var map = new GridMap(config.Width, config.Height, config.AllowDiagonal);
            var rng = new Random(config.Seed);

            switch (config.MapKind)
            {
                case MapKind.Random:
                    FillRandom(map, config.ObstacleDensity, rng, config.Start, config.Goal);
                    break;

                case MapKind.MazeLike:
                    FillMazeLike(map, rng, config.Start, config.Goal);
                    break;

                default:
                    FillRandom(map, config.ObstacleDensity, rng, config.Start, config.Goal);
                    break;
            }

            return map;
        }

        private static void FillRandom(
            GridMap map,
            double obstacleDensity,
            Random rng,
            GridNode start,
            GridNode goal)
        {
            int width = map.Width;
            int height = map.Height;
            int total = width * height;
            int obstacles = (int)(total * obstacleDensity);

            var cells = new List<(int x, int y)>(total);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if ((x == start.X && y == start.Y) ||
                        (x == goal.X && y == goal.Y))
                    {
                        continue;
                    }
                    cells.Add((x, y));
                }
            }
            int n = cells.Count;
            for (int i = 0; i < obstacles && i < n; i++)
            {
                int j = rng.Next(i, n);
                (cells[i], cells[j]) = (cells[j], cells[i]);
                var (bx, by) = cells[i];
                map.SetBlocked(bx, by, true);
            }
        }

        private static void FillMazeLike(
                    GridMap map,
                    Random rng,
                    GridNode start,
                    GridNode goal)
        {
            int width = map.Width;
            int height = map.Height;

            // 1. Lépés: Kezdetben minden legyen fal (blokkolt).
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    map.SetBlocked(x, y, true);
                }
            }

            // Segédstruktúra a generáláshoz (Iteratív Recursive Backtracker)
            var stack = new Stack<(int x, int y)>();

            int startX = Clamp(start.X, 1, width - 2);
            int startY = Clamp(start.Y, 1, height - 2);

            if (startX % 2 == 0) startX++;
            if (startY % 2 == 0) startY++;
            if (startX >= width) startX = width - 2;
            if (startY >= height) startY = height - 2;

            if (width < 3 || height < 3) { startX = 0; startY = 0; }

            map.SetBlocked(startX, startY, false);
            stack.Push((startX, startY));

            // Irányok: Fel, Jobbra, Le, Balra (csak ortogonális)
            var directions = new (int dx, int dy)[]
            {
                (0, -2), (2, 0), (0, 2), (-2, 0)
            };

            while (stack.Count > 0)
            {
                var (cx, cy) = stack.Peek();
                var neighbors = new List<((int nx, int ny), (int wx, int wy))>();

                // Szomszédok keresése (2 lépés távolságra)
                foreach (var (dx, dy) in directions)
                {
                    int nx = cx + dx;
                    int ny = cy + dy;

                    // Ellenőrizzük, hogy a szomszéd a pályán belül van-e
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        // Ha a célpont még fal (visited check helyett a blocked állapotot nézzük)
                        if (map.IsBlocked(new GridNode(nx, ny)))
                        {
                            // A köztes fal koordinátája
                            int wx = cx + (dx / 2);
                            int wy = cy + (dy / 2);
                            neighbors.Add(((nx, ny), (wx, wy)));
                        }
                    }
                }

                if (neighbors.Count > 0)
                {
                    // Véletlenszerű szomszéd kiválasztása
                    var chosen = neighbors[rng.Next(neighbors.Count)];
                    var (nx, ny) = chosen.Item1;
                    var (wx, wy) = chosen.Item2;

                    // Fal lebontása a kettő között és az új cella kivésése
                    map.SetBlocked(wx, wy, false); // Köztes fal
                    map.SetBlocked(nx, ny, false); // Új cella

                    stack.Push((nx, ny));
                }
                else
                {
                    // Nincs látogatható szomszéd, visszalépés
                    stack.Pop();
                }
            }

            // 2. Lépés: Biztosítjuk, hogy a Start és Goal pontok hozzáférhetőek legyenek.
            // Mivel a labirintus generálás néha falba teheti az eredeti koordinátákat
            // (ha azok páros koordinátákra estek a rácson), ezért manuálisan megnyitjuk őket
            // és a legközelebbi szabad utat.

            EnsureAccessible(map, start);
            EnsureAccessible(map, goal);
        }

        // Segédfüggvény: Ha a pont falban van, kinyitjuk és összekötjük egy szomszédos szabad hellyel
        private static void EnsureAccessible(GridMap map, GridNode node)
        {
            // Először is, legyen szabad maga a pont
            map.SetBlocked(node.X, node.Y, false);

            // Megnézzük, van-e szabad szomszédja. Ha nincs, ki kell ütnünk egy falat.
            bool hasOpenNeighbor = false;
            var neighbors = new (int dx, int dy)[] { (0, -1), (1, 0), (0, 1), (-1, 0) };

            foreach (var (dx, dy) in neighbors)
            {
                int nx = node.X + dx;
                int ny = node.Y + dy;
                if (nx >= 0 && nx < map.Width && ny >= 0 && ny < map.Height)
                {
                    if (!map.IsBlocked(new GridNode(nx, ny)))
                    {
                        hasOpenNeighbor = true;
                        break;
                    }
                }
            }

            // Ha teljesen be van falazva (elszigetelt), nyissunk egy utat egy tetszőleges irányba,
            // ami a pályán belül van.
            if (!hasOpenNeighbor)
            {
                foreach (var (dx, dy) in neighbors)
                {
                    int nx = node.X + dx;
                    int ny = node.Y + dy;
                    if (nx >= 0 && nx < map.Width && ny >= 0 && ny < map.Height)
                    {
                        map.SetBlocked(nx, ny, false);
                        // Elég egyet kinyitni, az valószínűleg már csatlakozik a labirintushoz,
                        // mivel a labirintus nagyon sűrű.
                        break;
                    }
                }
            }
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
