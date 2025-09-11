using System;
using GeneticSharp;
using MA_GA.Models;
using MA_GA.domain.geneticalgorithm.parameter;
using MA_GA.domain.geneticalgorithm.encoding;
using System.Text.RegularExpressions;
using MA_GA.domain.geneticalgorithm.crossover;
using MA_GA.domain.geneticalgorithm.selection;
using MA_GA.domain.geneticalgorithm.mutation;
using MA_GA.domain.reinsertion;


namespace MA_GA.domain.geneticalgorithm.engine;

public class GeneticAlgorithmEngineBuilder
{
    public class Builder
    {
        private Graph _graph;
        private GeneticAlgorithmParameter _geneticAlgorithmParameter;
        private MutationWeight _mutationWeight;

        private IFitness _fitness;

        public Builder Graph(Graph graph)
        {
            _graph = graph;
            return this;
        }

        public Builder GeneticAlgorithmParameter(GeneticAlgorithmParameter geneticAlgorithmParameter)
        {
            _geneticAlgorithmParameter = geneticAlgorithmParameter;
            return this;
        }

        public Builder MutationWeight(MutationWeight mutationWeight)
        {
            _mutationWeight = mutationWeight;

            return this;
        }

        public Builder Fitness(IFitness fitness)
        {
            _fitness = fitness;
            return this;
        }

        public GeneticAlgorithm CreatingEngineForMultiObjectiveProblem()
        {
            var geneticAlgorithmParameter = _geneticAlgorithmParameter;
            var population = CreatePopulation(_graph, geneticAlgorithmParameter);
            var selector = MultiObjectiveSelector();
            var crossover = CreateCrossover();
            var mutation = CreateMutatorn();

            return new GeneticAlgorithm(
                population,
                _fitness,
                selector,
                crossover,
                mutation)
            {

                Termination = new GenerationNumberTermination(geneticAlgorithmParameter.MaxGenerations),
                CrossoverProbability = geneticAlgorithmParameter.CrossoverRate,
                MutationProbability = geneticAlgorithmParameter.MutationRate
            };
        }

        // TODO: Termination need to be corrected for correct building 

        public GeneticAlgorithm CreatingEngineForWeightedSumProblem()
        {
            var geneticAlgorithmParameter = _geneticAlgorithmParameter;
            var population = CreatePopulation(_graph, geneticAlgorithmParameter);

            var selector = SingleObjectiveSelector();
            var crossover = CreateCrossover();
            var mutation = CreateMutatorn();

            return new GeneticAlgorithm(
                population,
                _fitness,
                selector,
                crossover,
                mutation)
            {
                Termination = new GenerationNumberTermination(geneticAlgorithmParameter.MaxGenerations),
                CrossoverProbability = geneticAlgorithmParameter.CrossoverRate,
                MutationProbability = geneticAlgorithmParameter.MutationRate,
                //  Reinsertion = new GaElitistReinsertion(geneticAlgorithmParameter.ElitismCount)
            };
        }

        private IMutation CreateMutatorn()
        {
            var geneticAlgorithmParameter = _geneticAlgorithmParameter;
            switch (geneticAlgorithmParameter.MutationType)
            {
                // default: return new UniformMutation();
                default: return new GraftMutator(_mutationWeight, _graph);
            }
        }

        private ICrossover CreateCrossover()
        {
            var geneticAlgorithmParameter = _geneticAlgorithmParameter;
            switch (geneticAlgorithmParameter.CrossoverType)
            {
                //  default: return new UniformCrossover();
                default: return new GroupCrossover(_graph);
            }
        }

        private ISelection MultiObjectiveSelector()
        {
            var geneticAlgorithmParameter = _geneticAlgorithmParameter;
            switch (geneticAlgorithmParameter.OffspringSelection)
            {
                default:
                    return new TournamentSelection(geneticAlgorithmParameter.TournamentSize, true);
            }
        }

        private ISelection SingleObjectiveSelector()
        {
            var geneticAlgorithmParameter = _geneticAlgorithmParameter;
            Console.WriteLine($"Offspring Selection: {geneticAlgorithmParameter.OffspringSelection}");
            switch (geneticAlgorithmParameter.OffspringSelection)
            {
                case "Roulette":
                    return new RouletteWheelSelection();
                default:
                    return new TournamentSelection(geneticAlgorithmParameter.TournamentSize, true);
            }
        }


        // Create a population with the given graph and genetic algorithm parameters
        private IPopulation CreatePopulation(Graph graph, GeneticAlgorithmParameter geneticAlgorithmParameter)
        {
            IChromosome chromosome = geneticAlgorithmParameter.UseGreedyPartition
                ? LinearLinkageEncodingInitialiser.InitializeLinearLinkageEncodingWithGreedyAlgorithm(graph)
                : Genotypeinitializer.GenerateGenotypeWithModulesForEachConnectedComponet(graph);

            /*  for checking initial chromosome      
            var initialChrome = geneticAlgorithmParameter.UseGreedyPartition
                        ? LinearLinkageEncodingInitialiser.InitializeLinearLinkageEncodingWithGreedyAlgorithm(graph)
                        : Genotypeinitializer.GenerateGenotypeWithModulesForEachConnectedComponet(graph);
            var lle = (LinearLinkageEncoding)initialChrome;

            Console.WriteLine("Initial Chromosome:");
            lle.DisplayChromosome();
            Console.WriteLine("END of Initial Chromosome");

 */

            return new Population(20, 100, chromosome);
        }
    }
}
