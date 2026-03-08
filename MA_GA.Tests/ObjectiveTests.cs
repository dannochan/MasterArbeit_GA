using FluentAssertions;
using MA_GA.domain.geneticalgorithm.objective;
using MA_GA.domain.geneticalgorithm.parameter;
using MA_GA.domain.module;
using MA_GA.models.enums;
using MA_GA.Models;
using Xunit;

namespace MA_GA.Tests;

public class ObjectiveTests
{
    // 2-node graph: two FunctionObjects, no edges (both isolated)
    private static Graph BuildIsolatedGraph()
    {
        var weight = new DataObjectRelationWeight();
        var graph = new Graph(weight);
        graph.AddNodeToGraph(new DataObject("F0", ObjectType.FunctionObject, "f0", false, 0));
        graph.AddNodeToGraph(new DataObject("F1", ObjectType.FunctionObject, "f1", false, 1));
        return graph;
    }

    // 3-node graph: 2 FunctionObjects + 1 InformationObject, edges: F0->I2, F1->I2
    private static (Graph graph, DataObject f0, DataObject f1, DataObject i2) BuildConnectedGraph()
    {
        var weight = new DataObjectRelationWeight();
        var graph = new Graph(weight);
        var f0 = new DataObject("F0", ObjectType.FunctionObject, "f0", false, 0);
        var f1 = new DataObject("F1", ObjectType.FunctionObject, "f1", false, 1);
        var i2 = new DataObject("I2", ObjectType.InformationObject, "i2", false, 2);
        graph.AddNodeToGraph(f0);
        graph.AddNodeToGraph(f1);
        graph.AddNodeToGraph(i2);
        graph.AddRelationToGraph(new ObjectRelation(0, RelationType.Read, f0, i2, 1.0));
        graph.AddRelationToGraph(new ObjectRelation(1, RelationType.Read, f1, i2, 1.0));
        return (graph, f0, f1, i2);
    }

    // --- CohesionObjective construction ---

    [Fact]
    public void CohesionObjective_Constructor_SetsWeight()
    {
        var graph = BuildIsolatedGraph();
        var objective = new CohesionObjective(graph, 0.5);

        objective.GetWeight().Should().Be(0.5);
    }

