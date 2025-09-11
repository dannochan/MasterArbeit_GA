using System;

namespace MA_GA.domain.geneticalgorithm.parameter;

public class MutationWeight
{
    private float _splitModulesWeight;
    private float _combineModulesWeight;
    private float _moveGeneToDifferentModuleWeight;

    public float SplitModulesWeight { get => _splitModulesWeight; set => _splitModulesWeight = value; }
    public float CombineModulesWeight { get => _combineModulesWeight; set => _combineModulesWeight = value; }
    public float MoveGeneToDifferentModuleWeight { get => _moveGeneToDifferentModuleWeight; set => _moveGeneToDifferentModuleWeight = value; }

    public MutationWeight()
    {
        SplitModulesWeight = 1.0f;
        CombineModulesWeight = 1.0f;
        MoveGeneToDifferentModuleWeight = 1.0f;
    }

    public MutationWeight(float splitModulesWeight, float combineModulesWeight, float moveGeneToDifferentModuleWeight)
    {
        SplitModulesWeight = splitModulesWeight;
        CombineModulesWeight = combineModulesWeight;
        MoveGeneToDifferentModuleWeight = moveGeneToDifferentModuleWeight;
    }

    public override String ToString()
    {
        return $"Split Modules Weight: {SplitModulesWeight}, Combine Modules Weight: {CombineModulesWeight}, Move Gene to Different Module Weight: {MoveGeneToDifferentModuleWeight}";
    }

}
