using FluentAssertions;
using MA_GA.domain.geneticalgorithm.parameter;
using MA_GA.Models;
using Xunit;

namespace MA_GA.Tests;

public class GraphTests
{
    private static DataObjectRelationWeight DefaultWeight() => new DataObjectRelationWeight();

    private static DataObject MakeFunc(string name, string shortName, int index) =>
        new DataObject(name, ObjectType.FunctionObject, shortName, false, index);

    private static DataObject MakeInfo(string name, string shortName, int index) =>
        new DataObject(name, ObjectType.InformationObject, shortName, false, index);

    private static ObjectRelation MakeRelation(int edgeNumber, DataObject source, DataObject target) =>
        new ObjectRelation(edgeNumber, RelationType.Read, source, target, 1.0);

    [Fact]
    public void NewGraph_IsEmpty()
    {
        var graph = new Graph(DefaultWeight());
        graph.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void AddNodeToGraph_SingleNode_IsNotEmpty()
    {
        var graph = new Graph(DefaultWeight());
        var node = MakeFunc("FuncA", "FA", 0);

        graph.AddNodeToGraph(node);

        graph.IsEmpty().Should().BeFalse();
    }

    [Fact]
    public void AddNodeToGraph_AddsVertexToInternalGraph()
    {
        var graph = new Graph(DefaultWeight());
        var node = MakeFunc("FuncA", "FA", 0);

        graph.AddNodeToGraph(node);

        graph.GetGraph().VertexCount.Should().Be(1);
    }

    [Fact]
    public void AddRelationToGraph_AddsEdgeToInternalGraph()
    {
        var graph = new Graph(DefaultWeight());
        var n0 = MakeFunc("FuncA", "FA", 0);
        var n1 = MakeInfo("InfoB", "IB", 1);
        graph.AddNodeToGraph(n0);
        graph.AddNodeToGraph(n1);

        graph.AddRelationToGraph(MakeRelation(0, n0, n1));

        graph.GetGraph().EdgeCount.Should().Be(1);
    }

    [Fact]
    public void AddRelationToGraph_NullRelation_ThrowsArgumentNullException()
    {
        var graph = new Graph(DefaultWeight());

        var act = () => graph.AddRelationToGraph(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetNodeNameById_ReturnsCorrectName()
    {
        var graph = new Graph(DefaultWeight());
        var node = MakeFunc("FunctionAlpha", "FA", 42);
        graph.AddNodeToGraph(node);

        var name = graph.GetNodeNameById(42);

        name.Should().Be("FunctionAlpha");
    }

    [Fact]
    public void GetNodeNameById_UnknownId_ThrowsKeyNotFoundException()
    {
        var graph = new Graph(DefaultWeight());

        var act = () => graph.GetNodeNameById(99);

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void GetNodeObjectByName_ReturnsCorrectNode()
    {
        var graph = new Graph(DefaultWeight());
        var node = MakeFunc("FuncSearch", "FS", 0);
        graph.AddNodeToGraph(node);

        // GetNodeObjectByName matches against ShortName (case-insensitive)
        var result = graph.GetNodeObjectByName("fs");

        result.Should().NotBeNull();
        result!.ShortName.Should().Be("FS");
    }

    [Fact]
    public void GetNodeObjectByName_EmptyName_ThrowsArgumentNullException()
    {
        var graph = new Graph(DefaultWeight());

        var act = () => graph.GetNodeObjectByName(string.Empty);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetModularisableElements_ReturnsAllVertices()
    {
        var graph = new Graph(DefaultWeight());
        graph.AddNodeToGraph(MakeFunc("F0", "f0", 0));
        graph.AddNodeToGraph(MakeFunc("F1", "f1", 1));
        graph.AddNodeToGraph(MakeInfo("I2", "i2", 2));

        var elements = graph.GetModularisableElements();

        elements.Should().HaveCount(3);
    }

    [Fact]
    public void GetModularisableElements_EmptyGraph_ThrowsInvalidOperationException()
    {
        var graph = new Graph(DefaultWeight());

        var act = () => graph.GetModularisableElements();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetModularisableElementByIndex_ReturnsCorrectElement()
    {
        var graph = new Graph(DefaultWeight());
        var node = MakeFunc("FuncX", "FX", 7);
        graph.AddNodeToGraph(node);

        var element = graph.GetModularisableElementByIndex(7);

        element.GetIndex().Should().Be(7);
    }

    [Fact]
    public void GetModularisableElementByIndex_UnknownIndex_ThrowsKeyNotFoundException()
    {
        var graph = new Graph(DefaultWeight());

        var act = () => graph.GetModularisableElementByIndex(99);

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void IsIsolatedVertex_VertexWithNoOutgoingEdges_ReturnsTrue()
    {
        var graph = new Graph(DefaultWeight());
        var n0 = MakeFunc("F0", "f0", 0);
        var n1 = MakeFunc("F1", "f1", 1);
        graph.AddNodeToGraph(n0);
        graph.AddNodeToGraph(n1);
        graph.AddRelationToGraph(MakeRelation(0, n0, n1)); // n1 has no outgoing edges

        graph.IsIsolatedVertex(n1).Should().BeTrue();
        graph.IsIsolatedVertex(n0).Should().BeFalse();
    }

    [Fact]
    public void FindIncidentRelationsForVertex_ReturnsEdgesConnectedToVertex()
    {
        var graph = new Graph(DefaultWeight());
        var n0 = MakeFunc("F0", "f0", 0);
        var n1 = MakeFunc("F1", "f1", 1);
        var n2 = MakeInfo("I2", "i2", 2);
        graph.AddNodeToGraph(n0);
        graph.AddNodeToGraph(n1);
        graph.AddNodeToGraph(n2);
        graph.AddRelationToGraph(MakeRelation(0, n0, n2));
        graph.AddRelationToGraph(MakeRelation(1, n1, n2));

        var incidentToN2 = graph.FindIncidentRelationsForVertex(n2);
        var incidentToN0 = graph.FindIncidentRelationsForVertex(n0);

        incidentToN2.Should().HaveCount(2);
        incidentToN0.Should().HaveCount(1);
    }

    [Fact]
    public void AddRelationToGraph_UpdatesVertexWeights()
    {
        var graph = new Graph(DefaultWeight());
        var n0 = MakeFunc("F0", "f0", 0);
        var n1 = MakeInfo("I1", "i1", 1);
        graph.AddNodeToGraph(n0);
        graph.AddNodeToGraph(n1);

        graph.AddRelationToGraph(new ObjectRelation(0, RelationType.Create, n0, n1, 1.0));

        n0.Weight.Should().BeGreaterThan(0.0);
        n1.Weight.Should().BeGreaterThan(0.0);
    }
}
