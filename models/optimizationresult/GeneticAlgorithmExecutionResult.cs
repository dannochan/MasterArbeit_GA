using System;
using MA_GA.domain.geneticalgorithm.parameter;

namespace MA_GA.models.optimizationresult;

public class GeneticAlgorithmExecutionResult
{

    //    private HashSet<ParetoOptimalSolution> ParetoOptimalSolutions { get; set; }

    public GeneticAlgorithmParameter GeneticAlgorithmParameter { get; set; }
    public GeneticAlgorithmResults GeneticAlgorithmResults { get; set; }

    public string GenerationResultString { get; set; }

    public GeneticAlgorithmExecutionResult()
    {
        GeneticAlgorithmParameter = new GeneticAlgorithmParameter();
        GeneticAlgorithmResults = new GeneticAlgorithmResults();
        GenerationResultString = string.Empty;
    }


}
