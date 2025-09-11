using System.Text.Json;
using MA_GA.domain;
using MA_GA.domain.geneticalgorithm.engine;
using MA_GA.domain.geneticalgorithm.parameter;
using MA_GA.domain.GreedyAlgorithm;
using MA_GA.Models;
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
        // object to hold the data
        Graph dataObjectCenter = new Graph();
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

        // Run the genetic algorithm engine
        for (int i = 0; i < 10; i++)
        {
            RunGAEngine(logger, dataObjectCenter);
        }


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

    private static void RunGAEngine(ILogger logger, Graph dataObjectCenter)
    {
        // create ga parameter for engine
        var geneticAlgorithmParameter = new GeneticAlgorithmParameter(
            "Interger",
            "Roulette",
            "ElitismSelection",
            "GroupCrossover",
            "GraftMutation",
            100, // Population size
            0.8f, // Crossover rate
            0.7f, // Mutation rate
            100, // Max generations
            5, // Tournament size
            0.5f, // Elitism count
            0.01, // Converged gene rate
            0.01, // Convergence rate
            0, // Count generation
            10, // Minimum Pareto set size
            100, // Maximum Pareto set size
            true, // Set to true to use weighted sum method
            true
        )
        {

        };
        var gaEngine = new MainGeneticAlgorithmEngine();
        logger.LogInformation("Running genetic algorithm engine.");
        var optimizationResult = gaEngine.run(dataObjectCenter, geneticAlgorithmParameter);
        Console.WriteLine(optimizationResult.GeneticAlgorithmResults.DisplaySolutionUsingShortName());
        logger.LogInformation("Genetic algorithm engine run completed.");
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