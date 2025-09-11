using System;
using GeneticSharp;
using MA_GA.domain.module;

namespace MA_GA.domain.geneticalgorithm.encoding;

public sealed class LinearLinkageEncodingInformationService
{

    public static List<Module> DetermineModules(LinearLinkageEncoding encoding)
    {
        var chromosomes = encoding.GetIntegerGenes()
            .Select(g => (int)g.Value)
            .ToList();

        var visitedNodes = new bool[chromosomes.Count];

        var modules = new List<Module>();
        for (int i = 0; i < chromosomes.Count; i++)
        {
            if (!visitedNodes[i])
            {
                int indext = chromosomes[i];

                bool isAlleleAlreadyInModule = modules.Any(module => module.CheckIndexInModule(indext));

                if (isAlleleAlreadyInModule)
                {
                    var module = modules.Where(m => m.CheckIndexInModule(indext)).First();
                    BuildModuleDFS(module, chromosomes, visitedNodes, i);
                }
                else
                {
                    var newModule = new Module();
                    BuildModuleDFS(newModule, chromosomes, visitedNodes, i);
                    modules.Add(newModule);
                }
            }
        }

        return modules;

    }

    /// <summary>
    /// Builds a module using Depth First Search (DFS) algorithm.
    /// It recursively traverses the chromosomes, marking nodes as visited and adding them to the module.
    /// </summary>
    /// <param name="newModule"></param>
    /// <param name="chromosomes"></param>
    /// <param name="visitedNodes"></param>
    /// <param name="index"></param>
    private static void BuildModuleDFS(Module newModule, List<int> chromosomes, bool[] visitedNodes, int index)
    {
        // Console.WriteLine($"Visiting index: {index}, value: {chromosomes[index]}");
        visitedNodes[index] = true;

        var nextNode = chromosomes[index];



        if (nextNode != index && !visitedNodes[nextNode])
        {
            BuildModuleDFS(newModule, chromosomes, visitedNodes, nextNode);
        }
        newModule.AddIndex(index);
    }

    public static int GetNumberOfNonIsolatedModules(LinearLinkageEncoding encoding)
    {
        return encoding.GetModules()
            .Count(module => !ModuleInformationService.IsIsolated(module, encoding.GetGraph()));
    }

    public static bool IsValidChromose(IChromosome chromosome)
    {
        if (chromosome is not LinearLinkageEncoding encoding)
        {
            throw new ArgumentException("The provided chromosome is not a LinearLinkageEncoding.");
        }

        return IsValidLinearLinkageEncoding(encoding);
    }

    public static bool IsValidLinearLinkageEncoding(LinearLinkageEncoding linearLinkageEncoding)
    {
        if (!IsValidAlleleValues(linearLinkageEncoding))
        {
            return false;
        }

        if (!IsAllElementsInModuleConnected(linearLinkageEncoding))
        {
            return false;
        }

        if (IsOneModuleConsistOfOneEdge(linearLinkageEncoding))
        {
            return false;
        }

        if (IsMonolith(linearLinkageEncoding))
        {
            return false;
        }

        return true;
    }

    public static bool IsValidAlleleValues(LinearLinkageEncoding encoding)
    {
        var genes = encoding.GetIntegerGenes();

        var alleleDictionary = new Dictionary<int, int>();

        foreach (var gene in genes)
        {
            var currentAllele = (int)gene.Value;
            if (alleleDictionary.ContainsKey(currentAllele))
            {
                alleleDictionary[currentAllele]++;
                if (alleleDictionary[currentAllele] > 2)
                {
                 //   Console.WriteLine($"Allele {currentAllele} appears more than twice in the encoding.");
                    return false; // Allele value appears more than onc
                }



            }
            else
            {
                alleleDictionary[currentAllele] = 1;
            }

        }
        return true;
    }

    public static bool IsAllElementsInModuleConnected(LinearLinkageEncoding encoding)
    {
        var graph = encoding.GetGraph();

        var modulesToCheck = encoding.GetModules().Where(module => module.GetIndices().Count > 1).ToList();
        return modulesToCheck.All(module =>
            ModuleInformationService.IsModuleConnected(module, graph));
    }

    public static bool IsOneModuleConsistOfOneEdge(LinearLinkageEncoding encoding)
    {
        var graph = encoding.GetGraph();
        return encoding.GetModules().Any(module => !ModuleInformationService.IsIsolated(module, graph) && module.GetIndices().Count <= 1);
    }

    public static bool IsMonolith(LinearLinkageEncoding encoding)
    {
        return GetNumberOfNonIsolatedModules(encoding) == 1; ;
    }


}
