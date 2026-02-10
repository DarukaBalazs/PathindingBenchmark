using Harness;
using Harness.Algorithms;
using Harness.Scenario;
using src.Core.Grids;
using src.Core.Models;
using System;
using System.Collections.Generic;

namespace HarnessApp
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            string outDir = args.Length > 0 ? args[0] : "results";

            int splitRuns = 50;

            Console.WriteLine($"[INFO] Output dir: {outDir}");
            Console.WriteLine($"[INFO] Repetitions per Split-Scenario: {splitRuns} (Total 100 per map type)");

            var scenarios = BuildDefaultScenarios(repetitions: splitRuns);

            var algorithms = new List<AlgorithmKind>
            {
                AlgorithmKind.Dijkstra,
                AlgorithmKind.WeightedAStar,
                AlgorithmKind.JPS
            };

            var runner = new BenchmarkRunner(scenarios, algorithms, outDir, warmupRuns: 3, regenerateMapEachRun: true);
            runner.RunAll();

            Console.WriteLine("[INFO] Done.");
        }

        public static List<ScenarioConfig> BuildDefaultScenarios(int repetitions = 50)
        {
            var list = new List<ScenarioConfig>();

            int timeBasedSeed = (int)DateTime.Now.Ticks;
            var masterRng = new Random(timeBasedSeed);

            Console.WriteLine($"[INIT] Generating scenarios with Master Seed: {timeBasedSeed}");

            var sizes = new (int w, int h)[]
            {
                (100,100),
                (512,512),
                (1024,1024)
            };

            var densities = new double[] { 0.10, 0.30 };

            int scenarioId = 0;
            foreach (var (w, h) in sizes)
            {
                foreach (var density in densities)
                {
                    int pairSeedCorner = masterRng.Next();
                    int pairSeedRandom = masterRng.Next();

                    var start = new GridNode(0, 0);
                    var goal = new GridNode(w - 1, h - 1);
                    var cfg = new PathfinderConfig { WeightW = 1.2, TieBreakLowG = true };

                    list.Add(new ScenarioConfig
                    {
                        Name = $"{scenarioId}_{w}x{h}_dens{density:0.00}_corner_NoDiag",
                        Width = w,
                        Height = h,
                        ObstacleDensity = density,
                        MapKind = MapKind.Random,
                        AllowDiagonal = false,
                        Seed = pairSeedCorner,
                        Start = start,
                        Goal = goal,
                        Repetitions = repetitions,
                        PathfinderConfig = cfg
                    });

                    list.Add(new ScenarioConfig
                    {
                        Name = $"{scenarioId}_{w}x{h}_dens{density:0.00}_corner_Diag",
                        Width = w,
                        Height = h,
                        ObstacleDensity = density,
                        MapKind = MapKind.Random,
                        AllowDiagonal = true,  
                        Seed = pairSeedCorner, 
                        Start = start,
                        Goal = goal,
                        Repetitions = repetitions,
                        PathfinderConfig = cfg
                    });
                    scenarioId++;

                    var rngLocal = new Random(pairSeedRandom);
                    var rStart = new GridNode(rngLocal.Next(0, w), rngLocal.Next(0, h));
                    var rGoal = new GridNode(rngLocal.Next(0, w), rngLocal.Next(0, h));
                    while (rStart.Equals(rGoal)) rGoal = new GridNode(rngLocal.Next(0, w), rngLocal.Next(0, h));

                    list.Add(new ScenarioConfig
                    {
                        Name = $"{scenarioId}_{w}x{h}_dens{density:0.00}_random_NoDiag",
                        Width = w,
                        Height = h,
                        ObstacleDensity = density,
                        MapKind = MapKind.Random,
                        AllowDiagonal = false,
                        Seed = pairSeedRandom,
                        Start = rStart,
                        Goal = rGoal,
                        Repetitions = repetitions,
                        PathfinderConfig = cfg
                    });

                    list.Add(new ScenarioConfig
                    {
                        Name = $"{scenarioId}_{w}x{h}_dens{density:0.00}_random_Diag",
                        Width = w,
                        Height = h,
                        ObstacleDensity = density,
                        MapKind = MapKind.Random,
                        AllowDiagonal = true,
                        Seed = pairSeedRandom,
                        Start = rStart,
                        Goal = rGoal,
                        Repetitions = repetitions,
                        PathfinderConfig = cfg
                    });
                    scenarioId++;
                }
            }

            var mazeSizes = new (int w, int h)[] { (100, 100), (512, 512) };
            int mazeScenarioId = 9000;

            foreach (var (w, h) in mazeSizes)
            {
                int pairSeedCorner = masterRng.Next();
                int pairSeedRandom = masterRng.Next();
                var cfg = new PathfinderConfig { WeightW = 1.2, TieBreakLowG = true };
                var start = new GridNode(1, 1);
                var goal = new GridNode(w - 2, h - 2);

                list.Add(new ScenarioConfig
                {
                    Name = $"{mazeScenarioId}_Maze_{w}x{h}_corner_NoDiag",
                    Width = w,
                    Height = h,
                    MapKind = MapKind.MazeLike,
                    AllowDiagonal = false,
                    Seed = pairSeedCorner,
                    Start = start,
                    Goal = goal,
                    Repetitions = repetitions,
                    PathfinderConfig = cfg
                });

                mazeScenarioId++;

                var rngLocal = new Random(pairSeedRandom);
                var rStart = new GridNode(rngLocal.Next(1, w - 1), rngLocal.Next(1, h - 1));
                var rGoal = new GridNode(rngLocal.Next(1, w - 1), rngLocal.Next(1, h - 1));

                list.Add(new ScenarioConfig
                {
                    Name = $"{mazeScenarioId}_Maze_{w}x{h}_randomEndpoints_NoDiag",
                    Width = w,
                    Height = h,
                    MapKind = MapKind.MazeLike,
                    AllowDiagonal = false,
                    Seed = pairSeedRandom,
                    Start = rStart,
                    Goal = rGoal,
                    Repetitions = repetitions,
                    PathfinderConfig = cfg
                });
                mazeScenarioId++;
            }

            return list;
        }
    }
}