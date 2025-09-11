using System;

namespace MA_GA.models.enums;

public enum OptimizationType
{
    Minimum,
    Maximum

}


public static class OptimizationExtenstion
{
    public static int Compare<T>(OptimizationType optimizationType, T value1, T value2) where T : IComparable<T>
    {
        return optimizationType switch
        {
            OptimizationType.Minimum => value1.CompareTo(value2),
            OptimizationType.Maximum => value2.CompareTo(value1),
            _ => throw new ArgumentOutOfRangeException(nameof(optimizationType), optimizationType, null)
        };
    }

}
