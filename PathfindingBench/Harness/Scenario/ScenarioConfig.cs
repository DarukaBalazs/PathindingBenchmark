using src.Core.Grids;

namespace Harness.Scenario
{
    public enum MapKind
    {
        Random,
        MazeLike
    }

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

        public int Repetitions { get; init; } = 5;

        public src.Core.Models.PathfinderConfig PathfinderConfig { get; init; }
            = new();
    }
}
