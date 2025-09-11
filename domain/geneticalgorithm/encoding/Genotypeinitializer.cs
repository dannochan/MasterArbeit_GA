using System;
using GeneticSharp;
using MA_GA.domain.geneticalgorithm.encoding;
using MA_GA.Models;

namespace MA_GA.domain.geneticalgorithm.encoding;

public class Genotypeinitializer
{

    public static IChromosome GenerateGenotypeWithModulesForEachConnectedComponet(Graph graph)
    {
        var linearLinkageEncoding = LinearLinkageEncodingInitialiser.InitializeLinearLinkageEncodingWithModulesForEachConnectedCompponent(graph);

        return new LinearLinkageEncoding(
            graph,
            linearLinkageEncoding.GetIntegerGenes()
        );
    }

}
