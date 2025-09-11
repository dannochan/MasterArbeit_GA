using System;
using GeneticSharp;
using MA_GA.domain.geneticalgorithm.encoding;
using MA_GA.domain.module;
using MA_GA.Models;

namespace MA_GA.domain.geneticalgorithm.encoding;

public class LinearLinkageEncoding : ChromosomeBase
{

    public List<Module> Modules { get; set; }
    public List<Gene> IntegerGenes { get; set; }
    public Graph BaseGraph { get; }


    /// <summary>
    ///  Initializes a new instance of the <see cref="LinearLinkageEncoding"/> class.
    ///  This constructor initializes the encoding with a specified graph and length.
    ///  The length is the number of genes in the chromosome, which refers to number of modularisable elements.
    ///  It throws an ArgumentNullException if the graph is null.
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="length"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /*
    public LinearLinkageEncoding(Graph graph, int length) : base(length)
    {
        BaseGraph = graph ?? throw new ArgumentNullException(nameof(graph), "Graph cannot be null");
        IntegerGenes = new List<Gene>(length);
        Modules = LinearLinkageEncodingInformationService.DetermineModules(this);
    }
    */

    /// <summary>
    ///  Initializes a new instance of the <see cref="LinearLinkageEncoding"/> class.
    ///  Since there is no Genotype in GeneticSharp, this constructor initializes using chromosome genes.
    ///  This constructor initializes the encoding with a specified graph and a chromosome.
    ///  The genes of the chromosome are used to initialize the encoding.
    ///  It throws an ArgumentNullException if the graph is null.
    /// </summary>
    /// <param name="chromosome"></param>
    /// <param name="graph"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public LinearLinkageEncoding(IChromosome chromosome, Graph graph) : this(graph, [.. chromosome.GetGenes()])
    {

    }

    public LinearLinkageEncoding(Graph graph, IList<Gene> genes) : base(genes.Count)
    {
        BaseGraph = graph ?? throw new ArgumentNullException(nameof(graph), "Graph cannot be null");

        IntegerGenes = genes.ToList();
        Modules = LinearLinkageEncodingInformationService.DetermineModules(this);
        SyncWithBaseGenes();

    }

    public override IChromosome CreateNew()
    {
        var CloneGenes = IntegerGenes.Select(g => new Gene(g.Value)).ToList();

        var encoding = new LinearLinkageEncoding(BaseGraph, CloneGenes);

        return encoding;

    }

    public List<Gene> GetIntegerGenes()
    {
        return IntegerGenes.ToList();
    }

    public void ReplaceIntegerGene(int index, Gene gene)
    {
        if (index < 0 || index >= IntegerGenes.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }

        IntegerGenes[index] = gene;
        ReplaceGene(index, gene);

    }

    protected override void CreateGene(int index)
    {
        if (index < 0 || index >= IntegerGenes.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }

        ReplaceGene(index, GenerateGene(index));
    }

    protected override void CreateGenes()
    {
        for (int i = 0; i < Length; i++)
        {
            ReplaceGene(i, GenerateGene(i));
        }
    }

    public void CopyFrom(LinearLinkageEncoding other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));

        for (int i = 0; i < other.IntegerGenes.Count; i++)
            ReplaceIntegerGene(i, new Gene(other.IntegerGenes[i].Value));

        Modules = other.Modules.Select(m => m.Clone()).ToList();
        Fitness = other.Fitness;
    }


    // This chromosome does not generate random genes; instead, it initializes genes based on modules from the graph.

    public override Gene GenerateGene(int geneIndex)
    {
        return new Gene(IntegerGenes[geneIndex].Value);
    }


    public int GetChromosomeLength()
    {
        return Length;
    }

    public Gene GetIntegerGene(int index)
    {
        if (index < 0 || index >= IntegerGenes.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }

        return IntegerGenes[index];
    }

    public bool IsValid()
    {
        // Implement validation logic if needed
        return LinearLinkageEncodingInformationService.IsValidLinearLinkageEncoding(this);
    }

    public Graph GetGraph()
    {
        return BaseGraph;
    }

    public List<Module> GetModules()
    {
        return Modules;
    }

    public override LinearLinkageEncoding Clone()
    {

        var clonedGenes = IntegerGenes.Select(g => new Gene(g.Value)).ToList();
        var clonedEncoding = new LinearLinkageEncoding(BaseGraph, clonedGenes)
        {
            Modules = this.Modules.Select(m => m.Clone()).ToList()
        };

        return clonedEncoding;
    }

    public Module GetModuleOfAllele(int allele)
    {
        Module module = Modules.FirstOrDefault(m => m.CheckIndexInModule(allele));
        if (module == null)
        {
            throw new NullReferenceException($"Module not found for allele {allele}. Ensure the allele is part of the encoding.");
        }
        return module;
    }

    private void SyncWithBaseGenes()
    {
        for (int i = 0; i < IntegerGenes.Count; i++)
        {
            ReplaceGene(i, new Gene(IntegerGenes[i].Value));
        }
    }

    public override string ToString()
    {

        return string.Join(", ", IntegerGenes.Select(g => g.Value));
    }
    public void DisplayChromosome()
    {
        var indices = new List<int>();
        // Display the encoding
        Console.WriteLine("Linear Linkage Encoding:");
        Console.WriteLine("Module Count: " + GetModules()?.Count);
        Console.WriteLine("indices of modules:");
        foreach (var module in GetModules())
        {
            foreach (var index in module.GetIndices())
            {
                indices.Add(index);
                Console.Write(index + " -> ");
            }

        }
        Console.WriteLine("End of Module indices"); ;

        // Display the genes
        Console.WriteLine("Genes:");
        var geneIndices = GetGenes().Select(g => g.Value).ToList();
        for (int i = 0; i < indices.Count; i++)
        {
            var geneIndex = indices[i];
            Console.Write(geneIndices[geneIndex] + " -> ");
        }

        Console.WriteLine("End of Gene Indices ");
        /*
                Console.WriteLine("IntergeGenes:");
                var intergeGeneIndices = GetIntegerGenes().Select(g => g.Value).ToList();
                for (int i = 0; i < indices.Count; i++)
                {
                    var geneIndex = indices[i];
                    Console.Write(intergeGeneIndices[geneIndex] + " -> ");
                }

                Console.WriteLine("End of Gene Indices ");
        */


    }


}
