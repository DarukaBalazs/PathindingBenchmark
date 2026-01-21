using Harness.Algorithms;
using Harness.Model;
using Harness.Output;
using Harness.Scenario;
using src.Core.Grids;
using src.Core.Interfaces;
using src.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Harness
{
    public sealed class BenchmarkRunner
    {
        private readonly IReadOnlyList<ScenarioConfig> _scenarios;
        private readonly IReadOnlyList<AlgorithmKind> _algorithms;
        private readonly string _outputDirectory;
        private readonly int _warmupRuns;
        private readonly bool _regenerateMapEachRun;

        public BenchmarkRunner(
            IReadOnlyList<ScenarioConfig> scenarios,
            IReadOnlyList<AlgorithmKind> algorithms,
            string outputDirectory,
            int warmupRuns = 2,
            bool regenerateMapEachRun = false)
        {
            _scenarios = scenarios ?? throw new ArgumentNullException(nameof(scenarios));
            _algorithms = algorithms ?? throw new ArgumentNullException(nameof(algorithms));
            _outputDirectory = outputDirectory ?? throw new ArgumentNullException(nameof(outputDirectory));
            _warmupRuns = Math.Max(0, warmupRuns);
            _regenerateMapEachRun = regenerateMapEachRun;

            Directory.CreateDirectory(_outputDirectory);
        }

        public void RunAll()
        {
            string csvPath = Path.Combine(_outputDirectory, "results.csv");
            using var writer = new CsvResultWriter(csvPath);

            var globalSummary = new List<object>();

            foreach (var scenario in _scenarios)
            {
                Console.WriteLine($"[INFO] Scenario: {scenario.Name} (repetitions={scenario.Repetitions}, seed={scenario.Seed})");

                var perAlgorithmRecords = new Dictionary<string, List<dynamic>>();
                foreach (var a in _algorithms)
                    perAlgorithmRecords[AlgorithmFactory.GetName(a)] = new List<dynamic>();

                for (int run = 0; run < scenario.Repetitions; run++)
                {
                    int seed = scenario.Seed + run;
                    var runScenario = CloneScenarioWithSeed(scenario, seed);

                    var map = ScenarioGenerator.BuildGrid(runScenario);

                    if (run % 10 == 0) Console.WriteLine($"  [RUN] scenario={scenario.Name} run={run} seed={seed} map={scenario.MapKind}");

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    foreach (var algoKind in _algorithms)
                    {
                        if (algoKind == AlgorithmKind.JPS && !scenario.AllowDiagonal)
                        {
                            continue;
                        }
                        string algoName = AlgorithmFactory.GetName(algoKind);
                        var pathfinder = AlgorithmFactory.Create(algoKind, scenario.PathfinderConfig.WeightW);

                        long memStart = GC.GetAllocatedBytesForCurrentThread();

                        var sw = Stopwatch.StartNew();

                        Result<GridNode> result = pathfinder.Solve(
                            scenario.Start,
                            scenario.Goal,
                            map,
                            scenario.PathfinderConfig);

                        sw.Stop();

                        long memEnd = GC.GetAllocatedBytesForCurrentThread();
                        long allocatedBytes = memEnd - memStart;

                        result.ElapsedMs = (long)sw.Elapsed.TotalMilliseconds;

                        if (run == 0)
                        {
                            string imgDir = Path.Combine(_outputDirectory, "images");
                            Directory.CreateDirectory(imgDir);

                            string safeScenarioName = scenario.Name.Replace(" ", "_");
                            string imgName = $"{safeScenarioName}_{algoName}.bmp";
                            string imgPath = Path.Combine(imgDir, imgName);

                            try
                            {
                                MapVisualizer.SaveToBmp(imgPath, map, result.Path, scenario.Start, scenario.Goal);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[WARN] Could not save image: {ex.Message}");
                            }
                        }

                        var row = BenchmarkResultRow.FromResult(
                            algoName,
                            scenario.Name,
                            run,
                            scenario.Width,
                            scenario.Height,
                            scenario.ObstacleDensity,
                            scenario.MapKind.ToString(),
                            scenario.AllowDiagonal,
                            scenario.Start,
                            scenario.Goal,
                            scenario.PathfinderConfig,
                            result,
                            allocatedBytes
                        );

                        try
                        {
                            writer.Append(row);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ERROR] writer.Append failed for scenario={scenario.Name} algo={algoName} run={run}: {ex}");
                            throw;
                        }

                        perAlgorithmRecords[algoName].Add(new
                        {
                            elapsedMs = result.ElapsedMs,
                            allocatedBytes = allocatedBytes,
                            found = result.Found,
                            expansions = result.Expansions,
                            pathLength = result.PathLength,
                            pathCost = result.PathCost
                        });
                    }
                }

                foreach (var kv in perAlgorithmRecords)
                {
                    var algoName = kv.Key;
                    var records = kv.Value;
                    globalSummary.Add(new
                    {
                        scenario = scenario.Name,
                        algorithm = algoName,
                        runs = records.Count,
                        stats = SummarizeDynamicList(records)
                    });
                }
            }

            string summaryPath = Path.Combine(_outputDirectory, "summary.json");
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(summaryPath, JsonSerializer.Serialize(globalSummary, jsonOptions));

            Console.WriteLine($"[INFO] All runs finished. Outputs in: {_outputDirectory}");
        }

        private static object SummarizeDynamicList(List<dynamic> records)
        {
            var times = new List<long>();
            var mems = new List<long>();

            foreach (dynamic r in records)
            {
                times.Add((long)r.elapsedMs);
                mems.Add((long)r.allocatedBytes);
            }

            times.Sort();
            mems.Sort();

            dynamic GetStats(List<long> data)
            {
                if (data.Count == 0) return new { min = 0, avg = 0.0, median = 0, p95 = 0 };
                double avg = data.Average();
                long median = data[data.Count / 2];
                long p95 = data[(int)Math.Ceiling(data.Count * 0.95) - 1];
                return new
                {
                    min = data[0],
                    avg = avg,
                    median = median,
                    p95 = p95
                };
            }

            return new
            {
                time = GetStats(times),
                memory = GetStats(mems)
            };
        }

        private static ScenarioConfig CloneScenarioWithSeed(ScenarioConfig s, int seed)
        {
            return new ScenarioConfig
            {
                Name = s.Name,
                Width = s.Width,
                Height = s.Height,
                ObstacleDensity = s.ObstacleDensity,
                MapKind = s.MapKind,
                AllowDiagonal = s.AllowDiagonal,
                Seed = seed,
                Start = s.Start,
                Goal = s.Goal,
                Repetitions = s.Repetitions,
                PathfinderConfig = s.PathfinderConfig
            };
        }
    }
}