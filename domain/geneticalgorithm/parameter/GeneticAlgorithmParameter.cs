using System;
using MA_GA.domain.geneticalgorithm.objective;

namespace MA_GA.domain.geneticalgorithm.parameter;

public class GeneticAlgorithmParameter
{
    private string _chromosomeEncoding;
    public string ChromosomeEncoding { get { return _chromosomeEncoding; } }
    public string OffspringSelection { get; set; }
    public string SurvivalSelection { get; set; }
    public string CrossoverType { get; set; }
    public string MutationType { get; set; }
    public int PopulationSize { get; set; }
    public float CrossoverRate { get; set; }
    public float MutationRate { get; set; }
    public int MaxGenerations { get; set; }
    public int TournamentSize { get; set; }
    public float ElitismCount { get; set; }

    public double ConvergedGeneRate { get; set; }

    public double ConvergenceRate { get; set; }
    public int CountGeneration { get; set; }
    public int MinimumParetoSetSize { get; set; }
    public int MaximumParetoSetSize { get; set; }

    public bool UseWeightedSumMethod { get; set; }
    public bool UseGreedyPartition { get; set; }


    public GeneticAlgorithmParameter(
        string chromosomeEncoding,
        string offspringSelection,
        string survivalSelection,
        string crossoverType,
        string mutationType,
        int populationSize,
        float crossoverRate,
        float mutationRate,
        int maxGenerations,
        int tournamentSize,
        float elitismCount,
        double convergedGeneRate,
        double convergenceRate,
        int countGeneration,
        int minimumParetoSetSize,
        int maximumParetoSetSize,
        bool useWeightedSumMethod = false,
        bool useGreedyPartition = true)
    {
        _chromosomeEncoding = chromosomeEncoding;
        OffspringSelection = offspringSelection;
        SurvivalSelection = survivalSelection;
        CrossoverType = crossoverType;
        MutationType = mutationType;
        PopulationSize = populationSize;
        CrossoverRate = crossoverRate;
        MutationRate = mutationRate;
        MaxGenerations = maxGenerations;
        TournamentSize = tournamentSize;
        ElitismCount = elitismCount;
        ConvergedGeneRate = convergedGeneRate;
        ConvergenceRate = convergenceRate;
        CountGeneration = countGeneration;
        MinimumParetoSetSize = minimumParetoSetSize;
        MaximumParetoSetSize = maximumParetoSetSize;
        UseWeightedSumMethod = useWeightedSumMethod; // Default value, can be set to true if needed
        UseGreedyPartition = useGreedyPartition; // Default value, can be set to false if needed
    }

    public override string ToString()
    {
        return $"GeneticAlgorithmParameter: " +
               $"ChromosomeEncoding={ChromosomeEncoding}, " +
               $"OffspringSelection={OffspringSelection}, " +
               $"SurvivalSelection={SurvivalSelection}, " +
               $"CrossoverType={CrossoverType}, " +
               $"MutationType={MutationType}, " +
               $"PopulationSize={PopulationSize}, " +
               $"CrossoverRate={CrossoverRate}, " +
               $"MutationRate={MutationRate}, " +
               $"MaxGenerations={MaxGenerations}, " +
               $"TournamentSize={TournamentSize}, " +
               $"ElitismCount={ElitismCount}, " +
               $"ConvergedGeneRate={ConvergedGeneRate}, " +
               $"ConvergenceRate={ConvergenceRate}, " +
               $"CountGeneration={CountGeneration}, " +
               $"MinimumParetoSetSize={MinimumParetoSetSize}, " +
               $"MaximumParetoSetSize={MaximumParetoSetSize}" +
                $", UseWeightedSumMethod={UseWeightedSumMethod}" +
                $", UseGreedyPartition={UseGreedyPartition}";
    }




}
