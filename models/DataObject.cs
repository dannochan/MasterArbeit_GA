using System;

namespace MA_GA.Models;

public class DataObject : ModularisableElement, IDataObject
{

    private int VertexNumber { get; set; }
    public ObjectType? ObjectType { get; set; }

    public string Name { get; set; }

    public string ShortName { get; set; }

    public bool? isExternalComponent { get; set; }

    public int Weight { get; set; } // Default weight, can be adjusted based on relation type
    public string Component { get; set; } // Component name, if applicable

    public List<IObjectRelation> Relations { get; set; }
    public List<IDataObject> OriginObjects { get; set; }
    public List<IDataObject> TargetObjects { get; set; }


    /// <summary>
    /// Constructor for DataObject. especially used for Information Objects.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="objectType"></param>
    /// <param name="shortName"></param>
    /// <param name="isExternalComponent"></param>
    public DataObject(string name, ObjectType objectType, string shortName, bool? isExternalComponent)
    {
        this.ObjectType = objectType;
        this.Name = name;
        this.ShortName = shortName;
        this.isExternalComponent = isExternalComponent;
        this.OriginObjects = new List<IDataObject>();
        this.TargetObjects = new List<IDataObject>();
        Weight = 0;
        
    }

    /// <summary>
    /// constructor for DataObject. especially used for functions Objects with vertex number.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="objectType"></param>
    /// <param name="shortName"></param>
    /// <param name="isExternalComponent"></param>
    /// <param name="vertexNumber"></param>
    public DataObject(string name, ObjectType objectType, string shortName, bool? isExternalComponent, int vertexNumber)
    {
        this.ObjectType = objectType;
        this.Name = name;
        this.ShortName = shortName;
        this.isExternalComponent = isExternalComponent;
        this.OriginObjects = new List<IDataObject>();
        this.TargetObjects = new List<IDataObject>();
        Weight = 0;
        this.VertexNumber = vertexNumber;
    }



    public string getName()
    {
        return Name;
    }
    public string getShortName()
    {
        return ShortName;
    }

    public void addRelation(IObjectRelation relation)
    {
        Relations.Add(relation);
    }
    public void removeRelation(IObjectRelation relation)
    {
        Relations.Remove(relation);
    }

    public void getAllRelations()
    {
        Console.WriteLine("Relations of " + Name + ":");
        foreach (var relation in Relations)
        {
            Console.WriteLine(relation.TargetObject + " - " + relation.RelationType);
        }
    }

    public void AddOriginObject(IDataObject originObject)
    {
        if (this.CheckOriginObject(originObject))
        {
            Console.WriteLine($"Origin object {originObject.Name} already exists in {Name}.");
            return;
        }

        OriginObjects.Add(originObject);
    }
    public void AddTargetObject(IDataObject targetObject)
    {

        if (this.CheckTargetObject(targetObject))
        {
            Console.WriteLine($"Target object {targetObject.Name} already exists in {Name}.");
            return;
        }
        TargetObjects.Add(targetObject);
    }

    public bool CheckOriginObject(IDataObject originObject)
    {

        return originObject != null && OriginObjects.Contains(originObject);
    }

    public bool CheckTargetObject(IDataObject targetObject)
    {
        return targetObject != null && TargetObjects.Contains(targetObject);
    }

    public void RemoveOriginObject(IDataObject originObject)
    {
        OriginObjects.Remove(originObject);
    }

    public void RemoveTargetObject(IDataObject targetObject)
    {
        TargetObjects.Remove(targetObject);
    }

    public List<IDataObject> GetOriginObjects()
    {
        return OriginObjects;
    }

    public List<IDataObject> GetTargetObjects()
    {
        return TargetObjects;
    }
    public override string ToString()
    {
        return $"DataObject: {Name}, Type: {ObjectType}, ShortName: {ShortName}, External: {isExternalComponent}";
    }

    public string ReadOriginObjects()
    {
        if (OriginObjects.Count == 0)
        {
            return "No origin objects found.";
        }
        string result = "Origin Objects: ";
        foreach (var originObject in OriginObjects)
        {
            result += originObject.Name + ", ";
        }
        return result.TrimEnd(',', ' ');
    }

    public string ReadTargetObjects()
    {
        if (TargetObjects.Count == 0)
        {
            return "No target objects found.";
        }
        string result = "Target Objects: ";
        foreach (var targetObject in TargetObjects)
        {
            result += targetObject.Name + ", ";
        }
        return result.TrimEnd(',', ' ');
    }

    public override int GetIndex()
    {
        return VertexNumber;
    }

    public override bool Equals(object? obj)
    {
        return obj is DataObject @object &&
               ObjectType == @object.ObjectType &&
               Name == @object.Name &&
               ShortName == @object.ShortName &&
               isExternalComponent == @object.isExternalComponent &&
               EqualityComparer<List<IObjectRelation>>.Default.Equals(Relations, @object.Relations) &&
               EqualityComparer<List<IDataObject>>.Default.Equals(OriginObjects, @object.OriginObjects) &&
               EqualityComparer<List<IDataObject>>.Default.Equals(TargetObjects, @object.TargetObjects);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ObjectType, Name, ShortName, isExternalComponent, Relations, OriginObjects, TargetObjects);
    }

    public void UpdateWeight(int newWeight)
    {

        Weight += newWeight;
    }
}
