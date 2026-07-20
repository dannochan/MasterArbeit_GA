# MA_GA — Genetic Algorithm for Business Component Identification

MA_GA is a .NET console application developed for a master's thesis. It models a software system as a graph of components and relations, then uses a genetic algorithm (GA) to search for module partitions that optimize cohesion, coupling, and modularity. A greedy graph-partitioning algorithm is included as a baseline for comparison against the GA results.

## Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Target framework: `net9.0`
- Cross-platform (Windows, Linux, macOS)

## Third-Party Dependencies

Declared in [MA_GA.csproj](MA_GA.csproj):

| Package | Version | Purpose |
|---|---|---|
| [GeneticSharp](https://github.com/giacomelli/GeneticSharp) | 3.1.4 | Genetic algorithm engine (population, selection, crossover, mutation, termination) |
| [QuikGraph](https://github.com/KeRNeLith/QuikGraph) | 2.5.0 | Graph data structures used to represent components and their relations |
| [QuikGraph.Graphviz](https://github.com/KeRNeLith/QuikGraph) | 2.5.0 | Graphviz/DOT export for graph visualization |
| [CsvHelper](https://joshclose.github.io/CsvHelper/) | 33.1.0 | Writing experiment results to CSV |
| [Microsoft.Extensions.Logging](https://learn.microsoft.com/dotnet/core/extensions/logging) + `.Console` | 9.0.5 | Structured console logging |

The test project ([MA_GA.Tests](MA_GA.Tests/MA_GA.Tests.csproj)) additionally uses **xUnit** and **FluentAssertions**.

## Getting Started

```bash
# restore dependencies
dotnet restore

# build
dotnet build

# run the program (executes the GA experiments defined in Program.cs)
dotnet run
```

Run the test suite:

```bash
dotnet test MA_GA.Tests/MA_GA.Tests.csproj
```

`Program.cs` reads an input graph from `data/`, runs the greedy partitioning algorithm once for reference, then executes the GA across a set of parameter combinations (an L27 orthogonal design), each replicated multiple times in parallel. Results are appended to `output/GeneticAlgorithmResults.csv`.

## Architecture

```
MA_GA/
├── Program.cs                  Entry point: loads data, runs greedy + GA experiments, writes CSV output
├── domain/
│   ├── Graph.cs                 In-memory graph of DataObjects/ObjectRelations, backed by QuikGraph
│   ├── GraphService.cs          Graph query/display helpers
│   ├── UndirectedModularityMeasurer.cs   Modularity metric computation
│   ├── geneticalgorithm/
│   │   ├── encoding/            Linear-linkage chromosome encoding for graph partitions
│   │   ├── crossover/           GroupCrossover operator
│   │   ├── mutation/            GraftMutator operator
│   │   ├── objective/           Cohesion, coupling and modularity objectives (multi-objective evaluation)
│   │   ├── fitnessfunction/     Fitness aggregation
│   │   ├── reinsertion/         Elitist reinsertion strategy
│   │   ├── termination/         Convergence-based termination criteria
│   │   ├── parameter/           GA and relation-weight parameter objects
│   │   └── engine/              GA engine setup (GeneticSharp) and orchestration
│   ├── greedyalgorithm/         Greedy graph-partitioning baseline algorithm
│   └── module/                  Module abstraction and services built from a partitioned graph
├── models/
│   ├── DataObject.cs, ObjectRelation.cs, ObjectType.cs, RelationType.cs   Core domain entities
│   ├── enums/                   ObjectiveType, OptimizationType
│   ├── MappingModels/           DTOs for mapping raw JSON input to domain objects
│   ├── optimizationresult/      GA execution/result models (incl. Pareto-optimal solutions)
│   └── ObjectHelper.cs, GraphObject.cs, GraphConnectivityInSpector.cs
├── modularisation/
│   └── ModularisableElement.cs  Abstract base class for modularisable entities
├── util/
│   └── CsvGenerator.cs          Appends optimization results to CSV
├── data/                        Input test cases (JSON): SmallTestcase.json, BigTestcase-2.json
├── result/, output/             Generated CSV results from experiment runs
└── MA_GA.Tests/                 xUnit test suite (graph, module, encoding, objective, crossover, mutation tests)
```

**Flow:** input JSON (`data/`) → `ObjectHelper` maps it into a `Graph` of `DataObject`/`ObjectRelation` → the greedy algorithm partitions the graph as a baseline → `MainGeneticAlgorithmEngine` runs GeneticSharp with the custom encoding, crossover, mutation, objectives and termination criteria → results are collected as `GeneticAlgorithmExecutionResult` and exported via `CsvGenerator` to `output/`.

## Input / Output Data

- **Input**: JSON files in `data/` describe components (`informationObjects`) and their relations, with optional external components.
- **Output**: CSV files (`output/GeneticAlgorithmResults.csv`, and historical runs under `result/`) record per-experiment parameters (population size, crossover/mutation rate, tournament size, max generations), best fitness, execution time, resulting component counts, and per-generation fitness history.
