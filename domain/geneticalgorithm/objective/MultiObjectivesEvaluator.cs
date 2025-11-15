using System;
using GeneticSharp;
using MA_GA.domain.geneticalgorithm.encoding;
using MA_GA.models.enums;
using MA_GA.Models;

namespace MA_GA.domain.geneticalgorithm.objective;

public class MultiObjectivesEvaluator
{
    private readonly List<Objective> _objectives;
    private readonly Graph _graph;


    public MultiObjectivesEvaluator(Graph graph, List<Objective> objectives) : this(objectives)
    {
        this._graph = graph ?? throw new ArgumentNullException(nameof(graph));
    }

    public MultiObjectivesEvaluator(List<Objective> objectives)
    {
        _objectives = objectives;
    }

    // TODO: CHECK IF OBJECTIVES WEIGHT should be considreed in the evaluation
   public double[] EvaluateAll(IChromosome chromosome)
{
    // Create local copy / representation once
    var lle = new LinearLinkageEncoding(chromosome, _graph); // cheap constructor? profile it
    if (LinearLinkageEncodingInformationService.IsOneModuleConsistOfOneEdge(lle) ||
        LinearLinkageEncodingInformationService.IsModuleWithOnlyInformationObjects(lle))
        {
            // Prefer penalize 
            return [0.0d, 0.0d];
       
    }

    var modules = lle.GetModules().ToList(); // compute once

    // maybe build index -> vertex map and pass along via graph or parameter

    var sumWeights = _objectives.Sum(o => o.GetWeight());
    var results = new double[_objectives.Count];

    for (int i = 0; i < _objectives.Count; i++)
    {
        var obj = _objectives[i];
        var value = obj.CalculateValue(modules);
        var weight = obj.GetWeight() / sumWeights;
        var weighted = weight * value;
        if (obj.GetOptimizationType() == OptimizationType.Minimum)
        {
            weighted = -weighted;
        }
        // other penalties (monolith, only-IO) apply here based on lle
        results[i] = weighted;
    }

    return results;
}



    public int ObjectiveCount => _objectives.Count;

}
