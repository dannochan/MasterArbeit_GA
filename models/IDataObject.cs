using System;
using System.Collections.Generic;

namespace MA_GA.Models;

public interface IDataObject
{

    /// <summary>
    /// get and set the type of the information object.
    /// </summary>
    ObjectType? ObjectType { get; set; }

    /// <summary>
    /// Gets the name of the information object.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets the short name of the information object.
    /// </summary>
    string ShortName { get; set; }

    /// <summary>
    /// if the information object is an external component.
    /// </summary>
    bool? isExternalComponent { get; set; }


    List<IObjectRelation> Relations { get; set; }
    List<IDataObject> OriginObjects { get; set; }
    List<IDataObject> TargetObjects { get; set; }

    int Weight { get; set; }
    string Component { get; set; }

    string ToString();

    string ReadOriginObjects();
    string ReadTargetObjects();

    void UpdateWeight(int weight);

    int GetIndex();

}
