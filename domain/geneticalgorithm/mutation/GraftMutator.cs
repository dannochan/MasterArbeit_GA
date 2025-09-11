using System;
using GeneticSharp;
using MA_GA.domain.geneticalgorithm.encoding;
using MA_GA.domain.geneticalgorithm.parameter;
using MA_GA.Models;

namespace MA_GA.domain.geneticalgorithm.mutation;

public class GraftMutator : MutationBase
{

    private readonly Graph baseGraph;
    private float divideModuleProbability;
    private float combineModuleProbability;
    private float movegeneToDifferentModuleProbability;



    public GraftMutator(MutationWeight mutationWeight, Graph graph)
    {

        baseGraph = graph;
        DetermineMutationWeights(mutationWeight);

    }

    private void DetermineMutationWeights(MutationWeight mutationWeight)
    {
        float sumPossibilities = mutationWeight.SplitModulesWeight + mutationWeight.CombineModulesWeight + mutationWeight.MoveGeneToDifferentModuleWeight;
        divideModuleProbability = mutationWeight.SplitModulesWeight / sumPossibilities;
        combineModuleProbability = mutationWeight.CombineModulesWeight / sumPossibilities;
        movegeneToDifferentModuleProbability = mutationWeight.MoveGeneToDifferentModuleWeight / sumPossibilities;
    }

    protected override void PerformMutate(IChromosome chromosome, float probability)
    {

        if (chromosome is not LinearLinkageEncoding lle)
            throw new InvalidOperationException("Chromosome must be of type YourEncodingChromosome.");


        var encoding = new LinearLinkageEncoding(lle, baseGraph);

        var rnd = RandomizationProvider.Current;

        // Only mutate if probability allows
        if (rnd.GetFloat() < probability)
        {
            var opRoll = rnd.GetFloat();

            if (opRoll < divideModuleProbability)
            {
                encoding = LinearLinkageEncodingOperator.DivideRandomModule(encoding);
            }
            else if (opRoll < divideModuleProbability + combineModuleProbability)
            {
                if (LinearLinkageEncodingInformationService.GetNumberOfNonIsolatedModules(encoding) > 2)
                    encoding = LinearLinkageEncodingOperator.CombineRandomGroup(encoding);
            }
            else
            {
                encoding = LinearLinkageEncodingOperator.MoveRandomGeneToIncidentModule(encoding);
            }
        }

        // Single repair step after mutation
        if (!encoding.IsValid())
            encoding = LinearLinkageEncodingOperator.FixLinearLinkageEncoding(encoding);

        // Update the original chromosome in place
        lle.CopyFrom(encoding);

    }
}
