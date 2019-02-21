using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerBase : MonoBehaviour {

    [Tooltip("The percentage of traits for a child to recieve from Parent B")]
    [Range(0.0f, 1.0f)]
    public float crossoverRate = 0.5f;
    [Tooltip("The percent (not decimal) chance for a mutation to occur on any given weight in a network." +
        " Values should be kept below 1.0 for realistic mutation rates. Large values cause quicker, less stable" +
        " changes.")]
    public float mutationRate;

    /// <summary>
    /// Selects two parent Neural Networks for use in reproduction.
    /// </summary>
    /// <param name="agents">An array of GameObjects that contain a NeuralNetworkAgent component or
    ///     child of a NeuralNetworkAgent</param>
    /// <returns>An array of two parent NeuralNetworks.</returns>
    public NeuralNetwork[] SelectParents(GameObject[] agents) {

        int size = agents.Length;
        float sum = 0.0f;
        float[] roulette = new float[size];
        NeuralNetwork[] parents = new NeuralNetwork[2];

        for (int j = 0; j < size; j++)
        {
            sum += agents[j].GetComponent<NeuralNetworkAgent>().GetFitness();
            roulette[j] = sum;
        }

        bool parentsFound = false;
        int i = 0;
        int parentA = -1;
        while (!parentsFound)
        {
            float rand = Random.Range(0.0f, sum);

            for (int j = 0; j < size; j++)
            {
                if (rand <= roulette[j])
                {
                    if (i == 0)
                    {
                        parents[i] = agents[j].GetComponent<NeuralNetworkAgent>().GetNetwork();
                        parentA = j;
                        i++;
                        break;  // Parent found, find next parent.
                    }
                    else
                    {
                        if (j == parentA)
                        {
                            break;  // Break, non-unique parents.
                        }
                        else
                        {
                            parents[i] = agents[j].GetComponent<NeuralNetworkAgent>().GetNetwork();
                            parentsFound = true;
                            break;  // Both parents found!
                        }
                    }
                }
            }
        }
        return parents;
    }

    /// <summary>
    /// Selects two parent Neural Networks for use in reproduction.
    /// </summary>
    /// <param name="networks">An array of NeuralNetworks that <i>may</i> be associated with 
    ///     a NeuralNetworkAgent GameObject</param>
    /// <returns>An array of two parent NeuralNetworks.</returns>
    public NeuralNetwork[] SelectParents(NeuralNetwork[] networks) {

        int size = networks.Length;
        float sum = 0.0f;
        float[] roulette = new float[size];
        NeuralNetwork[] parents = new NeuralNetwork[2];

        for (int j = 0; j < size; j++)
        {
            sum += networks[j].GetFitness();
            roulette[j] = sum;
        }

        bool parentsFound = false;
        int i = 0;
        int parentA = -1;
        while (!parentsFound)
        {
            float rand = Random.Range(0.0f, sum);

            for (int j = 0; j < size; j++)
            {
                if (rand <= roulette[j])
                {
                    if (i == 0)
                    {
                        parents[i] = networks[j];
                        parentA = j;
                        i++;
                        break;  // Parent found, find next parent.
                    }
                    else
                    {
                        if (j == parentA)
                        {
                            break;  // Break, non-unique parents.
                        }
                        else
                        {
                            parents[i] = networks[j];
                            parentsFound = true;
                            break;  // Both parents found!
                        }
                    }
                }
            }
        }
        return parents;
    }

    /// <summary>
    /// Generates child networks given an array of parent GameObjects.
    /// </summary>
    /// <param name="currentGeneration">Array of any NeuralNetworkAgent GameObjects</param>
    /// <returns>Array of child networks of the same length as the currentGeneration</returns>
    protected NeuralNetwork[] GenerateChildren(GameObject[] currentGeneration) {
        NeuralNetwork[] children = new NeuralNetwork[currentGeneration.Length];

        int count = 0;
        Debug.Log("Current Generation Length: " + currentGeneration.Length);
        for (int i = 0; i < currentGeneration.Length; i++)
        {
            NeuralNetwork[] parents = SelectParents(currentGeneration);
            children[i] = NeuralNetwork.Reproduce(parents[0], parents[1]);
            children[i].Mutate(mutationRate);
            if (children[i] != null)
                count++;
        }
        Debug.Log("Non-Null Children: " + count);
        return children;
    }

    /// <summary>
    /// Generates child networks given an array of parent networks.
    /// </summary>
    /// <param name="currentGeneration">Array of parent neural networks.</param>
    /// <returns>Array of child networks of the same length as the currentGeneration</returns>
    protected NeuralNetwork[] GenerateChildren(NeuralNetwork[] currentBrains) {
        NeuralNetwork[] children = new NeuralNetwork[currentBrains.Length];

        for (int i = 0; i < currentBrains.Length; i++)
        {
            NeuralNetwork[] parents = SelectParents(currentBrains);
            children[i] = NeuralNetwork.Reproduce(parents[0], parents[1], crossoverRate);
            children[i].Mutate(mutationRate);
        }
        return children;
    }
}
