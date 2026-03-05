using System;
using System.Text.RegularExpressions;
using MA_GA.domain.module;
using MA_GA.Models;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.ConnectedComponents;
using QuikGraph.Graphviz;
using QuikGraph.Graphviz.Dot;

namespace MA_GA.domain;

public static class GraphService
{

    /// <summary>
    ///  Creates a adjacency graph for the given data objects and their relations.
    /// </summary>
    /// <returns></returns>
    public static AdjacencyGraph<DataObject, IObjectRelation> CreateAdjacencyGraph()
    {
        var graph = new AdjacencyGraph<DataObject, IObjectRelation>(
            allowParallelEdges: true
        );
        return graph;
    }

    /// <summary>
    /// Generates a Graphviz DOT representation of the given graph.
    /// This method formats the vertices and edges according to their types and relations.
    /// The vertex shape is set to Ellipse for Information Objects and Rectangle for other types.
    /// The edge label is set to the relation type.
    /// The method throws an ArgumentNullException if the graph is null.
    /// </summary>
    /// <param name="graph"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>

    public static string GenerateGraphToDOT(AdjacencyGraph<DataObject, IObjectRelation> graph)
    {
        if (graph == null)
        {
            throw new ArgumentNullException("Graph cannot be null");
        }

        string graphviz = graph.ToGraphviz(algo =>
        {
            algo.Generate();
            algo.VisitedGraph = graph;

            algo.CommonVertexFormat.Shape = GraphvizVertexShape.Rectangle;
            algo.FormatVertex += (sender, args) =>
            {
                var vertex = args.Vertex;
                args.VertexFormat.Label = vertex.Name;

                args.VertexFormat.Shape = vertex.ObjectType == ObjectType.InformationObject
                    ? GraphvizVertexShape.Ellipse
                    : GraphvizVertexShape.Rectangle;

                // Fix: Add "cluster_" prefix for GraphViz clusters
                if (vertex.Component != null)
                {
                    args.VertexFormat.Group = $"cluster_{vertex.Component}";
                }


            };

            algo.FormatEdge += (sender, args) =>
            {
                var edge = args.Edge;
                args.EdgeFormat.Label = new GraphvizEdgeLabel
                {
                    Value = edge.RelationType.ToString()
                };
            };

            algo.FormatCluster += (sender, args) =>
                  {
                      string clusterName = args.Cluster.Vertices.FirstOrDefault()?.Component;
                      if (clusterName.StartsWith("cluster_"))
                      {
                          string componentName = clusterName.Substring("cluster_".Length);

                          args.GraphFormat.Label = $"Component: {componentName}";

                          args.GraphFormat.FontColor = GraphvizColor.Black;

                          args.GraphFormat.PenWidth = 2;
                      }
                  };


        }

        );

        return graphviz;


    }


    public static string GenerateClusteredGraphToDOT(ClusteredAdjacencyGraph<DataObject, IObjectRelation> graph)
    {
        if (graph == null)
        {
            throw new ArgumentNullException("Graph cannot be null");
        }

        string graphviz = graph.ToGraphviz(algo =>
        {
            algo.Generate();
            algo.VisitedGraph = graph;

            // layout
            algo.GraphFormat.RankDirection = GraphvizRankDirection.LR;

            algo.GraphFormat.Splines = GraphvizSplineType.Curved;
            algo.GraphFormat.IsConcentrated = true;

            algo.CommonVertexFormat.Shape = GraphvizVertexShape.Rectangle;
            algo.FormatVertex += (sender, args) =>
            {
                var vertex = args.Vertex;
                args.VertexFormat.Label = vertex.Name + vertex.GetIndex();
                args.VertexFormat.Group = vertex.Component;

                args.VertexFormat.Shape = vertex.ObjectType == ObjectType.InformationObject
                    ? GraphvizVertexShape.Ellipse
                    : GraphvizVertexShape.Rectangle;

                // Fix: Add "cluster_" prefix for GraphViz clusters
                if (vertex.Component != null)
                {
                    args.VertexFormat.Group = $"cluster_{vertex.Component}";
                }


            };

            algo.FormatEdge += (sender, args) =>
            {
                var edge = args.Edge;
                args.EdgeFormat.Label = new GraphvizEdgeLabel
                {
                    //  Value = edge.RelationType.ToString()
                    Value = edge.EdgeNumber.ToString()
                };
                if (edge.Source.Component != edge.Target.Component)
                {
                    args.EdgeFormat.Style = GraphvizEdgeStyle.Dashed;
                    args.EdgeFormat.IsConstrained = false;
                    args.EdgeFormat.StrokeColor = GraphvizColor.Gray;
                }
            };


        }

        );

        return graphviz.Replace("digraph G {", "digraph G { overlap=\"false\";");


    }


    public static string DiplayGraphByComponents(AdjacencyGraph<DataObject, IObjectRelation> graph)
    {
        if (graph == null || graph.VertexCount == 0)
        {
            return "Graph is empty.";
        }

        var components = graph.Vertices
            .GroupBy(v => v.Component)
            .Select(g => new { Component = g.Key, Vertices = g.ToList() })
            .ToList();

        var result = new System.Text.StringBuilder();
        foreach (var component in components)
        {
            result.AppendLine($"Component: {component.Component}");
            foreach (var vertex in component.Vertices)
            {
                result.AppendLine($" - Vertex: {vertex.Name}, Index: {vertex.GetIndex()}, Weight: {vertex.Weight}");
            }
        }

        return result.ToString();
    }


