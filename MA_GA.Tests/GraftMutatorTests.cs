using FluentAssertions;
using GeneticSharp;
using MA_GA.domain.geneticalgorithm.encoding;
using MA_GA.domain.geneticalgorithm.mutation;
using MA_GA.domain.geneticalgorithm.parameter;
using MA_GA.Models;
using Xunit;

namespace MA_GA.Tests;

public class GraftMutatorTests
{
    // Minimal non-LLE chromosome for the type-guard test
    private class FakeChromosome : ChromosomeBase
    {
        public FakeChromosome() : base(2) { }
        public override IChromosome CreateNew() => new FakeChromosome();
        public override Gene GenerateGene(int geneIndex) => new Gene(0);
    }

    // 4-node graph: 0→1, 2→3  (two connected pairs)
    private static (Graph graph, DataObject n0, DataObject n1, DataObject n2, DataObject n3) BuildFourNodeGraph()
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
        return (graph, n0, n1, n2, n3);
    }

    // 6-node chain: 0→1→2→3→4→5
    // Encoding [1,1,3,3,5,5] gives 3 non-isolated modules {0,1},{2,3},{4,5}
    // whose cross-edges (1→2, 3→4) make them graph-neighbors → combine can merge them.
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

    private static LinearLinkageEncoding MakeEncoding(Graph graph, int[] geneValues) =>
        new LinearLinkageEncoding(graph, geneValues.Select(v => new Gene(v)).ToList());

    // --- Type guard ---

    [Fact]
    public void Mutate_NonLleChromosome_ThrowsInvalidOperationException()
    {
        var (graph, _, _, _, _) = BuildFourNodeGraph();
        var mutator = new GraftMutator(new MutationWeight(), graph);

        var act = () => mutator.Mutate(new FakeChromosome(), 1.0f);

        act.Should().Throw<InvalidOperationException>();
    }

    // --- Probability gate ---

    [Fact]
    public void Mutate_ProbabilityZero_GenesUnchanged()
    {
        // probability=0 → rnd.GetFloat() < 0 is always false → no mutation applied
        var (graph, _, _, _, _) = BuildFourNodeGraph();
        var mutator = new GraftMutator(new MutationWeight(), graph);
        var lle = MakeEncoding(graph, [1, 1, 3, 3]);

        mutator.Mutate(lle, 0.0f);

        ((int)lle.GetIntegerGene(0).Value).Should().Be(1);
        ((int)lle.GetIntegerGene(1).Value).Should().Be(1);
        ((int)lle.GetIntegerGene(2).Value).Should().Be(3);
        ((int)lle.GetIntegerGene(3).Value).Should().Be(3);
    }

    // --- Split operation ---

    [Fact]
    public void Mutate_SplitOnly_ProbabilityOne_ResultIsValid()
    {
        // weights=(1,0,0) → divideModuleProbability=1.0 → always split
        var (graph, _, _, _, _) = BuildFourNodeGraph();
        var mutator = new GraftMutator(new MutationWeight(1f, 0f, 0f), graph);
        var lle = MakeEncoding(graph, [1, 1, 3, 3]);

        mutator.Mutate(lle, 1.0f);

        lle.IsValid().Should().BeTrue();
    }

    // --- Combine operation ---

    [Fact]
    public void Mutate_CombineOnly_TwoModules_CombineSkipped_GenesUnchanged()
    {
        // weights=(0,1,0) → always combine branch; but guard requires >2 non-isolated modules
        // [1,1,3,3] has exactly 2 non-isolated modules → guard fires → no-op
        var (graph, _, _, _, _) = BuildFourNodeGraph();
        var mutator = new GraftMutator(new MutationWeight(0f, 1f, 0f), graph);
        var lle = MakeEncoding(graph, [1, 1, 3, 3]);

        mutator.Mutate(lle, 1.0f);

        ((int)lle.GetIntegerGene(0).Value).Should().Be(1);
        ((int)lle.GetIntegerGene(1).Value).Should().Be(1);
        ((int)lle.GetIntegerGene(2).Value).Should().Be(3);
        ((int)lle.GetIntegerGene(3).Value).Should().Be(3);
    }

    [Fact]
    public void Mutate_CombineOnly_ThreeModules_ModuleCountDecreases()
    {
        // 6-node graph, [1,1,3,3,5,5] → 3 non-isolated modules {0,1},{2,3},{4,5}
        // weights=(0,1,0) → combine branch, guard passes (3 > 2) → two modules merged → 2 modules
        var graph = BuildSixNodeChainGraph();
        var mutator = new GraftMutator(new MutationWeight(0f, 1f, 0f), graph);
        var lle = MakeEncoding(graph, [1, 1, 3, 3, 5, 5]);

        mutator.Mutate(lle, 1.0f);

        lle.GetModules().Should().HaveCount(2);
        lle.IsValid().Should().BeTrue();
    }

    // --- Move operation ---

    [Fact]
    public void Mutate_MoveOnly_ProbabilityOne_ResultIsValid()
    {
        // weights=(0,0,1) → always falls to else branch → MoveRandomGeneToIncidentModule
        var (graph, _, _, _, _) = BuildFourNodeGraph();
        var mutator = new GraftMutator(new MutationWeight(0f, 0f, 1f), graph);
        var lle = MakeEncoding(graph, [1, 1, 3, 3]);

        mutator.Mutate(lle, 1.0f);

        lle.IsValid().Should().BeTrue();
    }
}
