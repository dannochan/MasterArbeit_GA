using System;
using MA_GA.Models;
using QuikGraph;

namespace MA_GA.domain;

public class UndirectedModularityMeasurer<TVertex>
{
    private readonly IUndirectedGraph<TVertex, IEdge<TVertex>> _graph;
    private readonly Dictionary<TVertex, double> _degrees = new();
    private readonly double _m;

    public UndirectedModularityMeasurer(IUndirectedGraph<TVertex, IEdge<TVertex>> graph)
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));

        var isWeighted = IsWeightedGraph();



        if (!isWeighted)
        {
            _m = _graph.Edges.Count();
            foreach (var v in _graph.Vertices)
                _degrees[v] = _graph.AdjacentDegree(v);
        }
        else
        {
            _m = _graph.Edges.Sum(e =>
            {
                var weightProp = e.GetType().GetProperty("Weight");
                var value = weightProp?.GetValue(e, null);
                return value != null ? Convert.ToDouble(value) : 1.0;
            });
            foreach (var v in _graph.Vertices)
                _degrees[v] = _graph.Edges
                    .Where(e => EqualityComparer<TVertex>.Default.Equals(e.Source, v) ||
                                EqualityComparer<TVertex>.Default.Equals(e.Target, v))
                    .Sum(e =>
                    {
                        var weightedEdge = e as dynamic;
                        double weight = (weightedEdge != null && weightedEdge.Weight != null)
                            ? Convert.ToDouble(weightedEdge.Weight)
                            : 1.0;
                        return (EqualityComparer<TVertex>.Default.Equals(e.Source, e.Target) ? 2 : 1) * weight;
                    });
        }
    }

    private bool IsWeightedGraph()
    {
        return _graph.Edges.All(e => e.GetType().GetProperty("Weight") != null);
    }

    /// <summary>
    /// Computes the modularity of the given vertex partition.
    /// </summary>
    /// <param name="partitions">A list of sets, each set is a community.</param>
    /// <returns>The modularity score.</returns>
    public double Modularity(List<HashSet<TVertex>> partitions)
    {
        if (partitions == null || partitions.Count == 0)
            throw new ArgumentException("Partitions cannot be null or empty.", nameof(partitions));

        var vertexPartition = new Dictionary<TVertex, int>();
        var weightedDegreeInPartition = new double[partitions.Count];

        for (int i = 0; i < partitions.Count; i++)
        {
            weightedDegreeInPartition[i] = 0;
            foreach (var v in partitions[i])
            {
                if (!_degrees.TryGetValue(v, out var d))
                    throw new ArgumentException("Invalid partition of vertices.");
                weightedDegreeInPartition[i] += d;
                vertexPartition[v] = i;
            }
        }

        var edgeWeightInPartition = new double[partitions.Count];
        foreach (var edge in _graph.Edges)
        {
            var v = edge.Source;
            var u = edge.Target;
            double weight = 1.0;
            var weightedEdge = edge as dynamic;
            if (weightedEdge != null && weightedEdge.Weight != null)
                weight = Convert.ToDouble(weightedEdge.Weight);

            if (!vertexPartition.TryGetValue(v, out var pv) ||
                !vertexPartition.TryGetValue(u, out var pu))
                throw new ArgumentException("Invalid partition of vertices.");

            if (pv == pu)
                edgeWeightInPartition[pv] += weight;
        }

        double mod = 0.0;
        for (int p = 0; p < partitions.Count; p++)
        {
            double expected = weightedDegreeInPartition[p] * weightedDegreeInPartition[p] / (2.0 * _m);
            mod += 2.0 * edgeWeightInPartition[p] - expected;
        }
        mod /= 2.0 * _m;
        return mod;
    }
}