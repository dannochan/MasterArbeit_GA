using System;
using System.Dynamic;
using System.Runtime.CompilerServices;
using GeneticSharp;
using MA_GA.domain.geneticalgorithm.encoding;
using MA_GA.Models;
using QuikGraph;
using QuikGraph.Algorithms.ConnectedComponents;
using QuikGraph.Predicates;

namespace MA_GA.domain.module;

public class ModuleInformationService
{

    public static HashSet<DataObject> GetVerticesOfModule(Module module, Graph graph)
    {

        return module.GetIndices()
            .Select(i => graph.GetModularisableElementByIndex((int)i))
            .OfType<DataObject>()
            .ToHashSet();

    }

    public static Module GetModule(ModularisableElement element, LinearLinkageEncoding encoding)
    {
        return encoding.GetModules()
            .First(m => m.GetIndices().Contains(element.GetIndex()));
    }

    public static List<Module> GetModuleNeighbors(
        Module targetModule,
        LinearLinkageEncoding encoding
    )
    {
        var remainingModules = new List<Module>(encoding.GetModules());

        var neighborModulesWhereVertexIsIncidentToOtherModuleEdge =
            GetModuleNeighborsWhereVertexIsIncideentToOtherModuleEdge(targetModule, encoding.GetGraph(), remainingModules);
        remainingModules.RemoveAll(module => neighborModulesWhereVertexIsIncidentToOtherModuleEdge.Contains(module));
        // var neighborModulesWhereEdgeIsIncidentToOtherModuleVertex =
        // GetModuleNeighborsWhereEdgeIsIncidentToOtherModuleVertex(targetModule, encoding.GetGraph(), remainingModules);

        var neighborModules = new List<Module>();
        neighborModules.AddRange(neighborModulesWhereVertexIsIncidentToOtherModuleEdge);
        //  neighborModules.AddRange(neighborModulesWhereEdgeIsIncidentToOtherModuleVertex);
        neighborModules = neighborModules.Distinct().ToList();
        if (neighborModules.Contains(targetModule))
        {
            neighborModules.Remove(targetModule);
        }
        return neighborModules;

    }



    private static List<Module> GetModuleNeighborsWhereVertexIsIncideentToOtherModuleEdge(
        Module targetModule,
        Graph graph,
        List<Module> modules
    )
    {

        var neighborModules = new List<Module>();
        modules.ForEach(module =>
        {
            var edgesInOtherModule = module.GetIndices()
                .SelectMany(graph.GetVertexEdgesByIndex)
                .Where(relation => targetModule.CheckIndexInModule(relation.Source.GetIndex()) ||
                                   targetModule.CheckIndexInModule(relation.Target.GetIndex()))
                .ToList();

            if (edgesInOtherModule.Any())
            {
                neighborModules.Add(module);
            }
        }
        );
        return neighborModules;

    }

    private static List<Module> GetModuleNeighborsWhereEdgeIsIncidentToOtherModuleVertex(
        Module targetModule,
        Graph graph,
        List<Module> modules
    )
    {
        var neighborModules = new List<Module>();

        var edgesInTargetModule = targetModule.GetIndices()
                .SelectMany(graph.GetVertexEdgesByIndex)
                .Where(relation => targetModule.CheckIndexInModule(relation.Source.GetIndex()) ||
                                   targetModule.CheckIndexInModule(relation.Target.GetIndex()))
                .ToList();

        modules.ForEach(module =>
        {
            var verticesInOtherModule = module.GetIndices()
                .Select(index => graph.GetModularisableElementByIndex((int)index))
                .OfType<DataObject>()
                .ToList();

            foreach (var edge in edgesInTargetModule)
            {
                if (verticesInOtherModule.Any(vertex => vertex.GetIndex() == edge.Source.GetIndex() ||
                                                         vertex.GetIndex() == edge.Target.GetIndex()))
                {
                    neighborModules.Add(module);
                    break; // No need to check further for this module
                }
            }
        });
        return neighborModules;

    }

    /// <summary>
    /// Checks if a module consists of a single vertex that is isolated in the graph.
    /// </summary>
    /// <param name="module"></param>
    /// <param name="graph"></param>
    /// <returns></returns>

    public static bool IsModuleConsistOfSoloVertex(Module module, Graph graph)
    {
        if (module.GetIndices().Count != 1)
        {
            return false;
        }

        var vertex = graph.GetModularisableElementByIndex((int)module.GetIndices().First());
        if (vertex is DataObject dataObject)
        {
            return new IsolatedVertexPredicate<DataObject, ObjectRelation>(
                (QuikGraph.IBidirectionalGraph<DataObject, ObjectRelation>)graph.GetGraph()
            ).Test(dataObject);
        }
        return false;
    }

    public static bool IsModuleConnected(Module module, Graph graph)
    {
        var indices = module.GetIndices();

        return CheckModuleConnectivityByIndices(module.GetIndices().ToList(), graph);
    }

    public static bool CheckModuleConnectivityByIndices(
        List<int> indices,
        Graph graph
    )
    {
        var indiceList = indices.Select(i => (int)i).ToList();
        var subgraph = GraphService.CreateSubgraphGraphFromIndices(indiceList, graph);

        var graphConnectivityInspector = GraphService.GetConnectivityInspector(subgraph);

        if (!graphConnectivityInspector.IsConnected())
        {
            return false;
        }
        var edgesOfIndices = indices.SelectMany(i => graph.GetVertexEdgesByIndex(i)).ToList();

        var remainingEdges = edgesOfIndices
            .Where(edge => !subgraph.Edges.Contains(edge))
            .ToList();

        var subgraphVertexSet = new HashSet<DataObject>(
            subgraph.Vertices.Select(v => v as DataObject).Where(v => v != null)
        );

        return remainingEdges.All(edge =>
        {
            return subgraphVertexSet.Contains(edge.Source) || subgraphVertexSet.Contains(edge.Target);
        });


    }