    public static List<DataObject> GetVertexNeighbors(DataObject vertex, AdjacencyGraph<DataObject, IObjectRelation> graph)
    {

        return graph.Edges
            .Where(e => e.SourceObject.Equals(vertex) || e.TargetObject.Equals(vertex))
            .Select(e => e.SourceObject.Equals(vertex) ? (DataObject)e.TargetObject : (DataObject)e.SourceObject)
            .ToList();

    }

    /// <summary>
    /// Retrieves all incident elements (both vertices and edges) for a given modularisable element.
    /// If the element is an ObjectRelation, it returns the source and target objects.
    /// If the element is a DataObject, it retrieves all edges connected to that vertex.
    /// </summary>
    /// <param name="element">The modularisable element for which to find incident elements.</param>
    /// <param name="graph">The graph containing the modularisable elements.</param>
    /// <returns>A list of incident ModularisableElement objects.</returns>
    public static List<ModularisableElement> GetIncidentElements(ModularisableElement element, Graph graph)
    {
        var incidentElements = new List<ModularisableElement>();

        var vertex = (DataObject)element;
        var edges = graph.GetGraph().Edges
            .Where(e => e.SourceObject.Equals(vertex) || e.TargetObject.Equals(vertex))
            .ToList();
        foreach (var edge in edges)
        {
            if (vertex.Equals(edge.SourceObject))
            {
                incidentElements.Add(edge.TargetObject);
            }
            else
            {
                incidentElements.Add(edge.SourceObject);
            }
        }

        return incidentElements;
    }

    /// <summary>
    /// Creates a subgraph from the original graph based on the provided indices.
    /// The subgraph will include vertices and edges corresponding to the indices of modularisable elements.
    /// </summary>
    /// <param name="indices">List of indices representing the modularisable elements to include in the subgraph.</param>
    /// <param name="originalGraph">The original graph from which to create the subgraph.</param>
    /// <returns>A new AdjacencyGraph containing the subgraph.</returns>

    public static AdjacencyGraph<DataObject, IObjectRelation> CreateSubgraphGraphFromIndices(
    List<int> indices,
    Graph originalGraph

)
    {

        var modularisableElements = indices.Select(index => originalGraph.GetModularisableElementByIndex(index))
            .ToList();
        var vertices = modularisableElements.Select(me => me as DataObject)
            .Where(v => v != null)
            .Cast<DataObject>()
            .ToList();
        var edges = originalGraph.GetGraph().Edges
            .Select(me => me as ObjectRelation)
            .Where(e => e != null)
            .Cast<ObjectRelation>()
            .ToList();

        var subgraph = CreateAdjacencyGraph();

        foreach (var vertex in vertices)
        {
            subgraph.AddVertex(vertex);
        }

        foreach (var edge in edges)
        {
            if (subgraph.ContainsVertex(edge.SourceObject) && subgraph.ContainsVertex(edge.TargetObject))
            {
                subgraph.AddEdge(edge);
            }
        }
        GraphService.DiplayGraphByComponents(subgraph);

        return subgraph;
    }

    public static ClusteredAdjacencyGraph<DataObject, IObjectRelation> CreateClusteredGraph(AdjacencyGraph<DataObject, IObjectRelation> graph)
    {

        var clusteredGraph = new ClusteredAdjacencyGraph<DataObject, IObjectRelation>(graph);
        clusteredGraph.AddVertexRange(graph.Vertices);
        clusteredGraph.AddEdgeRange(graph.Edges);

        var vertexComponents = graph.Vertices.GroupBy(v => v.Component);


        foreach (var component in vertexComponents)
        {
            var cluster = clusteredGraph.AddCluster();
            foreach (var vertex in component)
            {
                cluster.AddVertex(vertex);
            }

            foreach (var edge in graph.Edges)
            {
                if (component.Contains(edge.Source) && component.Contains(edge.Target))
                {
                    cluster.AddEdge(edge);
                }
            }
        }
        return clusteredGraph;

    }


    public static List<HashSet<DataObject>> GetConnectedComponentsFromGraph(AdjacencyGraph<DataObject, IObjectRelation> graph)
    {
        // This method is a placeholder for future implementation.
        // It will be used to retrieve connected components from the graph.
        // Currently, it does not perform any operations.

        var undirectedGraph = new UndirectedGraph<DataObject, IObjectRelation>(false);
        foreach (var vertex in graph.Vertices)
        {
            undirectedGraph.AddVertex(vertex);

        }
        foreach (var edge in graph.Edges)
        {
            undirectedGraph.AddEdge(edge);
        }


        return new GraphConnectivityInSpector<DataObject, IObjectRelation>(undirectedGraph).ConnectedSets(); // new ConnectedComponentsAlgorithm<DataObject, IObjectRelation>(undirectedGraph);

    }

    public static GraphConnectivityInSpector<DataObject, IObjectRelation> GetConnectivityInspector(AdjacencyGraph<DataObject, IObjectRelation> graph)
    {
        // This method is a placeholder for future implementation.
        // It will be used to retrieve connected components from the graph.
        // Currently, it does not perform any operations.

        var undirectedGraph = new UndirectedGraph<DataObject, IObjectRelation>(false);
        foreach (var vertex in graph.Vertices)
        {
            undirectedGraph.AddVertex(vertex);

        }
        foreach (var edge in graph.Edges)
        {
            undirectedGraph.AddEdge(edge);
        }


        return new GraphConnectivityInSpector<DataObject, IObjectRelation>(undirectedGraph); // new ConnectedComponentsAlgorithm<DataObject, IObjectRelation>(undirectedGraph);

    }



}
