# Revolver
Synchronous evolutionary solver for Rhino Compute.

## Pseudocode
```
Connect our input genes (parameters) to the main solver component.
Find the min, max and step size (precision) for each genome and the mutation rate for our algorithm.
Define our fitness function and connect the input to our algorithm.
Model our starting number of individual solutions (generation one) and store them globally.
Compare the best individuals from the generation, breed them and then insert these back into the population.
Run the algorithm in a loop until we satisfy our fitness criteria within a specified tolerance (success) or we hit a generation limit (failure).
Output the best performing set of genomes and their resulting fitness value.
```

### Choosing a mutation value
The mutation rate value controls how many genes in the chromosome will be modified.
One heuristic for the value of the mutateRate field is to use 1.0 / numGenes, so that on average one gene in the chromosome will be mutated every time Mutate is called.

### Fitness values vs. cost functions.
With arbitrary minimization problems, the target function to be minimized is usually called a cost function. In the context of evolutionary and genetic algorithms, however, the function is usually called a fitness function. Notice the terminology is a bit awkward because lower values of fitness are better than higher values. In this example, the fitness function is completely self-contained. In many optimization problems, the fitness function requires additional input parameters, such as a matrix of data or a reference to an external data file.

### What is evolutionary optimization?
An evolutionary optimization algorithm is an implementation of a meta-heuristic modeled on the behavior of biological evolution.
These algorithms can be used to find approximate solutions to difficult or impossible numerical minimization problems.

### The Evolve method.
The Evolve method begins by initializing the best fitness and best chromosomes to the first ones in the population. The method iterates exactly maxGenerations times, using gen (generation) as a loop counter. One of several alternatives is to stop when no improvement has occurred after some number of iterations. The Select method returns two good, but not necessarily best, individuals from the population. These two parents are passed to Reproduce, which creates and returns two children. The Accept method places the two children into the population, replacing two existing individuals. The Immigrate method generates a new random individual and places it into the population. The new population is then scanned to see if any of the three new individuals in the population is a new best solution.

### References
Demonstration code and description text in this repo is taken from: https://docs.microsoft.com/en-us/archive/msdn-magazine/2012/june/test-run-evolutionary-optimization-algorithms
