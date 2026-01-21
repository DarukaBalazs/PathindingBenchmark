using src.Core.Grids;

namespace Harness.Scenario
{
    public enum MapKind
    {
        Random,
        MazeLike
    }

    /// <summary>
    /// Egy benchmark-szcenárió paraméterei.
    /// Ez még nem generál térképet, csak leírja.
    /// </summary>
    public sealed class ScenarioConfig
    {
        public string Name { get; init; } = string.Empty;

        public int Width { get; init; }
        public int Height { get; init; }
        public double ObstacleDensity { get; init; }
        public MapKind MapKind { get; init; }
        public bool AllowDiagonal { get; init; }

        public int Seed { get; init; }

        public GridNode Start { get; init; }
        public GridNode Goal { get; init; }

        /// <summary>
        /// Hányszor futtatjuk ugyanazt a szcenáriót (repeat / warmup+measurement).
        /// </summary>
        public int Repetitions { get; init; } = 5;

        /// <summary>
        /// Algoritmus-specifikus config: Weighted A* súlya, time limit stb.
        /// </summary>
        public src.Core.Models.PathfinderConfig PathfinderConfig { get; init; }
            = new();
    }
}
