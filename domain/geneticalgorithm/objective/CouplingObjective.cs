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
        var moduleList = modules.Where(module => !ModuleInformationService.IsIsolated(module, graph)).ToList();
        var visitedEdges = new HashSet<IObjectRelation>();
        double totalCoupling = 0.0;
        double sumOfModuleEdgesWeights = 0.0;

        foreach (var module in moduleList)
        {
            if (ModuleInformationService.IsIsolated(module, graph))
                continue;

            var edgesOfModule = ModuleInformationService.GetModuleEdges(module, graph).Where(edge => module.CheckIndexInModule(edge.Source.GetIndex()) && module.CheckIndexInModule(edge.Target.GetIndex())).ToList();
            sumOfModuleEdgesWeights += edgesOfModule.Sum(edge => edge.Weight);

            var boundaryEdgesOfModuleWithoutBothIO = ModuleInformationService.GetBoundaryEdgesOfModule(module, graph).ToList();

            foreach (var edge in boundaryEdgesOfModuleWithoutBothIO)
            {
                if (edge != null && !visitedEdges.Contains(edge))
                {
                    visitedEdges.Add(edge);
                    totalCoupling += edge.Weight;
                }
            }
        }
        double allEdgesWeights = totalCoupling + sumOfModuleEdgesWeights;

        return (totalCoupling / allEdgesWeights) * 100.0; // return as percentage
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
