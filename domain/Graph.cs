using System;
using System.Collections.Concurrent;
using GeneticSharp;
using MA_GA.domain;
using QuikGraph;
using QuikGraph.Graphviz;

namespace MA_GA.Models;

/// <summary>
/// Graph class represents a directed graph structure containing nodes and edges.
/// It allows adding nodes, relations, and connections between nodes.
/// It also provides methods to retrieve nodes by ID or name, remove nodes and edges,
/// and read the current state of the graph.
/// </summary>

public class Graph
{
    // List to hold node and edge objects, including information objects
    List<IDataObject> nodeObjects;
    List<IObjectRelation> edgeObjects;

    // properties for creating graph using quikgraph
    private readonly Dictionary<int, DataObject> _nodeDictionary = [];
    private readonly Dictionary<int, ObjectRelation> _edgeDictionary = [];

    private readonly AdjacencyGraph<DataObject, IObjectRelation> _Graph;

    private readonly ConcurrentDictionary<ModularisableElement, List<ModularisableElement>> IncidentModularisableElements;
    private int EDGE_COUNT = 0;


    public Graph()
    {
        nodeObjects = new List<IDataObject>();
        edgeObjects = new List<IObjectRelation>();
        _Graph = GraphService.CreateAdjacencyGraph();
        IncidentModularisableElements = new ConcurrentDictionary<ModularisableElement, List<ModularisableElement>>();

    }

    public bool IsEmpty()
    {
        return _Graph.VertexCount == 0;
    }

    public void AddNodeToGraph(DataObject dataObject)
    {
        nodeObjects.Add(dataObject);
        // only add node to graph when it is not an information object
        if (dataObject.ObjectType == ObjectType.InformationObject)
        {
            return;
        }
        _Graph.AddVertex(dataObject);
        _nodeDictionary.Add(dataObject.GetIndex(), dataObject);
    }

    public void AddRelationToGraph(ObjectRelation relation)
    {


        if (relation == null)
        {
            throw new ArgumentNullException(nameof(relation), "Relation cannot be null");
        }
        // check if either source or / both target objects are information objects
        if (relation.SourceObject.ObjectType == ObjectType.InformationObject ||
            relation.TargetObject.ObjectType == ObjectType.InformationObject)
        {
            return; // do not add relation between two information objects
        }

        edgeObjects.Add(relation);
        _Graph.AddEdge(relation);
        _edgeDictionary.Add(EDGE_COUNT++, relation);
        // update vertex weights based on relation type
        relation.SourceObject.UpdateWeight(ObjectHelper.ConvertRelationTypeToWeight(relation.RelationType));
        relation.TargetObject.UpdateWeight(ObjectHelper.ConvertRelationTypeToWeight(relation.RelationType));

    }

    public List<ObjectRelation> FindIncidentRelationsForVertex(DataObject vertex)
    {
        var incidentEdges = new List<ObjectRelation>();

        foreach (var edge in _edgeDictionary)
        {
            if (edge.Value.SourceObject.GetIndex() == vertex.GetIndex() || edge.Value.TargetObject.GetIndex() == vertex.GetIndex())
            {
                incidentEdges.Add(edge.Value);
            }

        }

        return incidentEdges;
    }


    public DataObject GetNodeObjectByName(string name)
    {
        var nodeWanted = default(DataObject);
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name), "Name cannot be null or empty");
        }
        if (nodeObjects.Count == 0)
        {
            throw new InvalidOperationException("No data objects available");
        }


        foreach (var dataObject in nodeObjects)
        {
            if (dataObject.ShortName.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                nodeWanted = (DataObject?)dataObject;
            }
        }

        return nodeWanted;

    }


    public String GetNodeNameById(int id)
    {
        if (_nodeDictionary.TryGetValue(id, out var dataObject))
        {
            return dataObject.Name;
        }
        throw new KeyNotFoundException($"No node found with ID {id}");
    }

    public void ReadGraph()
    {
        Console.WriteLine("Graph contains the following nodes and edges:");
        foreach (var vertex in _Graph.Vertices)
        {
            Console.WriteLine($"Node: {vertex.Name}");
        }
        foreach (var edge in _Graph.Edges)
        {
            Console.WriteLine($"Edge from {edge.SourceObject.Name} to {edge.TargetObject.Name} with relation type {edge.RelationType}");
        }

        Console.WriteLine("Graphviz representation:");

        Console.WriteLine(GraphService.GenerateGraphToDOT(_Graph));

    }

    public AdjacencyGraph<DataObject, IObjectRelation> GetGraph()
    {
        return _Graph;
    }

    /// <summary>
    /// Retrieves all modularisable elements from the graph, including both vertices and edges.
    /// Throws an InvalidOperationException if the graph is empty or not initialized.
    /// </summary>
    /// <returns>A list of ModularisableElement objects representing the modularisable elements in the graph.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the graph is empty or not initialized.</exception>

    public List<ModularisableElement> GetModularisableElements()
    {
        var modularisableElements = new List<ModularisableElement>();
        if (_Graph == null || _Graph.VertexCount == 0)
        {
            throw new InvalidOperationException("Graph is empty or not initialized");
        }

        // Add vertices that are modularisable elements
        foreach (var vertex in _Graph.Vertices)
        {
            if (vertex is ModularisableElement modularisableElement)
            {
                modularisableElements.Add(vertex);
            }
        }


        return modularisableElements;
    }

    /// <summary>
    /// Retrieves a ModularisableElement by its index.
    /// This method checks both node and edge dictionaries for the given index.
    /// If the index is found in either dictionary, it returns the corresponding ModularisableElement.
    /// If the index is not found, it throws a KeyNotFoundException.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>

    public ModularisableElement GetModularisableElementByIndex(int index)
    {

        if (_nodeDictionary.ContainsKey(index))
        {
            return _nodeDictionary[index];
        }
        else
        {
            throw new KeyNotFoundException($"No ModularisableElement found with index {index}");
        }

    }

    /// <summary>
    /// Retrieves all incident modularisable elements for a given modularisable element.
    /// </summary>
    /// <param name="element">The modularisable element for which to find incident elements.</param>
    /// <returns>A list of incident ModularisableElement objects.</returns>

    public List<ModularisableElement> GetIncidentModularisableElements(ModularisableElement element)
    {

        return IncidentModularisableElements.GetOrAdd(
            element,
            key => GraphService.GetIncidentElements(key, this)
        );

    }

    public bool IsIsolatedVertex(DataObject dataObject)
    {

        return _Graph.OutDegree(dataObject) == 0;

    }

    public List<ObjectRelation> GetVertexEdgesByIndex(int index)
    {
        return _Graph.Edges
            .Where(e => e.Source.GetIndex() == index || e.Target.GetIndex() == index)
            .Select(e => (ObjectRelation)e)
            .ToList();
    }
}
