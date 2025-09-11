using System;
using GeneticSharp;
using MA_GA.domain.geneticalgorithm.encoding;
using MA_GA.domain.module;
using MA_GA.Models;

namespace MA_GA.domain.geneticalgorithm.crossover;

public class GroupCrossover : CrossoverBase
{
    private readonly Graph baseGraph;

    public GroupCrossover(Graph graph) : base(2, 2)
    {
        baseGraph = graph ?? throw new ArgumentNullException(nameof(graph), "Graph cannot be null");
    }
    protected override IList<IChromosome> PerformCross(IList<IChromosome> parents)
    {
        var parent1 = new LinearLinkageEncoding(parents[0], baseGraph);
        var parent2 = new LinearLinkageEncoding(parents[1], baseGraph);

        LinearLinkageEncoding offspring1 = parent1.Clone();
        LinearLinkageEncoding offspring2 = parent2.Clone();

        // If parents invalid, repair
        if (!parent1.IsValid())
            offspring1 = LinearLinkageEncodingOperator.FixLinearLinkageEncoding(offspring1);
        if (!parent2.IsValid())
            offspring2 = LinearLinkageEncodingOperator.FixLinearLinkageEncoding(offspring2);

        // Determine new modules for offspring
        var newModules1 = DetermineNewModulesForOffspring(parent1, parent2);
        var newModules2 = newModules1.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Clone());

        for (int i = 0; i < parent1.IntegerGenes.Count; i++)
        {
            if (IsEndingNode(parent1.GetIntegerGene(i), i) && IsEndingNode(parent2.GetIntegerGene(i), i))
                continue;

            AssignGeneToNewModule(parent1, newModules1, i);
            AssignGeneToNewModule(parent2, newModules2, i);
        }

        UpdateParentToOffspring(offspring1, newModules1.Values.ToList());
        UpdateParentToOffspring(offspring2, newModules2.Values.ToList());

        // Recompute modules and ensure validity
        offspring1.Modules = LinearLinkageEncodingInformationService.DetermineModules(offspring1);
        offspring2.Modules = LinearLinkageEncodingInformationService.DetermineModules(offspring2);

        if (!offspring1.IsValid())
            offspring1 = LinearLinkageEncodingOperator.FixLinearLinkageEncoding(offspring1);
        if (!offspring2.IsValid())
            offspring2 = LinearLinkageEncodingOperator.FixLinearLinkageEncoding(offspring2);

        return new List<IChromosome> { offspring1, offspring2 };
    }


    private void UpdateParentToOffspring(LinearLinkageEncoding offspring, List<Module> modules)
    {
        foreach (var module in modules)
        {
            var indicesOfModule = module.GetIndices();
            for (int i = 0; i < indicesOfModule.Count - 1; i++)
            {
                //   offspring.ReplaceGene(indicesOfModule[i], new Gene(indicesOfModule[i + 1]));
                offspring.ReplaceIntegerGene(indicesOfModule[i], new Gene(indicesOfModule[i + 1]));

            }
            var lastIndex = indicesOfModule[indicesOfModule.Count - 1];

            // offspring.ReplaceGene(lastIndex, new Gene(lastIndex));
            offspring.ReplaceIntegerGene(lastIndex, new Gene(lastIndex));
        }


    }

    private void AssignGeneToNewModule(LinearLinkageEncoding encodingParent, IDictionary<int, Module> newModuleForOffspring, int index)
    {
        var geneInParent = encodingParent.GetIntegerGene(index);
        var moduleOfGene = encodingParent.GetModuleOfAllele((int)geneInParent.Value);
        var alleleEndingNodeInParent = moduleOfGene.GetAlleleOfEndingNode();

        if (newModuleForOffspring.ContainsKey(alleleEndingNodeInParent))
        {
            newModuleForOffspring[alleleEndingNodeInParent].AddIndex(index);
            return;
        }

        var modularisableElement = baseGraph.GetModularisableElementByIndex(index);
        var incidentModularisableElements = baseGraph.GetIncidentModularisableElements(modularisableElement);

        var newModuleForOffspringList = newModuleForOffspring.Values.ToList();

        var moduleOfIncidentModularisableElements = newModuleForOffspringList

        .Where(m => incidentModularisableElements.Any(e => m.CheckIndexInModule(e.GetIndex())))

        .ToList();


        if (moduleOfIncidentModularisableElements.Count > 0)

        {

            var random = RandomizationProvider.Current;

            var randomModule = moduleOfIncidentModularisableElements[random.GetInt(0, moduleOfIncidentModularisableElements.Count)];

            randomModule.AddIndex(index);

        }
        else
        {
            //     if (newModuleForOffspring.Keys.Contains(index)) return;
            // If no potential module found, create a new one
            var newModule = new Module();
            newModule.AddIndex(index);
            newModuleForOffspring[index] = newModule;
        }

    }

    private bool IsEndingNode(Gene gene, int i) => gene.Value is int value && value == i;

    /// <summary>
    /// Determines new modules for the offspring based on the parents' encodings.
    /// new modules should contains end nodes as follow:
    /// in the first parent and the second parent the integer is an ending node
    /// in the first parent integer gene is  an endig node with 50% probability
    /// in the second parent integer gene is an ending node with 50% probability
    /// isolated vertices are always ending nodes
    /// </summary>
    /// <param name="encodingParent1"></param>
    /// <param name="encodingParent2"></param>
    /// <returns>Dict, where index of the ending node is the key and the corresponding module is the value</returns>

    private IDictionary<int, Module> DetermineNewModulesForOffspring(LinearLinkageEncoding parent1, LinearLinkageEncoding parent2)
    {
        var rnd = RandomizationProvider.Current;
        var newModules = new Dictionary<int, Module>();

        for (int i = 0; i < parent1.IntegerGenes.Count; i++)
        {
            var element = baseGraph.GetModularisableElementByIndex(i);
            bool isIsolated = element is DataObject data && baseGraph.IsIsolatedVertex(data);

            if (isIsolated ||
                (IsEndingNode(parent1.GetIntegerGene(i), i) && IsEndingNode(parent2.GetIntegerGene(i), i)) ||
                (IsEndingNode(parent1.GetIntegerGene(i), i) && rnd.GetDouble() < 0.5d) ||
                (IsEndingNode(parent2.GetIntegerGene(i), i) && rnd.GetDouble() < 0.5d))
            {
                var module = new Module();
                module.AddIndex(i);
                newModules.Add(i, module);
            }
        }

        return newModules;
    }
}
