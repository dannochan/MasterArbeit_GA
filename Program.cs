using System.Text.Json;
using MA_GA.domain;
using MA_GA.domain.geneticalgorithm.engine;
using MA_GA.domain.geneticalgorithm.parameter;
using MA_GA.domain.GreedyAlgorithm;
using MA_GA.models.optimizationresult;
using MA_GA.Models;
using MA_GA.util;
using Microsoft.Extensions.Logging;
using QuikGraph;


class MainApp
{
    static void Main(string[] args)
    {
        // logger
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger logger = factory.CreateLogger("Program");
        // running this program in vs code and use dotnet run to compile;
        // this make sure reading the JSON files on project based directory (not bin/debug)
        string dir = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;

        // define the path to the JSON file
        string filePath = Path.Combine(dir, "data", "SmallTestcase.json");
        string filePath2 = Path.Combine(dir, "data", "BigTestcase-2.json");

        var isBusy = false;

        // define genetic algorithm parameters create ga parameter for engine
        var geneticAlgorithmParameter = new GeneticAlgorithmParameter(
            "Interger",
            "Tournament",
            "ElitismSelection",
            "GroupCrossover",
            "GraftMutation",
            10, // Population size
            0.8f, // Crossover rate
            0.1f, // Mutation rate
            50, // Max generations
            2, // Tournament size
            0.5f, // Elitism count
            0.01, // Converged gene rate
            0.01, // Convergence rate
            0, // Count generation
            10, // Minimum Pareto set size
            100, // Maximum Pareto set size
            true, // Set to true to use weighted sum method
            true // Set to true to use greedy partition
        )
        {

        };

        // define the weights for different relation types. you can adjust these weights based on your requirements
        var dataObjectRelationWeight = new DataObjectRelationWeight(
            20.0, // conjunctionWeight
             15.0, // disjunctionWeight
              25.0, // exclusiveDisjunctionWeight
               20.0, // createWeight
               15.0, // readWeight
                15.0, // updateWeight 
                0.0,  // deleteWeight
                 15.0, // relatedToWeight
                  20.0, // partOfWeight
                  20.0) // isAWeight
        {

        };


        var mutationRateArry = new float[] { 0.1f, 0.4f, 0.8f };
        var crossoverRateArry = new float[] { 0.1f, 0.5f, 1f };
        var populationSizeArry = new int[] { 20, 100, 200 };
        var maxGenerationArry = new int[] { 50, 100, 200 };
        var selectionPressureArry = new int[] { 2, 7, 20 };

        var geneticAlgorithmParameterCombinations = GenerateGAParameterCombinations(mutationRateArry, crossoverRateArry, populationSizeArry, maxGenerationArry, selectionPressureArry);


        // object to hold the data
        Graph dataObjectCenter = new Graph(dataObjectRelationWeight);


        GraphObject rawObject;

        using (StreamReader sr = new StreamReader(filePath))
        {
            Console.WriteLine("Reading JSON file...");
            string json = sr.ReadToEnd();
            rawObject = JsonSerializer.Deserialize<GraphObject>(json);

        }

        if (rawObject != null)
        {
            lock (dataObjectCenter)
            {
                ObjectHelper.MapDataObjects(rawObject, dataObjectCenter, logger);
            }
            //      Console.WriteLine(GraphService.GenerateGraphToDOT(dataObjectCenter.GetGraph()));
        }

        if (dataObjectCenter.IsEmpty())
        {
            logger.LogError("DataObjects are empty, proceeding with graph displaying.");
            throw new InvalidOperationException("DataObjects are empty, cannot proceed with graph displaying.");
        }

        logger.LogInformation("DataObjects loaded successfully. Proceeding with graph partitioning.");
        var graph = dataObjectCenter.GetGraph();
        if (graph == null)
        {
            logger.LogError("Graph is null after creation.");
            throw new InvalidOperationException("Graph is null, cannot proceed with graph processing.");
        }

        // uncomment to deactivate greedy partition algorithm
        lock (dataObjectCenter)
        {
            ProcessGraphPartitioning(logger, graph);
        }

        for (int i = 0; i < geneticAlgorithmParameterCombinations.Count(); i++)
        {
            // geneticAlgorithmParameter.MutationRate = mutationRateArry[i];
            // run multiple times to observe the effect of mutation rate on optimization result and 
            // parameter configuration replication
            if (!isBusy)
            {
                for (int j = 0; j < 10; j++)
                {
                    isBusy = true;
                    GeneticAlgorithmExecutionResult optimizationResult;
                    lock (dataObjectCenter)
                    {
                        optimizationResult = RunGAEngine(logger, dataObjectCenter, geneticAlgorithmParameter);
                    }
                    ExportOptimizationResultToCsv(logger, optimizationResult);
                    isBusy = false;
                }


            }

        }


    }

