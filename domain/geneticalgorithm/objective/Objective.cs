using System;
using System.Reflection;
using GeneticSharp;
using MA_GA.domain.geneticalgorithm.encoding;
using MA_GA.domain.module;
using MA_GA.models.enums;
using MA_GA.Models;
using Microsoft.Extensions.Logging;
using Module = MA_GA.domain.module.Module;

namespace MA_GA.domain.geneticalgorithm.objective;

public abstract class Objective
{

    protected double weight;
    protected Graph graph;
    protected int numberOfElementsPerModule;

    public abstract double CalculateValue(List<Module> modules);

    public abstract string GetObjectiveName();
    public abstract OptimizationType GetOptimizationType();
    public abstract ObjectiveType GetObjectiveType();

    public double GetWeight() { return weight; }

    public void SetWeight(double weight)
    {
        if (weight < 0)
        {
            throw new ArgumentException("Weight cannot be negative.");
        }
        this.weight = weight;
    }

    public bool IsNumberOfElementsNeeded()
    {
        return false;
    }

    public void SetGraph(Graph graph)
    {
        this.graph = graph;
    }

    public void SetNumberOfElementsPerModule(int numberOFElementsPerModule)
    {
        numberOfElementsPerModule = numberOFElementsPerModule;
    }

    public void GetPrepare()
    {

    }

    public override string ToString()
    {
        return $"{GetObjectiveName()} (Weight: {weight})";
    }

    /// <summary>
    /// Evaluates the objective for a given chromosome.
    /// This method should be overridden in derived classes to provide specific evaluation logic.
    /// chromosome will be cast to LinearLinkageEncoding.
    /// If the chromosome is not of type LinearLinkageEncoding, an ArgumentException will be thrown.
    /// Modules will be extracted from the chromosome and passed to CalculateValue.
    /// </summary>
    /// <param name="chromosome"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>

    public double Evaluate(IChromosome chromosome)
    {
        var lle = new LinearLinkageEncoding(chromosome, graph);
        return CalculateValue(lle.GetModules());
    }

}
