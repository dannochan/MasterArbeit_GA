using System;
using GeneticSharp;
using MA_GA.domain.geneticalgorithm.encoding;

namespace MA_GA.domain.geneticalgorithm.selection;

public class GaTournamentSelection : SelectionBase
{

    public int Size { get; set; }
    public bool AllowWinnerCompeteNextTournament { get; set; }



    public GaTournamentSelection(int size, bool allowWinnerCompeteNextTournament = true) : base(size)
    {
        Size = size;
        AllowWinnerCompeteNextTournament = allowWinnerCompeteNextTournament;
    }
    public GaTournamentSelection(int size) : this(size, allowWinnerCompeteNextTournament: true)
    {

    }

    protected override IList<IChromosome> PerformSelectChromosomes(int number, Generation generation)
    {
        if (Size > generation.Chromosomes.Count)
        {
            throw new SelectionException(this, "The tournament size is greater than available chromosomes. Tournament size is {0} and generation {1} available chromosomes are {2}.".With(Size, generation.Number, generation.Chromosomes.Count));
        }

        var list = generation.Chromosomes.OfType<LinearLinkageEncoding>().ToList();
        var list2 = new List<LinearLinkageEncoding>();
        // TODO: Check if criteria is valid 
        while (list2.Count < number)
        {
            int[] randomIndexes = RandomizationProvider.Current.GetUniqueInts(Size, 0, list.Count);
            LinearLinkageEncoding chromosome = list
                .Where((c, i) => randomIndexes.Contains(i))
                .OrderByDescending(c => c.Fitness)
                .First();

            list2.Add(chromosome.Clone());
            if (!AllowWinnerCompeteNextTournament)
            {
                list.Remove(chromosome);
            }
        }

        return list2.Cast<IChromosome>().ToList();
    }
}