    public static IEnumerable<GeneticAlgorithmParameter> GenerateGAParameterCombinations(float[] mutationRateArry, float[] crossoverRateArry, int[] populationSizeArry, int[] maxGenerationArry, int[] selectionPressureArry)
    {


        return from mutationRate in mutationRateArry
               from crossoverRate in crossoverRateArry
               from populationSize in populationSizeArry
               from maxGeneration in maxGenerationArry
               from selectionPressure in selectionPressureArry
               select new GeneticAlgorithmParameter(
                   "Interger",
                   "Tournament",
                   "ElitismSelection",
                   "GroupCrossover",
                   "GraftMutation",
                   populationSize,
                   (float)crossoverRate,
                   (float)mutationRate,
                   maxGeneration,
                   (int)selectionPressure,
                   0.5f,
                   0.01f,
                   0.01f,
                   0,
                   10,
                   100,
                   true,
                   true
               );


    }

    private static void ExportOptimizationResultToCsv(ILogger logger, GeneticAlgorithmExecutionResult optimizationResult)
    {
        // output results to CSV
        var csvGenerator = new CsvGenerator();
        string dir = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
        string outputFilePath = Path.Combine(dir, "output", "GeneticAlgorithmResults.csv");
        logger.LogInformation("Generating CSV output.");
        csvGenerator.AppendToCsvAsync(optimizationResult, outputFilePath).Wait();
        logger.LogInformation("CSV output generated successfully.");
    }

    private static void ProcessGraphPartitioning(ILogger logger, AdjacencyGraph<DataObject, IObjectRelation> graph)
    {
        DisplayGraphInfo(graph, logger);

        void DisplayGraphInfo(AdjacencyGraph<DataObject, IObjectRelation> graph, ILogger logger)
        {
            // output edge count
            Console.WriteLine($"Graph contains {graph.VertexCount} vertices and {graph.EdgeCount} edges.");
            var algorithm = new GraphPartitionGreedyAlgorithm(graph);
            algorithm.CreatePriorityList();
            Console.WriteLine("Priority List created successfully. now partitioning the graph.");
            // partition the graph
            logger.LogInformation("Starting graph partitioning.");
            var partitionResult = algorithm.PartitionGraph();
            logger.LogInformation("Graph partitioning completed successfully.");
            // output partition result
            Console.WriteLine(GraphService.DiplayGraphByComponents(partitionResult));
            // generate DOT representation of the graph

            //   CreateClusteredGraphAndDisplay(partitionResult);
        }
    }

    private static GeneticAlgorithmExecutionResult RunGAEngine(ILogger logger, Graph dataObjectCenter, GeneticAlgorithmParameter geneticAlgorithmParameter)
    {

        var gaEngine = new MainGeneticAlgorithmEngine();
        logger.LogInformation("Running genetic algorithm engine.");
        var optimizationResult = gaEngine.run(dataObjectCenter, geneticAlgorithmParameter);
        Console.WriteLine(optimizationResult.GeneticAlgorithmResults.DisplaySolutionUsingShortName());
        logger.LogInformation("Genetic algorithm engine run completed.");

        return optimizationResult;

    }

    private static void CreateClusteredGraphAndDisplay(AdjacencyGraph<DataObject, IObjectRelation> partitionResult)
    {
        var newClusterGraph = GraphService.CreateClusteredGraph(partitionResult);
        Console.WriteLine(newClusterGraph.ClustersCount);
        var dotRepresentation = GraphService.GenerateClusteredGraphToDOT(newClusterGraph);
        Console.WriteLine("Graphviz DOT representation:");
        Console.WriteLine(dotRepresentation);
    }
}