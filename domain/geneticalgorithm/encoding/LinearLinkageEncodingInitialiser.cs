using System;
using GeneticSharp;
using MA_GA.domain.geneticalgorithm.encoding;
using MA_GA.domain.module;
using MA_GA.Models;
using Microsoft.Extensions.Logging;
using QuikGraph.Algorithms.ConnectedComponents;

namespace MA_GA.domain.geneticalgorithm.encoding;

public sealed class LinearLinkageEncodingInitialiser
{

    /// <summary>
    /// Initializes a LinearLinkageEncoding with modules for each connected component in the graph.
    /// </summary>
    /// <param name="graph"></param>
    /// <returns></returns>
    public static LinearLinkageEncoding InitializeLinearLinkageEncodingWithModulesForEachConnectedCompponent(Graph graph)
    {
        var targetGraph = graph.GetGraph();
        var targetGraphEdges = graph.GetGraph().Edges.ToList();

        var connectedComponents = GraphService.GetConnectedComponentsFromGraph(targetGraph);

        // module for each connected component
        var modules = new List<Module>();

        foreach (var component in connectedComponents)
        {
            var module = new Module();



            foreach (var vertex in component)
            {
                module.AddIndex(vertex.GetIndex());
            }

            modules.Add(module);

        }

        // create chromosome with list of modules
        var modularisableElementSize = graph.GetModularisableElements().Count;
        var genes = new List<Gene>();
        for (int i = 0; i < modularisableElementSize; i++)
        {
            genes.Add(new Gene(i));
        }
        var integerGenes = genes.AsReadOnly();
        var linearLinkageEncoding = new LinearLinkageEncoding(graph, integerGenes);

        return LinearLinkageEncodingOperator.UpdateIntegerGenes(modules, linearLinkageEncoding);
    }

    /// <summary>
    /// Initializes a LinearLinkageEncoding Chromosome using to convert the result of a greedy algorithm into a LinearLinkageEncoding.
    /// </summary>
    /// <param name="graph"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>

    public static LinearLinkageEncoding InitializeLinearLinkageEncodingWithGreedyAlgorithm(Graph graph)
    {
        if (graph == null)
        {
            throw new ArgumentNullException(nameof(graph), "Graph cannot be null");
        }


        var modularisableElements = graph.GetModularisableElements();
        var edgeList = graph.GetGraph().Edges.ToList();

        var sortedElements = new Dictionary<string, List<int>>();

        var modules = new List<Module>();

        foreach (var element in modularisableElements)
        {
            if (element is DataObject dataObject)
            {
                if (dataObject.Component == null)
                {
                    Console.WriteLine("dont forget to uncomment partitioning process!!!!!!!!!!!!!!!!!!!!!");
                    throw new ArgumentException("DataObject component cannot be null", nameof(dataObject));
                }
                if (!sortedElements.ContainsKey(dataObject.Component))
                {
                    sortedElements[dataObject.Component] = new List<int>();
                }

                sortedElements[dataObject.Component].Add(dataObject.GetIndex());

            }

        }


        foreach (var component in sortedElements)
        {
            var module = new Module();
            foreach (var index in component.Value)
            {
                module.AddIndex(index);
            }
            modules.Add(module);
        }

        // create chromosome with list of modules
        var modularisableElementSize = graph.GetModularisableElements().Count;
        var integerGenes = Enumerable.Range(0, modularisableElementSize)
    .Select(i => new GeneticSharp.Gene(i))
    .ToList()
    .AsReadOnly();


        return LinearLinkageEncodingOperator.UpdateIntegerGenes(modules, new LinearLinkageEncoding(graph, integerGenes));

    }

}
