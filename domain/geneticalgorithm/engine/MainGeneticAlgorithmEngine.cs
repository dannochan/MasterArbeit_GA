using System;
using System.Text;
using GeneticSharp;
using MA_GA.domain.geneticalgorithm.encoding;
using MA_GA.domain.geneticalgorithm.fitnessfunction;
using MA_GA.domain.geneticalgorithm.objective;
using MA_GA.domain.geneticalgorithm.parameter;
using MA_GA.domain.module;
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

    /**/
    private GeneticAlgorithmExecutionResult ModularisewithMultiObjectiveFitnessFunction(GeneticAlgorithmParameter geneticAlgorithmParameter, Graph graph, MutationWeight mutationWeight)
    {

        // TODO: ADD objective when available

        //  var fitnessFunction = new MultiObjectiveFitnessFunction();

        // build genetic algorithm engine
        var geneticAlgorithmEngine = new GeneticAlgorithmEngineBuilder.Builder()
            .Graph(graph)
            .GeneticAlgorithmParameter(geneticAlgorithmParameter)
            .Fitness(/*fitnessFunction*/null) // TODO: add multi objective fitness function
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

        var testCohesionObjective = new CohesionObjective(graph, 1);
        
            var testCouplingObjective = new CouplingObjective(graph, 1);

        var objectives = new List<Objective>
        {
                testCohesionObjective,
                testCouplingObjective
                //optional 
          //      new ModularityObjective(graph, 1),
        };

        var fitnessFunction = new FitnessFunction(objectives, graph);

        // string builder to store generation result
        var generationResultStringBuilder = new StringBuilder();
        // event handler for generation run and metrics collection
        var generationRunEventHandler = new EventHandler((sender, args) =>
        {
            var ga = (GeneticAlgorithm)sender;
            var best = (LinearLinkageEncoding)ga.BestChromosome;

            var modules = best.GetModules();

            var cohesionValue = objectives[0].CalculateValue(modules);
            var couplingValue = objectives[1].CalculateValue(modules);

            generationResultStringBuilder.Append(
                $"{ga.GenerationsNumber}-{best.Fitness}-{modules.Count}-{cohesionValue}-{couplingValue}!"
            );
        });


        // build genetic algorithm engine
        var geneticAlgorithmEngine = new GeneticAlgorithmEngineBuilder.Builder()
            .Graph(graph)
            .GeneticAlgorithmParameter(geneticAlgorithmParameter)
            .MutationWeight(mutationWeight)
            .Fitness(fitnessFunction)
            .GenerationMetricsHandler(generationRunEventHandler)
            .CreatingEngineForWeightedSumProblem();

        var taskExecutor = new ParallelTaskExecutor
        {
            MinThreads = 1,
            MaxThreads = Environment.ProcessorCount
        };

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

        var geneticAlgorithmResults = new GeneticAlgorithmResults
        {
            graph = graph,
            ModularisationExcecutionTimeInMillisecond = (long)time.TotalMilliseconds,
            ModulesFromBestSolution = BestChromosome!.GetModules().Select(m => m.Clone()).ToList(),
            IntergeGeneFromBestSolution = BestChromosome.GetIntegerGenes().Select(g => (int)g.Value).ToList(),
            BestFitness = geneticAlgorithmEngine.BestChromosome.Fitness.Value
        };


        return new GeneticAlgorithmExecutionResult()
        {
            GeneticAlgorithmParameter = geneticAlgorithmParameter,
            GeneticAlgorithmResults = geneticAlgorithmResults,
            GenerationResultString = generationResultStringBuilder.ToString()
        };
    }

}