    [Fact]
    public void CohesionObjective_Constructor_NullGraph_ThrowsArgumentNullException()
    {
        var act = () => new CohesionObjective(null!, 1.0);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CohesionObjective_Constructor_NegativeWeight_ThrowsArgumentException()
    {
        var graph = BuildIsolatedGraph();

        var act = () => new CohesionObjective(graph, -1.0);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CohesionObjective_GetObjectiveType_ReturnsMaximiseCohesion()
    {
        var graph = BuildIsolatedGraph();
        var objective = new CohesionObjective(graph, 1.0);

        objective.GetObjectiveType().Should().Be(ObjectiveType.MAXIMISE_COHESION);
    }

    [Fact]
    public void CohesionObjective_GetOptimizationType_ReturnsMaximum()
    {
        var graph = BuildIsolatedGraph();
        var objective = new CohesionObjective(graph, 1.0);

        objective.GetOptimizationType().Should().Be(OptimizationType.Maximum);
    }

    [Fact]
    public void CohesionObjective_GetObjectiveName_ReturnsNonEmpty()
    {
        var graph = BuildIsolatedGraph();
        var objective = new CohesionObjective(graph, 1.0);

        objective.GetObjectiveName().Should().NotBeNullOrEmpty();
    }

    // --- CohesionObjective.CalculateValue ---

    [Fact]
    public void CohesionObjective_AllIsolatedModules_ReturnsZero()
    {
        var graph = BuildIsolatedGraph();
        var objective = new CohesionObjective(graph, 1.0);

        // Both modules are singletons with no outgoing edges → isolated → filtered out
        var m0 = new Module(); m0.AddIndex(0);
        var m1 = new Module(); m1.AddIndex(1);

        var result = objective.CalculateValue(new List<Module> { m0, m1 });

        result.Should().Be(0.0);
    }

    [Fact]
    public void CohesionObjective_EmptyModuleList_ReturnsZero()
    {
        var graph = BuildIsolatedGraph();
        var objective = new CohesionObjective(graph, 1.0);

        var result = objective.CalculateValue(new List<Module>());

        result.Should().Be(0.0);
    }

    [Fact]
    public void CohesionObjective_NonIsolatedModule_ReturnsPositiveValue()
    {
        var (graph, f0, f1, i2) = BuildConnectedGraph();
        var objective = new CohesionObjective(graph, 1.0);

        // Module {0, 2}: F0 and I2, connected by edge F0->I2
        var module = new Module();
        module.AddIndex(0);
        module.AddIndex(2);

        // Module {1}: isolated singleton (F1 has out-edge so IsIsolatedVertex = false,
        // but it's a single-element module — let's use a 2-element module for the second partition)
        var module1 = new Module();
        module1.AddIndex(1);

        var result = objective.CalculateValue(new List<Module> { module, module1 });

        result.Should().BeGreaterThan(0.0);
    }

    // --- CouplingObjective construction ---

    [Fact]
    public void CouplingObjective_Constructor_NullGraph_ThrowsArgumentNullException()
    {
        var act = () => new CouplingObjective(null!, 1.0);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CouplingObjective_Constructor_SetsWeight()
    {
        var graph = BuildIsolatedGraph();
        var objective = new CouplingObjective(graph, 0.7);

        objective.GetWeight().Should().Be(0.7);
    }

    [Fact]
    public void CouplingObjective_GetObjectiveType_ReturnsMinimiseCoupling()
    {
        var graph = BuildIsolatedGraph();
        var objective = new CouplingObjective(graph, 1.0);

        objective.GetObjectiveType().Should().Be(ObjectiveType.MINIMISE_COUPLING);
    }

    [Fact]
    public void CouplingObjective_GetOptimizationType_ReturnsMinimum()
    {
        var graph = BuildIsolatedGraph();
        var objective = new CouplingObjective(graph, 1.0);

        objective.GetOptimizationType().Should().Be(OptimizationType.Minimum);
    }

    // --- CouplingObjective.CalculateValue ---

    // Graph helpers for coupling scenarios
    // 2-node graph: A(0) → B(1), weight=1.0
    private static Graph BuildTwoNodeGraph(out DataObject a, out DataObject b)
    {
        var graph = new Graph(new DataObjectRelationWeight());
        a = new DataObject("A", ObjectType.FunctionObject, "a", false, 0);
        b = new DataObject("B", ObjectType.FunctionObject, "b", false, 1);
        graph.AddNodeToGraph(a);
        graph.AddNodeToGraph(b);
        graph.AddRelationToGraph(new ObjectRelation(0, RelationType.Read, a, b, 1.0));
        return graph;
    }

    // 3-node chain graph: A(0) → B(1) → C(2), weight=1.0 each
    private static Graph BuildThreeNodeChainGraph(out DataObject a, out DataObject b, out DataObject c)
    {
        var graph = new Graph(new DataObjectRelationWeight());
        a = new DataObject("A", ObjectType.FunctionObject, "a", false, 0);
        b = new DataObject("B", ObjectType.FunctionObject, "b", false, 1);
        c = new DataObject("C", ObjectType.FunctionObject, "c", false, 2);
        graph.AddNodeToGraph(a);
        graph.AddNodeToGraph(b);
        graph.AddNodeToGraph(c);
        graph.AddRelationToGraph(new ObjectRelation(0, RelationType.Read, a, b, 1.0));
        graph.AddRelationToGraph(new ObjectRelation(1, RelationType.Read, b, c, 1.0));
        return graph;
    }

    [Fact]
    public void CouplingObjective_AllEdgesInternal_ReturnsZero()
    {
        // Graph: A→B (weight 1)
        // Module {A, B}: A→B is fully internal → no boundary edges → 0% coupling
        var graph = BuildTwoNodeGraph(out var a, out var b);
        var objective = new CouplingObjective(graph, 1.0);
        var module = new Module();
        module.AddIndex(0); // A
        module.AddIndex(1); // B

        var result = objective.CalculateValue(new List<Module> { module });

        result.Should().Be(0.0);
    }

    [Fact]
    public void CouplingObjective_AllEdgesBoundary_ReturnsOneHundred()
    {
        // Graph: A(0)→B(1)
        // Module {A} (non-isolated), Module {B} (isolated — no outgoing edges)
        // Only {A} is processed; A→B is a boundary edge (B not in {A})
        // totalCoupling=1, internal=0 → 100% coupling
        var graph = BuildTwoNodeGraph(out var a, out var b);
        var objective = new CouplingObjective(graph, 1.0);
        var mA = new Module(); mA.AddIndex(0);
        var mB = new Module(); mB.AddIndex(1);

        var result = objective.CalculateValue(new List<Module> { mA, mB });

        result.Should().Be(100.0);
    }

    [Fact]
    public void CouplingObjective_OneBoundaryOneInternal_ReturnsOneThird()
    {
        // Graph: A(0)→B(1)→C(2), each edge weight=1
        // Module {A,B}: A→B is internal; B→C is boundary (weight 1 each)
        // Module {C}: isolated (no outgoing) → filtered out
        // GetModuleEdges uses GetVertexEdgesByIndex (source OR target), so A→B appears
        // twice in the internal sum (once from A's list, once from B's list):
        //   sumOfModuleEdgesWeights = 2 × weight(A→B) = 2
        //   totalCoupling           = 1 × weight(B→C) = 1
        //   allEdgesWeights         = 1 + 2 = 3
        //   result                  = 1/3 × 100 ≈ 33.33%
        var graph = BuildThreeNodeChainGraph(out _, out _, out _);
        var objective = new CouplingObjective(graph, 1.0);
        var mAB = new Module(); mAB.AddIndex(0); mAB.AddIndex(1);
        var mC  = new Module(); mC.AddIndex(2);

        var result = objective.CalculateValue(new List<Module> { mAB, mC });

        result.Should().BeApproximately(100.0 / 3.0, 0.001);
    }
}
