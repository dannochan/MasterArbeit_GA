using FluentAssertions;
using GeneticSharp;
using MA_GA.domain.geneticalgorithm.encoding;
using MA_GA.domain.geneticalgorithm.parameter;
using MA_GA.Models;
using Xunit;

namespace MA_GA.Tests;

public class LinearLinkageEncodingTests
{
    // Build a 4-node graph: indices 0, 1, 2, 3 (all FunctionObjects)
    private static Graph BuildFourNodeGraph()
    {
        var weight = new DataObjectRelationWeight();
        var graph = new Graph(weight);
        graph.AddNodeToGraph(new DataObject("N0", ObjectType.FunctionObject, "n0", false, 0));
        graph.AddNodeToGraph(new DataObject("N1", ObjectType.FunctionObject, "n1", false, 1));
        graph.AddNodeToGraph(new DataObject("N2", ObjectType.FunctionObject, "n2", false, 2));
        graph.AddNodeToGraph(new DataObject("N3", ObjectType.FunctionObject, "n3", false, 3));
        return graph;
    }

    private static LinearLinkageEncoding MakeEncoding(Graph graph, int[] geneValues) =>
        new LinearLinkageEncoding(graph, geneValues.Select(v => new Gene(v)).ToList());

    // --- Gene access ---

    [Fact]
    public void GetIntegerGene_ReturnsCorrectValue()
    {
        var graph = BuildFourNodeGraph();
        var encoding = MakeEncoding(graph, [0, 1, 2, 3]);

        ((int)encoding.GetIntegerGene(0).Value).Should().Be(0);
        ((int)encoding.GetIntegerGene(2).Value).Should().Be(2);
    }

    [Fact]
    public void GetIntegerGene_OutOfRange_ThrowsArgumentOutOfRangeException()
    {
        var graph = BuildFourNodeGraph();
        var encoding = MakeEncoding(graph, [0, 1, 2, 3]);

        var act = () => encoding.GetIntegerGene(99);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetChromosomeLength_MatchesGeneCount()
    {
        var graph = BuildFourNodeGraph();
        var encoding = MakeEncoding(graph, [0, 1, 2, 3]);

        encoding.GetChromosomeLength().Should().Be(4);
    }

    // --- Module determination ---

    [Fact]
    public void AllSelfReferential_ProducesOneSingletonModulePerGene()
    {
        // [0, 1, 2, 3] → each gene points to itself → 4 singleton modules
        var graph = BuildFourNodeGraph();
        var encoding = MakeEncoding(graph, [0, 1, 2, 3]);

        var modules = encoding.GetModules();

        modules.Should().HaveCount(4);
        foreach (var module in modules)
        {
            module.GetIndices().Should().HaveCount(1);
        }
    }

    [Fact]
    public void TwoChains_ProducesTwoModules()
    {
        // [1, 1, 3, 3] → module {0,1} and module {2,3}
        var graph = BuildFourNodeGraph();
        var encoding = MakeEncoding(graph, [1, 1, 3, 3]);

        var modules = encoding.GetModules();

        modules.Should().HaveCount(2);
        modules.Should().ContainSingle(m => m.GetIndices().SequenceEqual(new[] { 0, 1 }));
        modules.Should().ContainSingle(m => m.GetIndices().SequenceEqual(new[] { 2, 3 }));
    }

    [Fact]
    public void OneChainAndTwoSingletons_ProducesThreeModules()
    {
        // [1, 1, 2, 3] → module {0,1}, singleton {2}, singleton {3}
        var graph = BuildFourNodeGraph();
        var encoding = MakeEncoding(graph, [1, 1, 2, 3]);

        var modules = encoding.GetModules();

        modules.Should().HaveCount(3);
        modules.Should().ContainSingle(m => m.GetIndices().SequenceEqual(new[] { 0, 1 }));
        modules.Should().ContainSingle(m => m.GetIndices().SequenceEqual(new[] { 2 }));
        modules.Should().ContainSingle(m => m.GetIndices().SequenceEqual(new[] { 3 }));
    }

    // --- Clone ---

    [Fact]
    public void Clone_ProducesDeepCopy()
    {
        var graph = BuildFourNodeGraph();
        var original = MakeEncoding(graph, [1, 1, 3, 3]);

        var clone = original.Clone();

        clone.GetModules().Should().HaveCount(original.GetModules().Count);
        clone.Should().NotBeSameAs(original);
    }

    [Fact]
    public void Clone_ModifyingCloneDoesNotAffectOriginal()
    {
        var graph = BuildFourNodeGraph();
        var original = MakeEncoding(graph, [1, 1, 3, 3]);
        var clone = original.Clone();

        clone.ReplaceIntegerGene(0, new Gene(0));

        ((int)original.GetIntegerGene(0).Value).Should().Be(1);
    }

    // --- ReplaceIntegerGene ---

    [Fact]
    public void ReplaceIntegerGene_UpdatesGene()
    {
        var graph = BuildFourNodeGraph();
        var encoding = MakeEncoding(graph, [0, 1, 2, 3]);

        encoding.ReplaceIntegerGene(2, new Gene(99));

        ((int)encoding.GetIntegerGene(2).Value).Should().Be(99);
    }

    [Fact]
    public void ReplaceIntegerGene_InvalidIndex_Throws()
    {
        var graph = BuildFourNodeGraph();
        var encoding = MakeEncoding(graph, [0, 1, 2, 3]);

        var act = () => encoding.ReplaceIntegerGene(-1, new Gene(0));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // --- Graph reference ---

    [Fact]
    public void GetGraph_ReturnsSameGraphInstance()
    {
        var graph = BuildFourNodeGraph();
        var encoding = MakeEncoding(graph, [0, 1, 2, 3]);

        encoding.GetGraph().Should().BeSameAs(graph);
    }

    // --- NullGraph ---

    [Fact]
    public void Constructor_NullGraph_ThrowsArgumentNullException()
    {
        // ChromosomeBase requires >= 2 genes, so pass 2 to reach the null-graph check
        var act = () => new LinearLinkageEncoding(null!, new List<Gene> { new Gene(0), new Gene(1) });

        act.Should().Throw<ArgumentNullException>();
    }
}
