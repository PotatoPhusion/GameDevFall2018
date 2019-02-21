using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NeuralNetwork : IComparable<NeuralNetwork>{

    public int[] layers;
    public float[][] neurons; // neuron matrix
    public float[][][] weights; // weights matrix
    private float fitness;

    /// <summary>
    /// Creates a new neural network.
    /// </summary>
    /// <param name="layers">Defines the number of neurons in each layer. Including input and output layers.</param>
	public NeuralNetwork(int[] layers) {

        this.layers = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            if (i < layers.Length - 2)
                this.layers[i] = layers[i] + 1; // Add the bias neuron
            else
                this.layers[i] = layers[i];
        }

        InitNeurons();
        InitWeights();
    }

    /// <summary>
    /// Creates a deep copy of a neural network.
    /// </summary>
    /// <param name="copyNetwork">The network to be copied</param>
    public NeuralNetwork(NeuralNetwork copyNetwork) {
        this.layers = new int[copyNetwork.layers.Length];
        for (int i = 0; i < copyNetwork.layers.Length; i++)
        {
            this.layers[i] = copyNetwork.layers[i];
        }

        InitNeurons();
        InitWeights();
        CopyWeights(copyNetwork.weights);
    }

    /// <summary>
    /// Used for copying networks.
    /// </summary>
    /// <param name="copyWeights">Weights of copy network</param>
    private void CopyWeights(float[][][] copyWeights) {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    weights[i][j][k] = copyWeights[i][j][k];
                }
            }
        }
    }

    /// <summary>
    /// Initialized neurons in each layer.
    /// </summary>
    private void InitNeurons() {
        // Neuron Initialization
        List<float[]> neuronList = new List<float[]>();

        for (int i = 0; i < layers.Length; i++) // run through all layers
        {
            if (i < layers.Length - 2)
            {
                neuronList.Add(new float[layers[i]]); // add layer to neuron list, +1 for bias
                neuronList[i][neuronList[i].Length - 1] = 1.0f;    // Sets the bias neuron to 1
            }
            else
                neuronList.Add(new float[layers[i]]);     // no bias immediately before or on output layer
        }

        neurons = neuronList.ToArray(); // convert list to array
    }

    /// <summary>
    /// Connects all neurons and sets up bias connections.
    /// </summary>
    private void InitWeights() {
        List<float[][]> weightList = new List<float[][]>();

        for (int i = 1; i < layers.Length; i++)
        {
            List<float[]> layerWeightList = new List<float[]>();

            int neuronsInPreviousLayer = layers[i - 1];

            for (int j = 0; j < neurons[i].Length; j++)
            {

                float[] neuronWeights = new float[neuronsInPreviousLayer]; // neuron weights

                if (i < layers.Length - 2)
                {
                    // set the weights randomly between -1 and 1
                    if (j < neurons[i].Length - 1)
                    {
                        for (int k = 0; k < neuronsInPreviousLayer; k++)
                        {
                            //give random weights to neuron weights
                            neuronWeights[k] = UnityEngine.Random.Range(-1.0f, 1.0f);
                        }
                    }
                }
                else
                {
                    for (int k = 0; k < neuronsInPreviousLayer; k++)
                    {
                        //give random weights to neuron weights
                        neuronWeights[k] = UnityEngine.Random.Range(-1.0f, 1.0f);
                    }
                }

                layerWeightList.Add(neuronWeights);
            }

            weightList.Add(layerWeightList.ToArray());
        }

        weights = weightList.ToArray();
    }

    /// <summary>
    /// The meat of the network. Feeds a vector of inputs values into the network
    /// returns the resulting output vector.
    /// </summary>
    /// <param name="inputs">The input vector to be fed through</param>
    /// <returns>Vector of resulting outputs.</returns>
    public float[] FeedForward(float[] inputs) {
        
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }
        
        for (int i = 1; i < layers.Length; i++)
        {
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float value = 0.0f;    // Kind of the bias, not really though

                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    value += weights[i - 1][j][k] * neurons[i - 1][k];
                }

                if (i < layers.Length - 2)
                {
                    if (j < neurons[i].Length - 1)
                    {
                        neurons[i][j] = (float)System.Math.Tanh(value);
                    }
                    else
                    {
                        neurons[i][j] = 1.0f;
                    }
                }
                else
                {
                    neurons[i][j] = (float)System.Math.Tanh(value);
                }
            }
        }
        return neurons[neurons.Length - 1]; // Returns output layer
    }

    /// <summary>
    /// Makes two networks bang each other and produce a child network that shares traits from each parent.
    /// </summary>
    /// <param name="parentA">The first parent network</param>
    /// <param name="parentB">The second parent network</param>
    /// <param name="crossRate">The percent chance of traits being inhierited from parent B</param>
    /// <returns>A new neural network that is the child of the two parent networks.</returns>
    public static NeuralNetwork Reproduce(NeuralNetwork parentA, NeuralNetwork parentB, float crossRate = 0.5f) {
        NeuralNetwork child = new NeuralNetwork(parentA);
        for (int i = 0; i < child.weights.Length; i++)
        {
            for (int j = 0; j < child.weights[i].Length; j++)
            {
                for (int k = 0; k < child.weights[i][j].Length; k++)
                {
                    float rand = UnityEngine.Random.Range(0.0f, 1.0f);
                    if (rand <= crossRate)
                    {
                        child.weights[i][j][k] = parentB.GetWeight(i, j, k);
                    }
                }
            }
        }
        return child;
    }

    /// <summary>
    /// Applies small changes to indiviual weights on a network based on the rate of mutation.
    /// </summary>
    /// <param name="mutationRate">The chance for a change to occur to any single weight. This value should be small.</param>
    public void Mutate(float mutationRate) {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    float weight = weights[i][j][k];

                    // mutate weight value
                    float rand = UnityEngine.Random.Range(0f, 100f);

                    if (rand <= mutationRate)
                    {
                        // flip sign of weight
                        weight *= -1f;
                    }
                    else if (rand <= 2 * mutationRate)
                    {
                        // Pick a random weight between -1 and 1
                        weight = UnityEngine.Random.Range(-1f, 1f);
                    }
                    else if (rand <= 3 * mutationRate)
                    {
                        // Randomly increase by 0% to 100%
                        float factor = UnityEngine.Random.Range(0f, 1f) + 1f;
                        weight *= factor;
                        if (weight > 1f)
                            weight = 1f;
                    }
                    else if (rand <= 4 * mutationRate)
                    {
                        // Randomly decrease by 0% to 100%
                        float factor = UnityEngine.Random.Range(0f, 1f);
                        weight *= factor;
                        if (weight < -1f)
                            weight = -1f;
                    }

                    weights[i][j][k] = weight;
                }
            }
        }
    }

//------------------- Accessors and Mutators ---------------------
    public void AddFitness(float fit) {
        fitness += fit;
    }

    public void SetFitness(float fit) {
        fitness = fit;
    }

    public float GetFitness() {
        return fitness;
    }

    public float GetWeight(int i, int j, int k) {
        return weights[i][j][k];
    }

    public int[] GetLayers() {
        return layers;
    }
//----------------------------------------------------------------

    // Compare two neural networks and sort based on fitness
    public int CompareTo(NeuralNetwork other) {
        if (other == null)
            return 1;

        if (fitness > other.fitness)
            return 1;
        else if (fitness < other.fitness)
            return -1;
        else
            return 0;
    }
}
