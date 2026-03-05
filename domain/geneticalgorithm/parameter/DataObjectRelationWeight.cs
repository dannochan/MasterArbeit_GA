using System;

namespace MA_GA.domain.geneticalgorithm.parameter;

public class DataObjectRelationWeight
{

    // weights for different funcition types
    private double _conjunctionWeight;
    private double _disjunctionWeight;
    private double _exclusiveDisjunctionWeight;

    // weights for different data operation types

    private double _createWeight;
    private double _readWeight;
    private double _updateWeight;
    private double _deleteWeight;

    // weights for different relationships between information objects
    private double _relatedToWeight;
    private double _partOfWeight;

    private double _isAWeight;

    public double ConjunctionWeight { get => _conjunctionWeight; }
    public double DisjunctionWeight { get => _disjunctionWeight; }
    public double ExclusiveDisjunctionWeight { get => _exclusiveDisjunctionWeight; }

    public double CreateWeight { get => _createWeight; }
    public double ReadWeight { get => _readWeight; }
    public double UpdateWeight { get => _updateWeight; }
    public double DeleteWeight { get => _deleteWeight; }
    public double RelatedToWeight { get => _relatedToWeight; }
    public double PartOfWeight { get => _partOfWeight; }
    public double IsAWeight { get => _isAWeight; }

    public DataObjectRelationWeight()
    {
        _conjunctionWeight = 1.0;
        _disjunctionWeight = 1.0;
        _exclusiveDisjunctionWeight = 1.0;

        _createWeight = 1.0;
        _readWeight = 1.0;
        _updateWeight = 1.0;
        _deleteWeight = 1.0;

        _relatedToWeight = 1.0;
        _partOfWeight = 1.0;
        _isAWeight = 1.0;
    }

    /// <summary>
    /// Constructor to initialize all weights.
    /// </summary>
    /// <param name="conjunctionWeight"></param>
    /// <param name="disjunctionWeight"></param>
    /// <param name="exclusiveDisjunctionWeight"></param>
    /// <param name="createWeight"></param>
    /// <param name="readWeight"></param>
    /// <param name="updateWeight"></param>
    /// <param name="deleteWeight"></param>
    /// <param name="relatedToWeight"></param>
    /// <param name="partOfWeight"></param>
    /// <param name="isAWeight"></param>
    public DataObjectRelationWeight(
        double conjunctionWeight,
        double disjunctionWeight,
        double exclusiveDisjunctionWeight,
        double createWeight,
        double readWeight,
        double updateWeight,
        double deleteWeight,
        double relatedToWeight,
        double partOfWeight,
        double isAWeight)
    {
        _conjunctionWeight = conjunctionWeight;
        _disjunctionWeight = disjunctionWeight;
        _exclusiveDisjunctionWeight = exclusiveDisjunctionWeight;

        _createWeight = createWeight;
        _readWeight = readWeight;
        _updateWeight = updateWeight;
        _deleteWeight = deleteWeight;

        _relatedToWeight = relatedToWeight;
        _partOfWeight = partOfWeight;
        _isAWeight = isAWeight;
    }

    public double GetMaximalFunctionWeight()
    {
        return Math.Max(_conjunctionWeight, Math.Max(_disjunctionWeight, _exclusiveDisjunctionWeight));
    }

    public double GetMaximalBPStoIOWeight()
    {
        return Math.Max(_createWeight, Math.Max(_readWeight, Math.Max(_updateWeight, _deleteWeight)));
    }

    public double GetMaximalInformationObjectRelationWeight()
    {
        return Math.Max(_relatedToWeight, Math.Max(_partOfWeight, _isAWeight));
    }

    public override String ToString()
    {
        return $"Conjunction Weight: {ConjunctionWeight}, Disjunction Weight: {DisjunctionWeight}, Exclusive Disjunction Weight: {ExclusiveDisjunctionWeight}, " +
            $"Create Weight: {CreateWeight}, Read Weight: {ReadWeight}, Update Weight: {UpdateWeight}, Delete Weight: {DeleteWeight}, " +
            $"Related To Weight: {RelatedToWeight}, Part Of Weight: {PartOfWeight}, Is A Weight: {IsAWeight}";
    }




}
