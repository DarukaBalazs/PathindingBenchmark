# PathfindingBench

High-performance benchmarking framework for graph and grid-based pathfinding algorithms in C# (.NET 8).

This project was created as part of a **Performance Optimized Computing** university course, with the goal of analyzing **runtime, memory behavior, and algorithmic efficiency** of different pathfinding strategies under controlled, reproducible conditions.

---

## Key Features

- Unified benchmark harness with strict isolation (warmup, GC control, deterministic seeds)
- Multiple pathfinding algorithms with identical interfaces
- Hardware-independent efficiency metrics (node expansions)
- Detailed memory and GC pressure analysis
- Designed for large-scale grids (up to 1024×1024)

---

## Algorithms Compared

### Dijkstra (Reference Baseline)
- Implemented as Weighted A* with zero heuristic
- Guarantees optimal paths
- Serves as correctness and performance baseline

### Weighted A* (General-Purpose Optimized Search)
- Manhattan heuristic with configurable weight (w = 1.2)
- Trades slight optimality loss for massive speedups
- Industry-standard choice for real-time systems

### Jump Point Search (JPS)
- Grid-specific structural optimization
- Skips symmetric paths and intermediate nodes
- Extremely low expansions and memory usage on open maps

---

## Benchmark Methodology

The benchmark follows a strict lifecycle to ensure fair and stable measurements:

- **Setup – Warmup – Measure – Teardown**
- Fixed random seeds for deterministic map generation
- Explicit `GC.Collect()` before measurements
- JIT warmup runs excluded from results

### Metrics Collected

- Runtime (mean, median)
- Node expansions (algorithmic work)
- Allocated bytes (heap pressure)
- GC collections (Gen0–Gen2)
- Path optimality ratio (PathCost / LinearDistance)

---

## Test Scenarios

- **Random grids** (up to 1024×1024)
- **Maze-like environments** (high constraint density)
- 4-directional and diagonal movement
- Identical start–goal pairs across algorithms

![Random Grid Benchmark](Images/5_Maze_vs_Random_NoDiag.png)
![Maze Benchmark](9000_Maze_100x100_corner_NoDiag_Dijkstra.bmp)

---

## Key Results

### Performance (1024×1024 Random Grid)

| Algorithm      | Avg Time | Speedup vs Dijkstra |
|----------------|----------|---------------------|
| Dijkstra       | ~709 ms  | 1×                  |
| Weighted A*    | ~1.38 ms | ~513×               |
| JPS            | ~2.05 ms | ~346×               |

![Runtime Comparison](img_runtime.png)

---

### Memory Usage

| Algorithm   | Allocated Memory |
|------------|------------------|
| Dijkstra   | ~337 MB          |
| Weighted A*| ~1 MB            |
| JPS        | ~1 MB            |

![Memory Usage](img_memory.png)

Dijkstra scales poorly due to large `Dictionary` and `HashSet` usage, while JPS and A* drastically reduce heap pressure.

---

### Algorithmic Efficiency (Node Expansions)

| Algorithm   | Avg Expansions |
|------------|----------------|
| Dijkstra   | ~600,000       |
| Weighted A*| ~895           |
| JPS        | ~663           |

![Expansions](img_expansions.png)

Speedups are driven by **eliminating unnecessary work**, not micro-optimizations.

---

### Path Optimality

| Algorithm   | Optimality Index |
|------------|------------------|
| Dijkstra   | ~1.04 (optimal) |
| JPS        | ~1.05 (optimal) |
| Weighted A*| ~1.09 (+4–5%)   |

Weighted A* sacrifices minimal path quality for orders-of-magnitude performance gains.

---

## Architecture Overview

- `IPathfinder<TNode>` – unified algorithm interface
- `IGraph / IGrid` – abstract search space
- Custom binary heap (`MinPriorityQueue`)
- Pluggable heuristics (Manhattan, Euclidean, Chebyshev)
- Structured `Result` object with metrics

![Architecture Diagram](img_architecture.png)

---

## Conclusion

There is no universally “best” pathfinding algorithm:

- **Dijkstra** is only viable for small graphs or theoretical reference
- **Weighted A*** offers the best robustness across environments
- **JPS** delivers extreme performance on suitable grid topologies

The benchmark demonstrates how **algorithmic choices dominate performance**, especially in memory-managed runtimes like .NET.

---

## Tech Stack

- C# / .NET 8
- Custom benchmark harness
- Deterministic test generation
- CSV/JSON result export
