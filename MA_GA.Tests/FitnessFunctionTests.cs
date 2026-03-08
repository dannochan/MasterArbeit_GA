using FluentAssertions;
using GeneticSharp;
using MA_GA.domain.geneticalgorithm.encoding;
using MA_GA.domain.geneticalgorithm.fitnessfunction;
using MA_GA.domain.geneticalgorithm.objective;
using MA_GA.domain.geneticalgorithm.parameter;
using MA_GA.Models;
using Xunit;

namespace MA_GA.Tests;

public class FitnessFunctionTests
{
    // 4-node graph: 0→1, 2→3 (two connected pairs)
    // Encoding [1,1,3,3] → valid modules {0,1} and {2,3}, all edges internal → 0% coupling
    private static Graph BuildFourNodeGraph()
    {
        var graph = new Graph(new DataObjectRelationWeight());
        var n0 = new DataObject("N0", ObjectType.FunctionObject, "n0", false, 0);
        var n1 = new DataObject("N1", ObjectType.FunctionObject, "n1", false, 1);
        var n2 = new DataObject("N2", ObjectType.FunctionObject, "n2", false, 2);
        var n3 = new DataObject("N3", ObjectType.FunctionObject, "n3", false, 3);
        graph.AddNodeToGraph(n0);
        graph.AddNodeToGraph(n1);
        graph.AddNodeToGraph(n2);
        graph.AddNodeToGraph(n3);
        graph.AddRelationToGraph(new ObjectRelation(0, RelationType.Read, n0, n1, 1.0));
        graph.AddRelationToGraph(new ObjectRelation(1, RelationType.Read, n2, n3, 1.0));
        return graph;
    }

    // 6-node chain: 0→1→2→3→4→5
    // Encoding [1,1,3,3,5,5] → valid modules {0,1},{2,3},{4,5} with boundary edges 1→2 and 3→4
    // CouplingObjective value = 2 boundary / (2 boundary + 6 internal) × 100 = 25%
    private static Graph BuildSixNodeChainGraph()
    {
        var graph = new Graph(new DataObjectRelationWeight());
        var nodes = Enumerable.Range(0, 6)
            .Select(i => new DataObject($"N{i}", ObjectType.FunctionObject, $"n{i}", false, i))
            .ToArray();
        foreach (var n in nodes) graph.AddNodeToGraph(n);
        for (int i = 0; i < 5; i++)
            graph.AddRelationToGraph(new ObjectRelation(i, RelationType.Read, nodes[i], nodes[i + 1], 1.0));
        return graph;
    }

    // 2-node graph A(0)→B(1). Encoding [0,1] produces singleton {0} (non-isolated) →
    // IsOneModuleConsistOfOneEdge = true → penalty fires → EvaluateAll returns [0,0].
    private static Graph BuildTwoNodeGraph()
    {
        var graph = new Graph(new DataObjectRelationWeight());
        var a = new DataObject("A", ObjectType.FunctionObject, "a", false, 0);
        var b = new DataObject("B", ObjectType.FunctionObject, "b", false, 1);
        graph.AddNodeToGraph(a);
        graph.AddNodeToGraph(b);
        graph.AddRelationToGraph(new ObjectRelation(0, RelationType.Read, a, b, 1.0));
        return graph;
    }

    private static LinearLinkageEncoding MakeEncoding(Graph graph, int[] geneValues) =>
        new LinearLinkageEncoding(graph, geneValues.Select(v => new Gene(v)).ToList());

    // --- Constructor guards ---

    [Fact]
    public void Constructor_NullObjectives_ThrowsArgumentNullException()
    {
        var graph = BuildFourNodeGraph();

        var act = () => new FitnessFunction(null!, graph);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullGraph_ThrowsArgumentNullException()
    {
        var objectives = new List<Objective> { new CohesionObjective(BuildFourNodeGraph(), 1.0) };

        var act = () => new FitnessFunction(objectives, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // --- EvaluateAll shape ---

    [Fact]
    public void EvaluateAll_LengthMatchesObjectiveCount()
    {
        var graph = BuildFourNodeGraph();
        var objectives = new List<Objective>
        {
            new CohesionObjective(graph, 1.0),
            new CouplingObjective(graph, 1.0)
        };
        var fitness = new FitnessFunction(objectives, graph);
        var chromosome = MakeEncoding(graph, [1, 1, 3, 3]);

        var results = fitness.EvaluateAll(chromosome);

        results.Should().HaveCount(2);
    }

    // --- Evaluate = sum of EvaluateAll ---

    [Fact]
    public void Evaluate_EqualsSumOfEvaluateAll()
    {
        var graph = BuildFourNodeGraph();
        var objectives = new List<Objective>
        {
            new CohesionObjective(graph, 1.0),
            new CouplingObjective(graph, 1.0)
        };
        var fitness = new FitnessFunction(objectives, graph);
        var chromosome = MakeEncoding(graph, [1, 1, 3, 3]);

        var score = fitness.Evaluate(chromosome);
        var parts = fitness.EvaluateAll(chromosome);

        score.Should().BeApproximately(parts.Sum(), 1e-9);
    }

    // --- Optimization direction ---

    [Fact]
    public void EvaluateAll_MaximumObjective_ValidEncoding_ReturnsNonNegativeScore()
    {
        // CohesionObjective (Maximum) → contribution is +weighted_value ≥ 0
        var graph = BuildFourNodeGraph();
        var fitness = new FitnessFunction(
            new List<Objective> { new CohesionObjective(graph, 1.0) }, graph);
        var chromosome = MakeEncoding(graph, [1, 1, 3, 3]);

        var results = fitness.EvaluateAll(chromosome);

        results[0].Should().BeGreaterThanOrEqualTo(0.0);
    }

    [Fact]
    public void EvaluateAll_MinimumObjective_WithBoundaryEdges_ReturnsNegativeScore()
    {
        // 6-node chain, encoding [1,1,3,3,5,5] → 3 modules with boundary edges 1→2 and 3→4
        // CouplingObjective value = 2/(2+6) × 100 = 25%; Minimum → negated → -25.0
        var graph = BuildSixNodeChainGraph();
        var fitness = new FitnessFunction(
            new List<Objective> { new CouplingObjective(graph, 1.0) }, graph);
        var chromosome = MakeEncoding(graph, [1, 1, 3, 3, 5, 5]);

        var results = fitness.EvaluateAll(chromosome);

        results[0].Should().BeApproximately(-25.0, 0.001);
    }

    // --- Penalty condition ---

    [Fact]
    public void EvaluateAll_PenaltyCondition_ReturnsBothZero()
    {
        // Graph A(0)→B(1), encoding [0,1] → singletons {0} and {1}
        // {0} is non-isolated (has out-edge) and has 1 element →
        // IsOneModuleConsistOfOneEdge = true → penalty: returns [0.0, 0.0]
        var graph = BuildTwoNodeGraph();
        var objectives = new List<Objective>
        {
            new CohesionObjective(graph, 1.0),
            new CouplingObjective(graph, 1.0)
        };
        var fitness = new FitnessFunction(objectives, graph);
        var chromosome = MakeEncoding(graph, [0, 1]);

        var results = fitness.EvaluateAll(chromosome);

        results.Should().Equal(0.0, 0.0);
    }
}
