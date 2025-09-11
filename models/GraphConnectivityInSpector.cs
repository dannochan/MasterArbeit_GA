using QuikGraph;
using QuikGraph.Algorithms.ConnectedComponents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MA_GA.Models;

    /// <summary>
    /// Allows obtaining various connectivity aspects of a graph.
    /// </summary>
    
        public class GraphConnectivityInSpector<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {
        private readonly IUndirectedGraph<TVertex, TEdge> _graph;
        private List<HashSet<TVertex>> _connectedSets;
        private Dictionary<TVertex, HashSet<TVertex>> _vertexToConnectedSet;

        public GraphConnectivityInSpector(IUndirectedGraph<TVertex, TEdge> graph)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            Init();
        }

        private void Init()
        {
            _connectedSets = null;
            _vertexToConnectedSet = new Dictionary<TVertex, HashSet<TVertex>>();
        }

        /// <summary>
        /// Returns true if the graph is connected.
        /// </summary>
        public bool IsConnected()
        {
            return ConnectedSets().Count == 1 && _graph.VertexCount > 0;
        }

        /// <summary>
        /// Returns the set of all vertices in the maximally connected component with the specified vertex.
        /// </summary>
        public HashSet<TVertex> ConnectedSetOf(TVertex vertex)
        {
            if (!_graph.ContainsVertex(vertex))
                throw new ArgumentException("Vertex does not exist in the graph.");

            if (_vertexToConnectedSet.TryGetValue(vertex, out var connectedSet))
                return connectedSet;

            // Use BFS to find all vertices in the connected component
            var visited = new HashSet<TVertex>();
            var queue = new Queue<TVertex>();
            queue.Enqueue(vertex);
            visited.Add(vertex);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var edge in _graph.AdjacentEdges(current))
                {
                    var neighbor = edge.GetOtherVertex(current);
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            _vertexToConnectedSet[vertex] = visited;
            return visited;
        }

        /// <summary>
        /// Returns all connected sets in the graph.
        /// </summary>
        public List<HashSet<TVertex>> ConnectedSets()
        {
            if (_connectedSets != null)
                return _connectedSets;

            var algo = new ConnectedComponentsAlgorithm<TVertex, TEdge>(_graph);
            algo.Compute();

            // Map component IDs to sets of vertices
            var componentMap = new Dictionary<int, HashSet<TVertex>>();
            foreach (var kvp in algo.Components)
            {
                if (!componentMap.TryGetValue(kvp.Value, out var set))
                {
                    set = new HashSet<TVertex>();
                    componentMap[kvp.Value] = set;
                }
                set.Add(kvp.Key);
                _vertexToConnectedSet[kvp.Key] = set;
            }

            _connectedSets = componentMap.Values.ToList();
            return _connectedSets;
        }

        /// <summary>
        /// Tests whether two vertices are in the same connected component.
        /// </summary>
        public bool PathExists(TVertex source, TVertex target)
        {
            return ConnectedSetOf(source).Contains(target);
        }
    }

    public static class EdgeExtensions
    {
        public static TVertex GetOtherVertex<TVertex>(this IEdge<TVertex> edge, TVertex vertex)
        {
            if (EqualityComparer<TVertex>.Default.Equals(edge.Source, vertex))
                return edge.Target;
            else if (EqualityComparer<TVertex>.Default.Equals(edge.Target, vertex))
                return edge.Source;
            else
                throw new ArgumentException("Vertex is not part of the edge.");
        }
    }


