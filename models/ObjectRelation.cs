using System;

namespace MA_GA.Models;

public class ObjectRelation :  IObjectRelation
{
    public int EdgeNumber { get; set; }
    public RelationType RelationType { get; set; }

    public DataObject SourceObject { get; set; }

    public DataObject TargetObject { get; set; }

    public DataObject Source { get; set; }

    public DataObject Target { get; set; }

    public int Weight { get; set; }

    public string Component { get; set; }

    public ObjectRelation(int edgeNumber, RelationType relationType,
    DataObject sourceObject,
    DataObject targetObject)
    {
        EdgeNumber = edgeNumber;
        RelationType = relationType;
        SourceObject = sourceObject;
        TargetObject = targetObject;
        Source = sourceObject;
        Target = targetObject;
        Weight = ObjectHelper.ConvertRelationTypeToWeight(relationType);
    }

/*
    public override int GetIndex()
    {
        return EdgeNumber;
    }
    */

    public override bool Equals(object? obj)
    {
        return obj is ObjectRelation relation &&
               RelationType == relation.RelationType &&
               EqualityComparer<IDataObject>.Default.Equals(SourceObject, relation.SourceObject) &&
               EqualityComparer<IDataObject>.Default.Equals(TargetObject, relation.TargetObject) &&
               Weight == relation.Weight;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(RelationType, SourceObject, TargetObject, Weight);
    }
}
