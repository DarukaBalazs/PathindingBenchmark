using System;
using src.Core.Grids;
using src.Core.Models;

namespace Harness.Model
{
    /// <summary>
    /// Egyetlen benchmark futás "kilapított" sora, amit közvetlenül CSV-be írunk.
    /// </summary>
    public sealed class BenchmarkResultRow
    {
        // Azonosítók / meta
        public string Algorithm { get; init; } = string.Empty;
        public string ScenarioName { get; init; } = string.Empty;
        public int RunIndex { get; init; }

        // Térkép paraméterek
        public int MapWidth { get; init; }
        public int MapHeight { get; init; }
        public double ObstacleDensity { get; init; }
        public string MapType { get; init; } = string.Empty;
        public bool AllowDiagonal { get; init; }

        // Start–goal (CSV-barát formátumban: "x,y")
        public string Start { get; init; } = string.Empty;
        public string Goal { get; init; } = string.Empty;

        // ÚJ MEZŐ: Légvonalbeli (Euklideszi) távolság a Start és Goal között
        public double LinearDistance { get; init; }

        // Algoritmus paraméterek
        public double WeightW { get; init; }
        public int TimeLimitMs { get; init; }
        public int MaxExpansions { get; init; }
        public bool TieBreakLowG { get; init; }

        // Metrikák
        public bool Found { get; init; }
        public double PathCost { get; init; }
        public int PathLength { get; init; }
        public long Expansions { get; init; }
        public double ElapsedMs { get; init; }

        // Memória mérés
        public long AllocBytes { get; init; }

        public static BenchmarkResultRow FromResult(
            string algorithm,
            string scenarioName,
            int runIndex,
            int mapWidth,
            int mapHeight,
            double obstacleDensity,
            string mapType,
            bool allowDiagonal,
            GridNode start,
            GridNode goal,
            src.Core.Models.PathfinderConfig cfg,
            Result<GridNode> result,
            long allocatedBytes)
        {
            // Távolság kiszámítása a start és goal node-ok alapján
            double dx = start.X - goal.X;
            double dy = start.Y - goal.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            return new BenchmarkResultRow
            {
                Algorithm = algorithm,
                ScenarioName = scenarioName,
                RunIndex = runIndex,
                MapWidth = mapWidth,
                MapHeight = mapHeight,
                ObstacleDensity = obstacleDensity,
                MapType = mapType,
                AllowDiagonal = allowDiagonal,
                Start = $"{start.X},{start.Y}",
                Goal = $"{goal.X},{goal.Y}",

                LinearDistance = dist,

                WeightW = cfg.WeightW,
                TimeLimitMs = cfg.TimeLineitMs,
                MaxExpansions = cfg.MaxExpansions,
                TieBreakLowG = cfg.TieBreakLowG,

                Found = result.Found,
                PathCost = result.PathCost,
                PathLength = result.PathLength,
                Expansions = result.Expansions,
                ElapsedMs = result.ElapsedMs,

                AllocBytes = allocatedBytes
            };
        }
    }
}