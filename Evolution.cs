using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revolver
{
    public class Evolution
    {
        public int Generation { get; set; }
        public bool Completed { get; set; }
        public double Fitness { get; set; }

        public static Random rnd = new Random(0); // Used across the solver.

        public double[] Solve(int dim, int popSize, double minX, double maxX, double mutateRate, double mutateChange, double tau, int maxGeneration, double exitError, double fitness)
        {
            // Assumes existence of an accessible Error function and a Individual class and a Random object rnd.
            Individual[] population = new Individual[popSize];
            double[] bestSolution = new double[dim]; // Best solution found by any individual.
            double bestError = double.MaxValue; // Smaller values better.

            // Population initialization.
            for (int i = 0; i < population.Length; ++i)
            {
                population[i] = new Individual(dim, minX, maxX, mutateRate, mutateChange, fitness);
                if (population[i].error < bestError)
                {
                    bestError = population[i].error;
                    Array.Copy(population[i].chromosome, bestSolution, dim);
                }
            }

            // Process.
            int gen = 0;
            bool done = false;

            while (gen < maxGeneration && done == false)
            {
                Individual[] parents = Select(2, population, tau); // Pick 2 good (not necessarily best) Individuals.
                Individual[] children = Reproduce(parents[0], parents[1], minX, maxX, mutateRate, mutateChange, ); // Create 2 children.
                Place(children[0], children[1], population); // Sort pop, replace two worst with new children.
                Immigrate(population, dim, minX, maxX, mutateRate, mutateChange); // Bring in a random Individual.

                for (int i = popSize - 3; i < popSize; ++i) // Check the 3 new Individuals.
                {
                    if (population[i].error < bestError)
                    {
                        bestError = population[i].error;
                        population[i].chromosome.CopyTo(bestSolution, 0);
                        if (bestError < exitError)
                        {
                            done = true;
                            this.Completed = true;
                        }
                    }
                }
                ++gen;

                this.Generation = gen;
            }
            return bestSolution;
        }

        public static Individual[] Select(int n, Individual[] population, double tau) // Select n 'good' Individuals.
        {
            // Tau is selection pressure = % of population to grab.
            int popSize = population.Length;
            int[] indexes = new int[popSize];
            for (int i = 0; i < indexes.Length; ++i)
                indexes[i] = i;

            for (int i = 0; i < indexes.Length; ++i) // shuffle
            {
                int r = rnd.Next(i, indexes.Length);
                int tmp = indexes[r]; indexes[r] = indexes[i]; indexes[i] = tmp;
            }

            int tournSize = (int)(tau * popSize);
            if (tournSize < n) tournSize = n;
            Individual[] candidates = new Individual[tournSize];

            for (int i = 0; i < tournSize; ++i)
                candidates[i] = population[indexes[i]];
            Array.Sort(candidates);

            Individual[] results = new Individual[n];
            for (int i = 0; i < n; ++i)
                results[i] = candidates[i];

            return results;
        }

        public static Individual[] Reproduce(Individual parent1, Individual parent2, double minGene, double maxGene, double mutateRate, double mutateChange, double error) // Crossover and mutation.
        {
            int numGenes = parent1.chromosome.Length;

            int cross = rnd.Next(0, numGenes - 1); // Crossover point. 0 means 'between 0 and 1'.

            Individual child1 = new Individual(numGenes, minGene, maxGene, mutateRate, mutateChange, error); // Random chromosome.
            Individual child2 = new Individual(numGenes, minGene, maxGene, mutateRate, mutateChange, error);

            for (int i = 0; i <= cross; ++i)
                child1.chromosome[i] = parent1.chromosome[i];
            for (int i = cross + 1; i < numGenes; ++i)
                child2.chromosome[i] = parent1.chromosome[i];
            for (int i = 0; i <= cross; ++i)
                child2.chromosome[i] = parent2.chromosome[i];
            for (int i = cross + 1; i < numGenes; ++i)
                child1.chromosome[i] = parent2.chromosome[i];

            Mutate(child1, maxGene, mutateRate, mutateChange);
            Mutate(child2, maxGene, mutateRate, mutateChange);

            Individual[] result = new Individual[2];
            result[0] = child1;
            result[1] = child2;

            return result;
        }

        public static void Mutate(Individual child, double maxGene, double mutateRate, double mutateChange)
        {
            double hi = mutateChange * maxGene;
            double lo = -hi;
            for (int i = 0; i < child.chromosome.Length; ++i)
            {
                if (rnd.NextDouble() < mutateRate)
                {
                    double delta = (hi - lo) * rnd.NextDouble() + lo;
                    child.chromosome[i] += delta;
                }
            }
        }

        public static void Place(Individual child1, Individual child2, Individual[] population)
        {
            // Place child1 and child2 into the population, replacing two worst individuals.
            int popSize = population.Length;
            Array.Sort(population);
            population[popSize - 1] = child1;
            population[popSize - 2] = child2;
            return;
        }

        public static void Immigrate(Individual[] population, int numGenes, double minGene, double maxGene, double mutateRate, double mutateChange, double error)
        {
            // Kill off third-worst Individual and replace with new Individual.
            // Assumes population is sorted.
            Individual immigrant = new Individual(numGenes, minGene, maxGene, mutateRate, mutateChange, error);
            int popSize = population.Length;
            population[popSize - 3] = immigrant; // Replace third worst individual.
        }

        // Replacing this fitness function with our GH input.
        /*
        public static double Error(double[] x)
        {
            // Absolute error for hyper-sphere function.
            double trueMin = 0.0;
            double z = 0.0;
            for (int i = 0; i < x.Length; ++i)
                z += (x[i] * x[i]);
            return Math.Abs(trueMin - z);
        }
        */
    }
}
