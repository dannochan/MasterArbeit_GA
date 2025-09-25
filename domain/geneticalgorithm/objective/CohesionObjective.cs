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
        return modules.Where(module => !ModuleInformationService.IsIsolated(module, graph)).Sum(module =>
        {
            var edges = ModuleInformationService.GetModuleEdges(module, graph);
            var edgeTypeCounts = CalculateEdgeTypeCounts(module, edges);
            var maxMPScontribution = (edgeTypeCounts["BPS"] * (edgeTypeCounts["BPS"] - 1.0)) / 2.0;
            var maxIOcontribution = (edgeTypeCounts["IO"] * (edgeTypeCounts["IO"] - 1.0)) / 2.0;
            var maxBPS_IOcontribution = edgeTypeCounts["BPS"] * edgeTypeCounts["IO"];
            var maxCohesionOfTheModule = maxIOcontribution * 20.0 + maxMPScontribution * 25.0 + maxBPS_IOcontribution * 20.0;

            double sum = 0.0;
            foreach (var edge in edges)
            {
                if (visitedEdges.Contains(edge))
                {
                    continue; // Skip already visited edges
                }
                visitedEdges.Add(edge);
                var source = edge.Source;
                var target = edge.Target;

                if (module.CheckIndexInModule(source.GetIndex()) && module.CheckIndexInModule(target.GetIndex()))
                {
                    sum += edge.Weight;

                }

            }

            double actualCohesion = sum / maxCohesionOfTheModule;
            double edgeCount = edgeTypeCounts["BPS"] + edgeTypeCounts["IO"];
            double weightedCohesion = actualCohesion * (edgeCount / (double)graph.GetGraph().VertexCount);
            return weightedCohesion * 100.0; // return as percentage
        });
    }

    private static Dictionary<string, double> CalculateEdgeTypeCounts(Module module, List<ObjectRelation> edges)
    {
        var edgeTypeCounts = new Dictionary<string, double>
            {
                { "BPS", 0.0 },
                { "IO", 0.0 },
                { "BPS_IO", 0.0 }
            };

        foreach (var edge in edges)
        {
            var sourceType = edge.Source.ObjectType;
            var targetType = edge.Target.ObjectType;
            var isInModule = module.CheckIndexInModule(edge.Source.GetIndex()) && module.CheckIndexInModule(edge.Target.GetIndex());
            if (!isInModule)
            {
                continue; // Skip edges where both nodes are not in the module
            }

            if (sourceType == ObjectType.FunctionObject && targetType == ObjectType.FunctionObject)
            {
                edgeTypeCounts["BPS"]++;
            }
            else if (sourceType == ObjectType.InformationObject && targetType == ObjectType.InformationObject)
            {
                edgeTypeCounts["IO"]++;
            }
            else if (
                (sourceType == ObjectType.FunctionObject && targetType == ObjectType.InformationObject) ||
                (sourceType == ObjectType.InformationObject && targetType == ObjectType.FunctionObject)
            )
            {
                edgeTypeCounts["BPS_IO"]++;
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
