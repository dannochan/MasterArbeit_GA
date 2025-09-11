using System;
using MA_GA.domain.module;
using MA_GA.models.enums;
using MA_GA.Models;
using QuikGraph;

namespace MA_GA.domain.geneticalgorithm.objective;

public class CouplingObjective : Objective
{
    public CouplingObjective(Graph graph, double weight)
    {
        SetGraph(graph ?? throw new ArgumentNullException(nameof(graph)));
        SetWeight(weight);
    }
    public override double CalculateValue(List<Module> modules)
    {
        var visitedEdges = new HashSet<IObjectRelation>();
        double totalCoupling = 0.0;

        foreach (var module in modules)
        {
            if (ModuleInformationService.IsIsolated(module, graph))
                continue;

            foreach (var edge in ModuleInformationService.GetBoundaryEdgesOfModule(module, graph))
            {
                if (visitedEdges.Add(edge))
                {
                    totalCoupling += edge.Weight;
                }
            }
        }

        return totalCoupling;
    }
    public override string GetObjectiveName()
    {
        return GetObjectiveType().ToString();
    }

    public override ObjectiveType GetObjectiveType()
    {
        return ObjectiveType.MINIMISE_COUPLING;
    }

    public override OptimizationType GetOptimizationType()
    {
        return OptimizationType.Minimum;
    }


}
