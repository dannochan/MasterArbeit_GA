using System;
using System.Text;
using MA_GA.domain.module;
using MA_GA.Models;

namespace MA_GA.models.optimizationresult;

public class GeneticAlgorithmResults
{
    public Graph graph { get; set; }
    public long ModularisationExcecutionTimeInMillisecond { get; set; }

    // private int ParetoSetSize { get; set; }
    public List<Module> ModulesFromBestSolution { get; set; }
    public List<int> IntergeGeneFromBestSolution { get; set; }

    public double BestFitness { get; set; }

    public GeneticAlgorithmResults()
    {
        ModulesFromBestSolution = new List<Module>();
        IntergeGeneFromBestSolution = new List<int>();
    }

    public string DisplaySolutionUsingShortName()
    {
        Console.WriteLine($"Module Count : {ModulesFromBestSolution.Count}");
        var sb = new StringBuilder();
        sb.AppendLine("=== Modules ===");
        for (int i = 0; i < ModulesFromBestSolution.Count; i++)
        {
            Console.WriteLine($"Module {i + 1}:");
            Console.WriteLine("Node Ids : " + string.Join(", ", ModulesFromBestSolution[i].GetIndices()));
            var module = ModulesFromBestSolution[i];
            var names = module.GetIndices().Select(id => $"{id} : {graph.GetNodeNameById(id)}").ToList();

            sb.AppendLine($"Module {i + 1}: [ {string.Join(", ", names)} ]");
        }

        sb.AppendLine();

        sb.AppendLine("=== Summary ===");
        sb.AppendLine($"Modules: {ModulesFromBestSolution.Count}");
        sb.AppendLine($"Largest module size: {ModulesFromBestSolution.Max(c => c.GetIndices().Count)}");
        sb.AppendLine($"Smallest module size: {ModulesFromBestSolution.Min(c => c.GetIndices().Count)}");
        sb.AppendLine($"Total nodes: {IntergeGeneFromBestSolution.Count}");
        sb.AppendLine($"Best fitness: {BestFitness}");
        sb.AppendLine($"Execution time: {ModularisationExcecutionTimeInMillisecond} ms");

        return sb.ToString();

    }




}
