using System;
using System.Dynamic;

namespace MA_GA.domain.module;

/// <summary>
/// Module class represents a bundle of modularisable elements. 
/// Indices are used to keep track of the modularisable elements in the module and is sorted in ascending order.
/// The HashSet IndicesSet is used to quickly check if an index (modularisable element) exists in the module or if an element contains an index, this is only for utility performance.
/// The last indext in the LinkedList is considered the ending node of the module.
/// </summary>

public class Module
{

    private readonly LinkedList<object> Indices;

    private readonly HashSet<object> IndexSet;


    public Module()
    {
        Indices = new LinkedList<object>();
        IndexSet = new HashSet<object>();
    }

    public void AddIndex(int index)
    {
        this.IndexSet.Add(index);

        if (Indices.Count == 0)
        {
            Indices.AddFirst(index);
        }
        else if (Indices.Last() is int lastIndex && lastIndex < index)
        {
            Indices.AddLast(index);
        }
        else if (Indices.First() is int firstIndex && firstIndex > index)
        {
            Indices.AddFirst(index);
        }
        else
        {

            for (int i = 1; i < Indices.Count; i++)
            {
                if (!Indices.Select(x => (int)x).Contains(index))
                {
                    var currentIndex = (int)Indices.ElementAt(i);
                    var previousIndex = (int)Indices.ElementAt(i - 1);
                    if (currentIndex > index && previousIndex < index)
                    {
                        var node = Indices.Find(currentIndex);
                        if (node != null)
                        {
                            Indices.AddBefore(node, index);
                            return;
                        }
                        throw new InvalidOperationException($"Node with index {currentIndex} not found in the linked list.");
                    }
                }
            }

        }
    }

    public void AddIndices(HashSet<object> indices)
    {
        foreach (var index in indices)
        {
            AddIndex((int)index);
        }
    }

    public void RemoveIndex(int index)
    {
        if (!this.IndexSet.Contains(index))
        {
            throw new ArgumentException($"Index {index} does not exist in the module.");
        }

        var node = Indices.Find(index);
        if (node != null)
        {
            Indices.Remove(node);
        }

        // Remove from the HashSet
        this.IndexSet.Remove(index);
    }



    public bool CheckIndexInModule(int index)
    {
        return IndexSet.Contains(index);
    }

    public int GetAlleleOfEndingNode()
    {
        return Indices.Last?.Value is int lastIndex ? lastIndex : throw new InvalidOperationException("Module is empty, no ending node found.");
    }


    public override bool Equals(object? obj)
    {
        return obj is Module module && this.GetIndices().SequenceEqual(module.GetIndices());
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(string.Join(",", GetIndices()));
    }


    public IReadOnlyList<int> GetIndices()
    {
        return Indices.Select(index => (int)index).ToList();
    }

    public Module Clone()
    {
        var clonedModule = new Module();
        foreach (var index in this.Indices)
        {
            clonedModule.AddIndex((int)index);
        }
        return clonedModule;
    }
}
