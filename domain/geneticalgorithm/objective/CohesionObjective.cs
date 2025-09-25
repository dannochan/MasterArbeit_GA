using System;
using System.Reflection;
using GeneticSharp;
using MA_GA.domain.module;
using MA_GA.models.enums;
using MA_GA.Models;
using QuikGraph;
using Module = MA_GA.domain.module.Module;

namespace MA_GA.domain.geneticalgorithm.objective;

public class CohesionObjective : Objective
{
    public CohesionObjective(Graph graph, double weight)
    {
        SetGraph(graph ?? throw new ArgumentNullException(nameof(graph)));
        SetWeight(weight);

    }
    public override double CalculateValue(List<Module> modules)
    {
        var visitedEdges = new HashSet<IObjectRelation>();
        return modules
            .Where(module => !ModuleInformationService.IsIsolated(module, graph))
            .Sum(module =>
            {


                var edges = ModuleInformationService.GetModuleEdges(module, graph);
                var edgeTypeCounts = CalculateObjectTypeCounts(module);
                double bpsCount = edgeTypeCounts["BPS"];
                double ioCount = edgeTypeCounts["IO"];

                double maxMpsContribution = (bpsCount * (bpsCount - 1.0)) / 2.0;
                double maxIoContribution = (ioCount * (ioCount - 1.0)) / 2.0;
                double maxBpsIoContribution = bpsCount * ioCount;
                double maxCohesion = maxIoContribution * 20.0 + maxMpsContribution * 25.0 + maxBpsIoContribution * 20.0;

                double totalEdgeWeightOfTheModule = 0.0;
                foreach (var edge in edges)
                {
                    if (visitedEdges.Contains(edge))
                        continue;
                    visitedEdges.Add(edge);
                    bool sourceInModule = module.CheckIndexInModule(edge.Source.GetIndex());
                    bool targetInModule = module.CheckIndexInModule(edge.Target.GetIndex());

                    if (sourceInModule && targetInModule)
                    {

                        totalEdgeWeightOfTheModule += edge.Weight;
                    }
                }

                double actualCohesion = maxCohesion > 0.0 ? totalEdgeWeightOfTheModule / maxCohesion : 0.0;
                double objectTypeCount = bpsCount + ioCount;
                double vertexCount = (double)graph.GetGraph().VertexCount;
                double weightedCohesion = vertexCount > 0.0 ? actualCohesion * (objectTypeCount / vertexCount) : 0.0;
                return weightedCohesion * 100; // return as percentage
            });
    }

    private Dictionary<string, double> CalculateObjectTypeCounts(Module module)
    {
        var edgeTypeCounts = new Dictionary<string, double>
            {
                { "BPS", 0.0 },
                { "IO", 0.0 }
            };

        var vertexIndicesInModule = new HashSet<int>(module.GetIndices().ToList());
        foreach (var vertexIndex in vertexIndicesInModule)
        {
            var vertex = graph.GetGraph().Vertices.FirstOrDefault(v => v.GetIndex() == vertexIndex);
            if (vertex != null)
            {
                var objectType = vertex.ObjectType;
                if (objectType == ObjectType.FunctionObject)
                {
                    edgeTypeCounts["BPS"] += 1.0;
                }
                else if (objectType == ObjectType.InformationObject)
                {
                    edgeTypeCounts["IO"] += 1.0;
                }
            }
        }


        return edgeTypeCounts;
    }

    public override string GetObjectiveName()
    {
        return GetObjectiveType().ToString();
    }

    public override ObjectiveType GetObjectiveType()
    {
        return ObjectiveType.MAXIMISE_COHESION;
    }

    public override OptimizationType GetOptimizationType()
    {
        return OptimizationType.Maximum;
    }

}
