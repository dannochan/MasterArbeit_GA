using System;
using System.Reflection;

namespace MA_GA.models.optimizationresult;

public class ParetoOptimalSolution
{

    private List<Module> Modules { get; set; }
    private double[] FitnessValues { get; set; }
    private double[] NormalizedFitnessValues { get; set; }
    private object Fitnessvalue { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is ParetoOptimalSolution solution &&
               EqualityComparer<List<Module>>.Default.Equals(Modules, solution.Modules) &&
               EqualityComparer<double[]>.Default.Equals(FitnessValues, solution.FitnessValues) &&
               EqualityComparer<double[]>.Default.Equals(NormalizedFitnessValues, solution.NormalizedFitnessValues) &&
               EqualityComparer<object>.Default.Equals(Fitnessvalue, solution.Fitnessvalue);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Modules);
    }
}
