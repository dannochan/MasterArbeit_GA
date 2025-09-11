using System;
using MA_GA.Models.MappingModels;

namespace MA_GA.Models;

/// <summary>
/// GraphObject class represents the structure of the graph object as defined in the JSON file.
/// </summary>

public class GraphObject
{
    public List<NodeObject>? externalComponents { get; set; }
    public List<NodeObject> informationObjects { get; set; }
    public List<NodeObject> functions { get; set; }
    public List<NodeObject>? actors { get; set; }
    public List<EdgeObject> relations { get; set; }




    public List<NodeObject> getInfoObjectList()
    {
        return informationObjects;
    }


}
