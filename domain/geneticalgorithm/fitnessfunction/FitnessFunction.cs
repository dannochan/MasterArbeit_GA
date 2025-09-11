using System;
using GeneticSharp;
using MA_GA.domain.geneticalgorithm.objective;
using MA_GA.models.enums;
using MA_GA.Models;

namespace MA_GA.domain.geneticalgorithm.fitnessfunction;

public class FitnessFunction : IFitness
{
    private readonly List<Objective> _objectives;

    private readonly MultiObjectivesEvaluator _multiObjectivesEvaluator;
    private readonly Graph _graph;

    private readonly double _sumObjectiveWeights;

    public FitnessFunction(List<Objective> objectives, Graph graph)
    {
        _objectives = objectives ?? throw new ArgumentNullException(nameof(objectives));
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        _multiObjectivesEvaluator = new MultiObjectivesEvaluator(_graph, _objectives);
        _sumObjectiveWeights = objectives.Sum(o => o.GetWeight());
    }

    /// <summary>
    /// Scalarization Fitness function Evaluates the fitness of a chromosome based on the objectives defined.
    /// </summary>
    /// <param name="chromosome"></param>
    /// <returns></returns>
    public double Evaluate(IChromosome chromosome)
    {
        /*
 return _objectives.Select(obj =>
 {


     var weight = obj.GetWeight() / _sumObjectiveWeights;
     var objectiveValue = obj.Evaluate(chromosome);

     var weightedValue = weight * objectiveValue;
     // TODO: how to deal with type of objectives, see paper of ali p49

     if (obj.GetOptimizationType() == OptimizationType.Minimum)
     {
         return weightedValue *= -1;
     }
     ;

     return weightedValue;

 }).Sum();  */

        return EvaluateAll(chromosome).Sum();
    }

    public double[] EvaluateAll(IChromosome chromosome)
    {
        return _multiObjectivesEvaluator.EvaluateAll(chromosome);

    }
}
