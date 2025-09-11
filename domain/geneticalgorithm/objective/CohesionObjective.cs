using System;
using System.Reflection;
using GeneticSharp;
using MA_GA.domain.module;
using MA_GA.models.enums;
using MA_GA.Models;
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
                else
                {
                    sum += edge.Weight / 2.0;
                }

            }
            return sum;
        });
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
