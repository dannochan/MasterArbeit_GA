using System;
using MA_GA.domain.geneticalgorithm.parameter;
using MA_GA.models.optimizationresult;
using MA_GA.Models;

namespace MA_GA.domain.geneticalgorithm.engine;

public interface GeneticAlgorithmEngine
{
    GeneticAlgorithmExecutionResult run(Graph graph, GeneticAlgorithmParameter geneticAlgorithmParameter, MutationWeight? mutationWeight);

}