    public static List<ObjectRelation> GetModuleEdges(
        Module module,
        Graph graph
    )
    {
        return module.GetIndices()
            .SelectMany(i => graph.GetVertexEdgesByIndex((int)i))
            .ToList();
    }

    public static List<ModularisableElement> GetModularisableElementsOfModule(
        Module module,
        Graph graph
    )
    {
        return module.GetIndices()
            .Select(i => graph.GetModularisableElementByIndex((int)i))
            .ToList();
    }

    public static HashSet<ObjectRelation> GetAllBoundaryEdgesOfModules(
        List<Module> modules,
        Graph graph
    )
    {
        var boundaryEdges = new HashSet<ObjectRelation>();

        foreach (var module in modules)
        {
            Console.WriteLine($"Processing module with indices: {string.Join(", ", module.GetIndices())}");
            var edges = GetBoundaryEdgesOfModule(module, graph);
            boundaryEdges.UnionWith(edges);

        }



        return boundaryEdges;
    }

    /// <summary>
    /// Get edges that occur between two modules. by checking if a edge has an end in one module and the other not
    /// </summary>
    /// <param name="module"></param>
    /// <param name="graph"></param>
    /// <returns></returns>/

    public static HashSet<ObjectRelation> GetBoundaryEdgesOfModule(
        Module module,
        Graph graph
    )
    {
        var boundaryEdges = graph.GetGraph().Edges.Where(
            edge =>
            {
                if (!module.CheckIndexInModule(edge.Source.GetIndex()) &&
                    !module.CheckIndexInModule(edge.Target.GetIndex()))
                {
                    return false;
                }
                bool containSourceVertex = module.CheckIndexInModule(edge.Source.GetIndex());
                bool containTargetVertex = module.CheckIndexInModule(edge.Target.GetIndex());
                return containSourceVertex ^ containTargetVertex; // XOR: one is in the module, the other is not
            }
        ).Select(e => (ObjectRelation)e).ToHashSet();

        return boundaryEdges;
    }

    public static List<Module> GetIncidentModules(ModularisableElement element, LinearLinkageEncoding encoding)
    {
        if (element is DataObject vertex)
        {
            return GetIncidenModulesToVertex(vertex, encoding);
        }

        throw new ArgumentException("Element must be either a DataObject or an ObjectRelation.");
    }




    private static List<Module> GetIncidenModulesToVertex(DataObject vertex, LinearLinkageEncoding encoding)
    {
        var graph = encoding.GetGraph();

        var incidentEdges = graph.GetGraph().Edges
            .Where(e => e.Source.Equals(vertex) || e.Target.Equals(vertex))
            .ToList();
        var incidentModules = encoding.GetModules();


        return incidentEdges.Select(edge => incidentModules.FirstOrDefault(module => !module.CheckIndexInModule(vertex.GetIndex()) && (module.CheckIndexInModule(edge.Source.GetIndex()) || module.CheckIndexInModule(edge.Target.GetIndex()))))
            .Where(module => module != null)
            .Distinct()
            .ToList();
    }

    /*
        private static List<Module> GetIncidentModulesToEdge(
            ObjectRelation edge,
            LinearLinkageEncoding encoding

        )
        {
            var moduleOfEdge = GetModule(edge, encoding);
            var moduleSourceVertex = GetModule(edge.Source, encoding);
            var moduleTargetVertex = GetModule(edge.Target, encoding);

            var modules = new List<Module>();


            if (moduleSourceVertex != moduleOfEdge)
            {
                modules.Add(moduleSourceVertex);

            }
            if (moduleTargetVertex != moduleOfEdge && !modules.Contains(moduleTargetVertex))
            {
                modules.Add(moduleTargetVertex);
            }

            return modules;
        }

        */

    public static List<ModularisableElement> GetNonConnectedModularisableElements(
        Module module,
        Graph graph)
    {
        var nonConnectedElements = new List<ModularisableElement>();

        var modularisableElements = module.GetIndices()
            .Select(i => graph.GetModularisableElementByIndex((int)i))
            .ToList();
        var verticesIfModule = modularisableElements.OfType<DataObject>().ToList();
        var edgesIfModule = module.GetIndices()
            .SelectMany(i => graph.GetVertexEdgesByIndex((int)i))
            .ToList();

        foreach (var vertex in verticesIfModule)
        {
            bool isVertexIncidentToEdge = edgesIfModule.Any(edge =>
                        edge.Source.Equals(vertex) || edge.Target.Equals(vertex));
            if (!isVertexIncidentToEdge)
            {
                nonConnectedElements.Add(vertex);
            }
        }


        return nonConnectedElements;


    }

    public static bool IsIsolated(Module module, Graph graph)
    {
        var indices = module.GetIndices().ToList();
        if (indices.Count == 0)
        {
            return true; // An empty module is considered isolated
        }

        if (indices.Count > 1)
        {
            return false;
        }
        var modularisableElement = graph.GetModularisableElementByIndex((int)indices.First());

        if (modularisableElement is DataObject vertex)
        {
            return graph.IsIsolatedVertex(vertex);
        }
        throw new ArgumentException("Module must contain either a DataObject or an ObjectRelation.");

    }

}
