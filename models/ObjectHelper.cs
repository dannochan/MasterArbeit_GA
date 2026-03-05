using System;
using MA_GA.domain.geneticalgorithm.parameter;
using Microsoft.Extensions.Logging;

namespace MA_GA.Models;


// <summary>
// Helper class for object operations
// </summary>
public static class ObjectHelper
{
    private static int INDEX = 0;

    public static void MapDataObjects(GraphObject rawObject, Graph dataObjectCenter, ILogger logger)
    {
        int elementIndex = 0;
        // map information objects


        // map function objects

        if (rawObject.functions != null)
        {

            foreach (var item in rawObject.functions)
            {
                dataObjectCenter.AddNodeToGraph(new DataObject(
                    item.name,
                    ObjectType.FunctionObject,
                    item.nameShort,
                    item.externalComponent,
                    INDEX++ // Assigning an index to each function object
                ));
            }


        }
        else
        {
            logger.LogError("FunctionObjects is null");
        }

        if (rawObject.informationObjects != null)
        {
            foreach (var item in rawObject.informationObjects)
            {
                dataObjectCenter.AddNodeToGraph(new DataObject(
                    item.name,
                    ObjectType.InformationObject,
                    item.nameShort,
                    item.externalComponent,
                    INDEX++ // Assigning an index to each information object
                ));
            }

        }
        else
        {
            logger.LogError("InformationObjects is null");
        }

        // map relation objects
        if (rawObject.relations != null)
        {
            foreach (var item in rawObject.relations)
            {
                var relationType = convertIntToRelationTyp(item.type);
                dataObjectCenter.AddRelationToGraph(new ObjectRelation(
                    INDEX++,
                    relationType,
                    dataObjectCenter.GetNodeObjectByName(item.from),
                    dataObjectCenter.GetNodeObjectByName(item.to),
                    ConvertRelationTypeToWeight(relationType, dataObjectCenter.GetDataObjectRelationWeight())
                ));
            }
        }
        else
        {
            logger.LogError("RelationObjects is null");
        }


    }

    /// <summary>
    /// Converts a RelationType to a weight value.
    /// </summary>
    /// <param name="relationType">The type of relation to convert.</param>
    /// <returns>An integer representing the weight of the relation type. -1 indicates relation between information object</returns>
    public static double ConvertRelationTypeToWeight(RelationType relationType, DataObjectRelationWeight dataObjectRelationWeight)
    {
        return relationType switch
        {
            RelationType.Konjunktion => dataObjectRelationWeight.ConjunctionWeight,
            RelationType.Disjunktion => dataObjectRelationWeight.DisjunctionWeight,
            RelationType.ExclusiveDisjunktion => dataObjectRelationWeight.ExclusiveDisjunctionWeight,
            RelationType.Create => dataObjectRelationWeight.CreateWeight,
            RelationType.Read => dataObjectRelationWeight.ReadWeight,
            RelationType.Update => dataObjectRelationWeight.UpdateWeight,
            RelationType.Delete => dataObjectRelationWeight.DeleteWeight,
            RelationType.RelatedTo => dataObjectRelationWeight.RelatedToWeight,
            RelationType.PartOf => dataObjectRelationWeight.PartOfWeight,
            RelationType.IsA => dataObjectRelationWeight.IsAWeight,
            _ => throw new ArgumentOutOfRangeException(nameof(relationType), "Invalid relation type")
        };
    }


    private static RelationType convertIntToRelationTyp(int type)
    {
        return type switch
        {
            0 => RelationType.Konjunktion,
            1 => RelationType.Disjunktion,
            2 => RelationType.ExclusiveDisjunktion,
            3 => RelationType.Create,
            4 => RelationType.Read,
            5 => RelationType.Update,
            7 => RelationType.RelatedTo,
            8 => RelationType.PartOf,
            9 => RelationType.IsA,
            _ => throw new ArgumentOutOfRangeException(nameof(type), "Invalid relation type")
        };
    }


}
