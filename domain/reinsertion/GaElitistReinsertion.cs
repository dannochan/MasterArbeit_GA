using System;
using GeneticSharp;
using MA_GA.domain.geneticalgorithm.encoding;

namespace MA_GA.domain.reinsertion;


public class GaElitistReinsertion : ReinsertionBase
{
    private float SelectionRate { get; set; }

    public GaElitistReinsertion(float selectionRate) : base(false, true)
    {

        SelectionRate = selectionRate;
    }

    protected override IList<IChromosome> PerformSelectChromosomes(IPopulation population, IList<IChromosome> offspring, IList<IChromosome> parents)
    {

        var diff = (int)(population.MaxSize * SelectionRate);

        if (diff > 0)
        {
            var bestParents = parents
                .OrderByDescending(p => p.Fitness)
                .Take(diff)
                .ToList();

            foreach (var p in bestParents)
                offspring.Add(p);
        }
        int needed = population.MaxSize - offspring.Count;

        // If you need more chromosomes, fill from parents
        if (needed > 0)
        {
            var fill = parents
                .OrderByDescending(p => p.Fitness)
                .Take(needed)
                .ToList();

            offspring = offspring.Concat(fill).ToList();
        }


        return offspring;  // return AFTER finishing the loop
    }
}
