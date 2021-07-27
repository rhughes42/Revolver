# Revolver
Synchronous evolutionary solver for Rhino Compute.

## Pseudocode Sketch
´´´
Connect our input genes (parameters) to the main solver component.
Find the min, max and step size (precision) for each genome and the mutation rate for our algorithm.
Define our fitness function and connect the input to our algorithm.
Model our starting number of individual solutions (generation one) and store them globally.
Compare the best individuals from the generation, breed them and then insert these back into the population.
Run the algorithm in a loop until we satisfy our fitness criteria within a specified tolerance (success) or we hit a generation limit (failure).
Output the best performing set of genomes and their resulting fitness value.
´´´

### What is evolutionary optimization?
An evolutionary optimization algorithm is an implementation of a meta-heuristic modeled on the behavior of biological evolution.
These algorithms can be used to find approximate solutions to difficult or impossible numerical minimization problems.

### References
Demonstration code and description text in this repo is taken from: https://docs.microsoft.com/en-us/archive/msdn-magazine/2012/june/test-run-evolutionary-optimization-algorithms
