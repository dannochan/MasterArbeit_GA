using System;
using GeneticSharp;
using MA_GA.domain.geneticalgorithm.encoding;
using MA_GA.domain.geneticalgorithm.fitnessfunction;
using MA_GA.domain.geneticalgorithm.objective;
using MA_GA.domain.geneticalgorithm.parameter;
using MA_GA.models.optimizationresult;
using MA_GA.Models;

namespace MA_GA.domain.geneticalgorithm.engine;

public class MainGeneticAlgorithmEngine : GeneticAlgorithmEngine
{

    private static readonly float RANDOM_GENERTATED_SEED = 12345f;


    public GeneticAlgorithmExecutionResult run(Graph graph, GeneticAlgorithmParameter geneticAlgorithmParameter, MutationWeight? mutationWeight = null)
    {
        if (mutationWeight == null)
        {
            mutationWeight = new MutationWeight();
        }

        BasicRandomization.ResetSeed((int)RANDOM_GENERTATED_SEED);
        RandomizationProvider.Current = new BasicRandomization();
        if (geneticAlgorithmParameter.UseWeightedSumMethod)
        {
            return ModularisewithWeightedSumFitnessFunction(geneticAlgorithmParameter, graph, mutationWeight);
        }

        return ModularisewithMultiObjectiveFitnessFunction(geneticAlgorithmParameter, graph, mutationWeight);


    }

    private GeneticAlgorithmExecutionResult ModularisewithMultiObjectiveFitnessFunction(GeneticAlgorithmParameter geneticAlgorithmParameter, Graph graph, MutationWeight mutationWeight)
    {

        // TODO: ADD objective when available

        var fitnessFunction = new MultiObjectiveFitnessFunction();

        // build genetic algorithm engine
        var geneticAlgorithmEngine = new GeneticAlgorithmEngineBuilder.Builder()
            .Graph(graph)
            .GeneticAlgorithmParameter(geneticAlgorithmParameter)
            .Fitness(new MultiObjectiveFitnessFunction())
            .MutationWeight(mutationWeight)
            .CreatingEngineForMultiObjectiveProblem();


        // run the genetic algorithm
        geneticAlgorithmEngine.Start();
        Console.WriteLine($"Population Size: {geneticAlgorithmEngine.Population.GenerationsNumber}");
        // print the best chromosome
        Console.WriteLine($"Best Fitness: {geneticAlgorithmEngine.BestChromosome.Fitness.Value}");
        // print modules of the best chromosome

        var chromosome = geneticAlgorithmEngine.BestChromosome.ToString();
        Console.WriteLine($"Best Chromosome: {chromosome}");

        // run the genetic algorithm


        return new GeneticAlgorithmExecutionResult();
    }

    private GeneticAlgorithmExecutionResult ModularisewithWeightedSumFitnessFunction(GeneticAlgorithmParameter geneticAlgorithmParameter, Graph graph, MutationWeight? mutationWeight)
    {

        // TODO: move to genetic parameter settings
        var objectives = new List<Objective>
        {
                 new CohesionObjective(graph, 1),
                  new CouplingObjective(graph, 1),
                  new ModularityObjective(graph, 1),
        };

        var fitnessFunction = new FitnessFunction(objectives, graph);

        // build genetic algorithm engine
        var geneticAlgorithmEngine = new GeneticAlgorithmEngineBuilder.Builder()
            .Graph(graph)
            .GeneticAlgorithmParameter(geneticAlgorithmParameter)
            .MutationWeight(mutationWeight)
            .Fitness(fitnessFunction)
            .CreatingEngineForWeightedSumProblem();

        var taskExecutor = new ParallelTaskExecutor();
        taskExecutor.MinThreads = 1;
        taskExecutor.MaxThreads = 20;
        geneticAlgorithmEngine.TaskExecutor = taskExecutor;


        // run the genetic algorithm
        geneticAlgorithmEngine.Start();
        Console.WriteLine($"GenerationNumber: {geneticAlgorithmEngine.Population.GenerationsNumber}");
        // print the best chromosome
        Console.WriteLine($"Best Fitness: {geneticAlgorithmEngine.BestChromosome.Fitness.Value}");
        // print modules of the best chromosome
        var BestChromosome = new LinearLinkageEncoding(geneticAlgorithmEngine.BestChromosome, graph);
        BestChromosome?.DisplayChromosome();

        var time = geneticAlgorithmEngine.TimeEvolving;
        Console.WriteLine($"Time taken: {time.TotalSeconds} seconds");


        // testing result class
        var geneticAlgorithmResults = new GeneticAlgorithmResults
        {
            graph = graph,
            ModularisationExcecutionTimeInMillisecond = (long)time.TotalMilliseconds,
            ModulesFromBestSolution = BestChromosome!.GetModules().Select(m => m.Clone()).ToList(),
            IntergeGeneFromBestSolution = BestChromosome.GetIntegerGenes().Select(g => (int)g.Value).ToList(),
            BestFitness = geneticAlgorithmEngine.BestChromosome.Fitness.Value
        };

        // Display the results

        return new GeneticAlgorithmExecutionResult()
        {
            GeneticAlgorithmResults = geneticAlgorithmResults
        };
    }

}




internal class MultiObjectiveFitnessFunction : IFitness
{
    public MultiObjectiveFitnessFunction()
    {
    }

    public double Evaluate(IChromosome chromosome)
    {
        chromosome.Fitness = 0.0; // Placeholder for actual fitness evaluation logic
        return chromosome.Fitness.Value;
    }
}