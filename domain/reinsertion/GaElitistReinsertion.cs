using System;
using GeneticSharp;

namespace MA_GA.domain.reinsertion;


public class GaElitistReinsertion : ReinsertionBase
{
    private float SelectionRate { get; set; }

    public GaElitistReinsertion(float selectionRate) : base(false, true)
    {

        SelectionRate = selectionRate ;
    }

    protected override IList<IChromosome> PerformSelectChromosomes(IPopulation population, IList<IChromosome> offspring, IList<IChromosome> parents)
    {

        var diff = (int)(population.MaxSize * SelectionRate);
        Console.WriteLine($"Performing elitist reinsertion with selection rate: {SelectionRate}, diff: {diff}");
        if (diff > 0)
        {
            var bestParents = parents.OrderByDescending(p => p.Fitness).Take(diff).ToList();

            for (int i = 0; i < bestParents.Count; i++)
            {
                offspring.Add(bestParents[i]);
            }
        }

        return offspring;
    }
}
