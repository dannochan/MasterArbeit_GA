using System;
using MA_GA.domain.module;
using MA_GA.models.enums;
using MA_GA.Models;
using QuikGraph;

namespace MA_GA.domain.geneticalgorithm.objective;

public class ModularityObjective : Objective
{
    public UndirectedModularityMeasurer<DataObject>? _modularityMeasurer { get; private set; }
    public ModularityObjective(Graph graph, double weight)
    {
        SetGraph(graph ?? throw new ArgumentNullException(nameof(graph)));
        SetWeight(weight);
        SetModularityMeasurer();
    }

    private void SetModularityMeasurer()
    {
        var edges = this.graph.GetGraph().Edges;
        var vertex = this.graph.GetGraph().Vertices;


        var newUndirectedGraph = new UndirectedGraph<DataObject, IEdge<DataObject>>();
        foreach (var v in vertex)
        {
            newUndirectedGraph.AddVertex(v);
        }
        foreach (var e in edges)
        {
            newUndirectedGraph.AddEdge(e);
        }


        _modularityMeasurer = new UndirectedModularityMeasurer<DataObject>(newUndirectedGraph);
    }

    public override double CalculateValue(List<Module> modules)
    {

        var partitions = modules.Select(m => ModuleInformationService.GetVerticesOfModule(m, this.graph)).ToList();
        return _modularityMeasurer.Modularity(partitions);

    }
    public override string GetObjectiveName()
    {
        return GetObjectiveType().ToString();
    }

    public override ObjectiveType GetObjectiveType()
    {
        return ObjectiveType.MAX_MODULARITY;
    }

    public override OptimizationType GetOptimizationType()
    {
        return OptimizationType.Maximum;
    }


}
