using System;
using GeneticSharp;

namespace MA_GA.domain.geneticalgorithm.termination;

public class ConvergenceTermination : ITermination
{
    private readonly int _patience;       // Number of generations to wait
    private readonly double _threshold;   // Minimum improvement to consider as progress
    private double _lastBestFitness;
    private int _noImprovementCount;
    private int _maxGenCount; 

    public ConvergenceTermination(int patience, double threshold, int maxGens)
    {
        _patience = patience;
        _threshold = threshold;
        _lastBestFitness = double.MinValue;
        _noImprovementCount = 0;
        _maxGenCount = maxGens;
    }
    public bool HasReached(IGeneticAlgorithm geneticAlgorithm)
    {
        var currentBest = geneticAlgorithm.BestChromosome.Fitness.Value;

        if (_lastBestFitness < 0) _lastBestFitness = currentBest;

        if (Math.Abs(currentBest - _lastBestFitness) < _threshold)
        {
            _noImprovementCount++;
        }
        else
        {
            _noImprovementCount = 0;
        }

        _lastBestFitness = currentBest;

        return _noImprovementCount >= _patience || geneticAlgorithm.GenerationsNumber >= _maxGenCount;
    }
}
