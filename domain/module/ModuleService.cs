
using System.Collections;
using GeneticSharp;
using MA_GA.domain.geneticalgorithm.encoding;
using MA_GA.Models;

namespace MA_GA.domain.module;
/// <summary>
/// ModuleService provides methods for manipulating modules, including merging, splitting, and dividing modules.
/// </summary>
public class ModuleService
{
    public static Module MergeModules(Module module1, Module module2)
    {
        // ger indices of both modules
        var indices1 = module1.GetIndices();
        var indices2 = module2.GetIndices();

        // create a new module to hold the merged indices
        Module mergedModule = new Module();

        int index1 = 0, index2 = 0;

        bool isNewModuleBuilt = false;

        while (!isNewModuleBuilt)
        {
            if (index1 < indices1.Count && index2 < indices2.Count)
            {
                // compare the current indices of both modules
                if (indices1[index1] < indices2[index2])
                {
                    // add index from module1 to merged module
                    mergedModule.AddIndex(indices1[index1]);
                    index1++;
                }
                else
                {
                    mergedModule.AddIndex(indices2[index2]);
                    index2++;
                }
            }
            else
            {
                // if one of the indices is exhausted, add the remaining indices from the other module
                while (index1 < indices1.Count)
                {
                    mergedModule.AddIndex(indices1[index1]);
                    index1++;
                }
                while (index2 < indices2.Count)
                {
                    mergedModule.AddIndex(indices2[index2]);
                    index2++;
                }
                isNewModuleBuilt = true;
            }
        }
        return mergedModule;

    }

    /// <summary>
    /// Find indices in a module that are not fully connected, and then split them into smaller modules.
    /// </summary>
    /// <param name="nonConnectedModule"></param>
    /// <param name="encoding"></param>
    /// <returns></returns> 

    public static List<Module> SplitNonIncidentModule(Module nonConnectedModule, LinearLinkageEncoding encoding)
    {
        var graph = encoding.GetGraph();
        var subgraphOfModule = GraphService.CreateSubgraphGraphFromIndices(nonConnectedModule.GetIndices().ToList(), graph);

        // Extract Graph and connected sets
        // get vertices of subgraph


        var connectedComponents = GraphService.GetConnectedComponentsFromGraph(subgraphOfModule); // List<HashSet<DataObject>>
        var components = new Dictionary<int, HashSet<DataObject>>();
        for (int i = 0; i < connectedComponents.Count; i++)
        {
            components[i] = connectedComponents[i];
        }


        var edgesOfModule = GetModuleEdges(nonConnectedModule, graph);
        var listOfConnectedSet = new List<HashSet<object>>();


        //Related Edges to connected sets
        foreach (var verticesOfConnectedSets in components.Values)
        {

            var edgesOfConnectedSet = edgesOfModule.Where(edge =>
             verticesOfConnectedSets.Contains(edge.SourceObject) || verticesOfConnectedSets.Contains(edge.TargetObject)).ToList();

            var indexOfConnectedSet = new HashSet<object>();


            foreach (var vertex in verticesOfConnectedSets)
            {
                indexOfConnectedSet.Add(vertex.GetIndex());
            }

            listOfConnectedSet.Add(indexOfConnectedSet);

        }

        // handle isolated edges by add them to separate modules
        // TODO: How handle isolated edges?
        /*
        foreach (var edge in edgesOfModule)
        {

            bool sourceInAny = components.Values.Any(v => v.Contains(edge.SourceObject));
            bool targetInAny = components.Values.Any(v => v.Contains(edge.TargetObject));

            if (!sourceInAny && !targetInAny)
            {

                listOfConnectedSet.Add([edge.GetIndex()]);
            }
        }
*/
        // Create new modules
        return listOfConnectedSet.Select(indices =>
        {
            var newModule = new Module();
            newModule.AddIndices(indices);
            return newModule;
        }).ToList();
    }

    public static HashSet<Module> DivideModuleRandomly(Module module, Graph graph)
    {
        if (module == null || graph == null)
        {
            throw new ArgumentNullException("Module or graph cannot be null.");
        }

        if (module.GetIndices().Count < 1)
        {
            throw new ArgumentException("Module must contain at least one index.");
        }

        var splitModules = new HashSet<Module>();

        var indices = new ArrayList(module.GetIndices().ToList());



        switch (indices.Count)
        {
            case 1:
                splitModules.Add(module);
                return splitModules;
            case 2:
                var newModule1 = new Module();
                newModule1.AddIndex((int)indices[0]);
                var newModule2 = new Module();
                newModule2.AddIndex((int)indices[1]);
                splitModules.Add(newModule1);
                splitModules.Add(newModule2);
                return splitModules;

            default:
                var halfSizeOfModule = indices.Count / 2;
                var remainingIndices = new List<int>(module.GetIndices());
                while (remainingIndices.Count > 0)
                {
                    var randomStartElementIndex = RandomizationProvider.Current.GetInt(0, remainingIndices.Count - 1);
                    var startElement = graph.GetModularisableElementByIndex(remainingIndices[randomStartElementIndex]);
                    var indicesOfSplitteModule = CreateIndicesOfSubGraphRandomly(startElement, graph, halfSizeOfModule, remainingIndices);

                    Module newModule = new Module();
                    newModule.AddIndices(indicesOfSplitteModule);
                    splitModules.Add(newModule);
                    remainingIndices.RemoveAll(x => indicesOfSplitteModule.Contains(x));

                }
                return splitModules;
        }

    }

