using System;
using QuikGraph;

namespace MA_GA.Models;

/// <summary>
/// This interface defines the existing relations between objects in the graph.
/// It includes properties for the type of relation, the source object, and the target object.
/// </summary>

public interface IObjectRelation : IEdge<DataObject>
{

    // Edge number for the relation
    int EdgeNumber { get; set; }
    // Type of the relation
    RelationType RelationType { get; set; }

    // Source object
    DataObject SourceObject { get; set; }


    // target object

    DataObject TargetObject { get; set; }

    // weight of the relation
    int Weight { get; set; }
/*
    int GetIndex();

*/

    string Component { get; set; }


}
