using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Revolver
{
    public class Solver : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Solver class.
        /// </summary>
        public Solver()
          : base("Solver", "Revolver",
              "Simplified synchronous evolutionary solver.",
              "Revolver", "Core")
        {
        }

        List<string> log = new List<string>();

        public List<IGH_Param> Genomes { get; set; }
        public int GenomeCount { get; set; }
        public int Generation { get; set; }
        public bool Completed { get; set; }
        public double Fitness { get; set; } // The value of our fitness function.

        public static Random rnd = new Random(0); // Used across the solver.

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Genomes", "Genes", "The input genomes to control.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Population", "Pop", "Initial population size.", GH_ParamAccess.item, 50);
            pManager.AddNumberParameter("MinGene", "minG", "Minimum gene.", GH_ParamAccess.item, -10);
            pManager.AddNumberParameter("MaxGene", "maxG", "Maximum gene.", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("MutateRate", "mRate", "Likelihood that a gene is changed.", GH_ParamAccess.item, 0.20);
            pManager.AddNumberParameter("MutateChange", "mChange", "Controls magnitude of mutation change.", GH_ParamAccess.item, 0.01);
            pManager.AddNumberParameter("Tau", "Tau", "Selection pressure (percent pop selected for tournament selection).", GH_ParamAccess.item, 0.40);
            pManager.AddIntegerParameter("MaxGen", "maxGen", "Loop counter.", GH_ParamAccess.item, 5000);
            pManager.AddNumberParameter("ExitError", "ExitErr", "Early exit if meet this Error or less.", GH_ParamAccess.item, 0.00001);
            pManager.AddNumberParameter("Fitness", "Fitness", "Fitness function result.", GH_ParamAccess.item, 100.00);
            pManager.AddBooleanParameter("Run", "Run", "Start the solbing process.", GH_ParamAccess.item, false);

            // Set everything after population as optional.
            for (int i = 3; i < pManager.ParamCount; i++)
            {
                pManager[i].Optional = true;
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Log", "Log", "Output debug log.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            log.Clear();

            log.Add("\nBegin Evolutionary Optimization demo\n");
            log.Add("Goal is to minimize f(x0,x1,x2,x3,x4,x5) = x0^2 + x1^2 + x2^2 + x3^2 + x4^2 + x5^2");
            log.Add("Known solution is 0.0 at x0 = x1 = x2 = x3 = x4 = x5 = 0.000000");

            List<object> genes = new List<object>(); // Genomes to control during the solve instance.
            int popSize = 50;
            double minX = -10.0; // MinGene
            double maxX = 10.0; // MaxGene
            double mutateRate = 0.20; // Likelihood that a gene is changed.
            double mutateChange = 0.01; // Controls magnitude of mutation change.
            double tau = 0.40; // Selection pressure (percent pop selected for tournament selection).
            int maxGeneration = 5000; // Loop counter.
            double exitError = 0.00001; // Early exit if meet this Error or less.
            double fitness = 100.00; // Our fitness function result.
            bool run = false;

            if (!DA.GetDataList(0, genes)) return;
            if (!DA.GetData(1, ref popSize)) return;
            if (!DA.GetData(2, ref minX)) return;
            if (!DA.GetData(3, ref maxX)) return;
            if (!DA.GetData(4, ref mutateRate)) return;
            if (!DA.GetData(5, ref mutateChange)) return;
            if (!DA.GetData(6, ref tau)) return;
            if (!DA.GetData(7, ref maxGeneration)) return;
            if (!DA.GetData(8, ref exitError)) return;
            if (!DA.GetData(9, ref fitness)) return; // TODO: Get the component that is holding this fitness criteria (we need to grab it each time inside the loop.
            if (!DA.GetData(10, ref run)) return;

            List<IGH_Param> genomes = this.Params.Input[0].Sources[0] as List<IGH_Param>; // Get the connected genomes for our component.
            this.Genomes = genomes;
            this.GenomeCount = genomes.Count;

            if (run)
            {
                this.Message = "Starting...";

                log.Add("\nSetting problem dimension = numGenes = " + this.GenomeCount);
                log.Add("Setting population size = " + popSize);
                log.Add("Setting minX = minGene = " + minX.ToString("F1"));
                log.Add("Setting maxX = maxGene = " + maxX.ToString("F1"));
                log.Add("Setting mutation rate = " + mutateRate.ToString("F2"));
                log.Add("Setting mutation change factor = " + mutateChange.ToString("F2"));
                log.Add("Setting selection pressure (tau) = " + tau.ToString("F2"));
                log.Add("Setting max generations = " + maxGeneration);
                log.Add("Setting early-exit Error value = " + exitError.ToString("F6"));

                this.Message = "Running...";

                log.Add("\nBeginning evolutionary optimization loop");

                // Assumes existence of an accessible Error function and a Individual class and a Random object rnd.
                Individual[] population = new Individual[popSize];
                double[] bestSolution = new double[this.GenomeCount]; // Best solution found by any individual.
                double bestError = double.MaxValue; // Smaller values better.

                // Population initialization.
                for (int i = 0; i < population.Length; ++i)
                {
                    population[i] = new Individual(this.Genomes, minX, maxX, mutateRate, mutateChange, fitness);
                    if (population[i].Error < bestError)
                    {
                        bestError = population[i].Error;
                        Array.Copy(population[i].Chromosome, bestSolution, this.GenomeCount);
                    }
                }

                // Process.
                int gen = 0;
                bool done = false;

                while (gen < maxGeneration && done == false)
                {
                    Individual[] parents = Select(2, population, tau); // Pick 2 good (not necessarily best) Individuals.
                    Individual[] children = Reproduce(parents[0], parents[1], minX, maxX, mutateRate, mutateChange, fitness); // Create 2 children.

                    Place(children[0], children[1], population); // Sort pop, replace two worst with new children.
                    Immigrate(population, this.GenomeCount, minX, maxX, mutateRate, mutateChange, fitness); // Bring in a random Individual.

                    for (int i = popSize - 3; i < popSize; ++i) // Check the 3 new Individuals.
                    {
                        if (population[i].Error < bestError)
                        {
                            bestError = population[i].Error;
                            population[i].Chromosome.CopyTo(bestSolution, 0);
                            if (bestError < exitError)
                            {
                                done = true;
                                this.Completed = true;
                            }
                        }
                    }
                    ++gen;

                    this.Generation = gen;
                    this.Message = gen.ToString();
                }
                double[] best = bestSolution;

                log.Add("\nBest solution found:\n");
                for (int i = 0; i < bestSolution.Length; ++i)
                    log.Add(bestSolution[i].ToString("F6") + " ");

                double Error = fitness;
                log.Add("\n\n(Absolute) Error value at best solution = " + Error.ToString("F6"));

                log.Add("\nEnd Evolutionary Optimization demo\n");

                this.Message = "Complete!";
            }

            // Out 
            DA.SetDataList(0, log);
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

        public Individual[] Reproduce(Individual parent1, Individual parent2, double minGene, double maxGene, double mutateRate, double mutateChange, double Error) // Crossover and mutation.
        {
            int numGenes = parent1.Chromosome.Length;

            int cross = rnd.Next(0, numGenes - 1); // Crossover point. 0 means 'between 0 and 1'.

            Individual child1 = new Individual(this.Genomes, minGene, maxGene, mutateRate, mutateChange, Error); // Random Chromosome.
            Individual child2 = new Individual(this.Genomes, minGene, maxGene, mutateRate, mutateChange, Error);

            for (int i = 0; i <= cross; ++i)
                child1.Chromosome[i] = parent1.Chromosome[i];
            for (int i = cross + 1; i < numGenes; ++i)
                child2.Chromosome[i] = parent1.Chromosome[i];
            for (int i = 0; i <= cross; ++i)
                child2.Chromosome[i] = parent2.Chromosome[i];
            for (int i = cross + 1; i < numGenes; ++i)
                child1.Chromosome[i] = parent2.Chromosome[i];

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
            for (int i = 0; i < child.Chromosome.Length; ++i)
            {
                if (rnd.NextDouble() < mutateRate)
                {
                    double delta = (hi - lo) * rnd.NextDouble() + lo;
                    child.Chromosome[i] += delta;
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

        public void Immigrate(Individual[] population, int numGenes, double minGene, double maxGene, double mutateRate, double mutateChange, double Error)
        {
            // Kill off third-worst Individual and replace with new Individual.
            // Assumes population is sorted.
            Individual immigrant = new Individual(this.Genomes, minGene, maxGene, mutateRate, mutateChange, Error); // TODO: Create a new random immigrant indiviudal.
            int popSize = population.Length;
            population[popSize - 3] = immigrant; // Replace third worst individual.
        }

        // Replacing this fitness function with our GH input.
        /*
        public static double Error(double[] x)
        {
            // Absolute Error for hyper-sphere function.
            double trueMin = 0.0;
            double z = 0.0;
            for (int i = 0; i < x.Length; ++i)
                z += (x[i] * x[i]);
            return Math.Abs(trueMin - z);
        }
        */

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0754134c-81bf-4149-a4c8-e73b1e3e728b"); }
        }
    }

    public class Individual : IComparable<Individual>
    {
        public double[] Chromosome; // Represents a solution.
        public double Error; // Smaller values are better for minimization.

        private int NumGenes; // Problem dimension.
        private double MutateRate; // Used during reproduction by Mutate.
        private double MutateChange; // Used during reproduction.

        static Random rnd = new Random(0); // Used by ctor.

        public Individual(List<IGH_Param> genomes, double mutateRate, double mutateChange, double Error)
        {
            this.NumGenes = genomes.Count;
            this.MutateRate = mutateRate;
            this.MutateChange = mutateChange;

            this.Chromosome = new double[genomes.Count];

            for (int i = 0; i < this.Chromosome.Length; ++i)
                this.Chromosome[i] = (maxGene - minGene) * rnd.NextDouble() + minGene; // TODO: Get the min and max of each slider, and adjust them programmatically and randomly.

            //this.Error = Evolution.Error(this.Chromosome);
            this.Error = Error;
        }

        public int CompareTo(Individual other) // From smallest Error (better) to largest.
        {
            if (this.Error < other.Error) return -1;
            else if (this.Error > other.Error) return 1;
            else return 0;
        }
    }
}