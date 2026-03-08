using FluentAssertions;
using GeneticSharp;
using MA_GA.domain.geneticalgorithm.crossover;
using MA_GA.domain.geneticalgorithm.encoding;
using MA_GA.domain.geneticalgorithm.parameter;
using MA_GA.Models;
using Xunit;

namespace MA_GA.Tests;

public class GroupCrossoverTests
{
    // 4-node graph: 0→1, 2→3 (two disconnected pairs)
    private static Graph BuildFourNodeGraph()
    {
        var weight = new DataObjectRelationWeight();
        var graph = new Graph(weight);
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

    private static LinearLinkageEncoding MakeEncoding(Graph graph, int[] geneValues) =>
        new LinearLinkageEncoding(graph, geneValues.Select(v => new Gene(v)).ToList());

    [Fact]
    public void Constructor_NullGraph_ThrowsArgumentNullException()
    {
        var act = () => new GroupCrossover(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Cross_ReturnsTwoOffspring()
    {
        var graph = BuildFourNodeGraph();
        var crossover = new GroupCrossover(graph);
        // Parent 1: two modules {0,1} and {2,3}
        // Parent 2: four singletons {0}, {1}, {2}, {3}
        var parent1 = MakeEncoding(graph, [1, 1, 3, 3]);
        var parent2 = MakeEncoding(graph, [0, 1, 2, 3]);

        var offspring = crossover.Cross(new List<IChromosome> { parent1, parent2 });

        offspring.Should().HaveCount(2);
    }

    [Fact]
    public void Cross_OffspringAreSameLengthAsParents()
    {
        var graph = BuildFourNodeGraph();
        var crossover = new GroupCrossover(graph);
        var parent1 = MakeEncoding(graph, [1, 1, 3, 3]);
        var parent2 = MakeEncoding(graph, [0, 1, 2, 3]);

        var offspring = crossover.Cross(new List<IChromosome> { parent1, parent2 });

        offspring[0].Length.Should().Be(4);
        offspring[1].Length.Should().Be(4);
    }

    [Fact]
    public void Cross_OffspringAreValidEncodings()
    {
        var graph = BuildFourNodeGraph();
        var crossover = new GroupCrossover(graph);
        var parent1 = MakeEncoding(graph, [1, 1, 3, 3]);
        var parent2 = MakeEncoding(graph, [0, 1, 2, 3]);

        var offspring = crossover.Cross(new List<IChromosome> { parent1, parent2 });

        ((LinearLinkageEncoding)offspring[0]).IsValid().Should().BeTrue();
        ((LinearLinkageEncoding)offspring[1]).IsValid().Should().BeTrue();
    }

    [Fact]
    public void Cross_IdenticalSingletonParents_OffspringAreValid()
    {
        // Both parents are all-singletons [0,1,2,3]. Every position is an ending node
        // in both parents, so crossover is deterministic. Connected nodes (0↔1, 2↔3)
        // get merged by the validity fix, producing 2 graph-connected modules.
        var graph = BuildFourNodeGraph();
        var crossover = new GroupCrossover(graph);
        var parent1 = MakeEncoding(graph, [0, 1, 2, 3]);
        var parent2 = MakeEncoding(graph, [0, 1, 2, 3]);

        var offspring = crossover.Cross(new List<IChromosome> { parent1, parent2 });

        ((LinearLinkageEncoding)offspring[0]).IsValid().Should().BeTrue();
        ((LinearLinkageEncoding)offspring[1]).IsValid().Should().BeTrue();
        ((LinearLinkageEncoding)offspring[0]).GetModules().Should().HaveCount(2);
        ((LinearLinkageEncoding)offspring[1]).GetModules().Should().HaveCount(2);
    }

    [Fact]
    public void Cross_IdenticalTwoModuleParents_OffspringPreserveBothModules()
    {
        // Both parents: {0,1} and {2,3}
        // Both ending nodes (1 and 3) are ending nodes in both parents → deterministic:
        // newModules = {1: {1}, 3: {3}}, then nodes 0 and 2 are assigned
        // to the module of their parent's ending node (1 and 3 respectively).
        var graph = BuildFourNodeGraph();
        var crossover = new GroupCrossover(graph);
        var parent1 = MakeEncoding(graph, [1, 1, 3, 3]);
        var parent2 = MakeEncoding(graph, [1, 1, 3, 3]);

        var offspring = crossover.Cross(new List<IChromosome> { parent1, parent2 });

        var lle = (LinearLinkageEncoding)offspring[0];
        lle.GetModules().Should().HaveCount(2);
        lle.GetModules().Should().ContainSingle(m => m.GetIndices().SequenceEqual(new[] { 0, 1 }));
        lle.GetModules().Should().ContainSingle(m => m.GetIndices().SequenceEqual(new[] { 2, 3 }));
    }
}
