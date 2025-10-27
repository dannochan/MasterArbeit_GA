using System;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using MA_GA.models.optimizationresult;

namespace MA_GA.util;

public class CsvGenerator
{

    private readonly CsvConfiguration _config;

    public CsvGenerator(char delimiter = ',')
    {
        _config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = delimiter.ToString(),
            // Handle special characters properly
            ShouldQuote = args => true, // Quote all fields for safety
            Encoding = Encoding.UTF8

        };
    }

    /// <summary>
    /// Streaming write for large datasets - memory efficient
    /// </summary>
    public async Task GenerateCsvStreamingAsync(
        IAsyncEnumerable<GeneticAlgorithmExecutionResult> results,
        string filePath)
    {
        await using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
        await using var csv = new CsvWriter(writer, _config);

        // Register the mapping
        csv.Context.RegisterClassMap<RuntimeResultMap>();

        // Write header
        csv.WriteHeader<GeneticAlgorithmExecutionResult>();
        await csv.NextRecordAsync();

        // Stream records one by one
        await foreach (var result in results)
        {
            csv.WriteRecord(result);
            await csv.NextRecordAsync();
        }
    }

    /// <summary>
    /// Standard write for smaller datasets
    /// </summary>
    public async Task GenerateCsvAsync(
        IEnumerable<GeneticAlgorithmExecutionResult> results,
        string filePath)
    {
        await using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
        await using var csv = new CsvWriter(writer, _config);

        csv.Context.RegisterClassMap<RuntimeResultMap>();
        await csv.WriteRecordsAsync(results);
    }

    /// <summary>
    /// Append mode for incremental writes during runtime
    /// </summary>
    public async Task AppendToCsvAsync(
        GeneticAlgorithmExecutionResult result,
        string filePath)
    {
        bool fileExists = File.Exists(filePath);

        await using var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write);
        await using var writer = new StreamWriter(stream, Encoding.UTF8);
        await using var csv = new CsvWriter(writer, _config);

        csv.Context.RegisterClassMap<RuntimeResultMap>();

        // Write header only if file is new
        if (!fileExists)
        {
            csv.WriteHeader<GeneticAlgorithmExecutionResult>();
            await csv.NextRecordAsync();
        }

        csv.WriteRecord(result);
        await csv.NextRecordAsync();
    }


    /// <summary>
    /// Custom mapping for handling arrays/lists
    /// </summary>
    public sealed class RuntimeResultMap : ClassMap<GeneticAlgorithmExecutionResult>
    {
        public RuntimeResultMap()
        {
            Map(m => m.GeneticAlgorithmResults.BestFitness).Index(0).Name("BestFitness");


        }
    }


};