    /*
        public static int DetermineSplittedModuleSize(List<object> indices)
        {
            return indices.Count / 2;
        }
    */
    /// <summary>
    /// Creates a subgraph of the given modularisable element by randomly selecting indices from the graph.
    /// It takes following parameters:
    /// - modularisableElement: The starting point for the subgraph.
    /// - graph: The graph from which the subgraph is created.
    /// - subgraphSize: The desired size of the subgraph.
    /// - indicesOfModule: The list of indices that are part of the module.
    /// </summary>
    /// /// <returns>A HashSet of selected indices representing the subgraph.</returns>
    public static HashSet<object> CreateIndicesOfSubGraphRandomly(ModularisableElement modularisableElement, Graph graph, int subgraphSize, List<int> indicesOfModule)
    {
        var selectedIndices = new HashSet<object>();
        var visitedModularisableElement = new HashSet<int>();
        var visitedEdges = new HashSet<ObjectRelation>();

        var queue = new Stack<ModularisableElement>();
        queue.Push(modularisableElement);

        while (queue.Count > 0 && selectedIndices.Count < subgraphSize)
        {
            var currentElement = queue.Pop();
            selectedIndices.Add(currentElement.GetIndex());

            if (currentElement is DataObject vertex)
            {

                var edgesOfCurrentVertex = graph.FindIncidentRelationsForVertex(vertex);

                var edgesInModuleAndNotVisited = edgesOfCurrentVertex
                .Where(e =>
                    indicesOfModule.Contains(e.Source.GetIndex()) && indicesOfModule.Contains(e.Target.GetIndex()) &&
                    !visitedEdges.Contains(e)).ToList().OrderBy(e => e.Weight).ToList();

                foreach (var edge in edgesInModuleAndNotVisited)
                {
                    if (edge.Source.Equals(vertex))
                    {
                        queue.Push(edge.Target);
                    }
                    else
                    {
                        queue.Push(edge.Source);
                    }
                    visitedEdges.Add(edge);
                }


            }

            visitedModularisableElement.Add(currentElement.GetIndex());

        }


        return selectedIndices;
    }

    public static List<ObjectRelation> GetModuleEdges(Module module, Graph graph)
    {


        return graph.GetGraph().Edges.Where(
            edge => module.CheckIndexInModule(edge.Source.GetIndex()) && module.CheckIndexInModule(edge.Target.GetIndex())
        ).Select(e => (ObjectRelation)e).ToList();
    }

    public static List<Module> DivideModuleRandomWalk2(Module selectedModule, Graph graph)
    {

        var resultSet = new HashSet<Module>();
        var indices = new List<int>(selectedModule.GetIndices());

        if (indices.Count == 1)
        {
            resultSet.Add(selectedModule);
            return resultSet.ToList();
        }

        if (indices.Count == 2)
        {
            var module1 = new Module();
            module1.AddIndex(indices[0]);
            var module2 = new Module();
            module2.AddIndex(indices[1]);
            resultSet.Add(module1);
            resultSet.Add(module2);
            return resultSet.ToList();
        }

        var randomSizeOfModule1 = indices.Count / 2;
        var remainingIndices = new List<int>(indices);

        while (remainingIndices.Count != 0)
        {
            var randomIndex = RandomizationProvider.Current.GetInt(0, remainingIndices.Count);
            var startElement = graph.GetModularisableElementByIndex(remainingIndices[randomIndex]);
            var indicesOfSubGraph = CreateIndicesOfSubGraphRandomly(startElement, graph, randomSizeOfModule1, remainingIndices);

            var newModule = new Module();
            newModule.AddIndices(indicesOfSubGraph);
            resultSet.Add(newModule);

            // Remove the indices of the newly created module from the remaining indices
            foreach (var index in indicesOfSubGraph)
            {
                remainingIndices.Remove((int)index);
            }

        }

        return resultSet.ToList();
    }
}
