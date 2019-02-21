using System.Collections;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkAgent : MonoBehaviour {

    public static bool isSerial = true;

    [Tooltip("The first element is always the number of inputs." +
        " The last element is always the number of outputs." +
        " All intermediate layers are hidden layers of the network.")]
    public int[] layers;
    [HideInInspector]
    public bool hasFailed;

    protected NeuralNetwork net;

    /// <summary>
    /// A new network is always initalized when a NeuralNetworkAgent is instantiated.
    /// The network can be immediately overwritten, however, with SetNetwork().
    /// </summary>
    void Awake() {
        if (net == null)
            net = new NeuralNetwork(layers);
    }

    /// <summary>
    /// Accessor for the fitness of the NeuralNetwork.
    /// If there is no current network, it returns 0.
    /// </summary>
    /// <returns>Fitness of the network.</returns>
    public float GetFitness() {
        if (net == null)
        {
            return 0.0f;
        }
        else
        {
            return net.GetFitness();
        }
    }

    /// <summary>
    /// Accessor for the NeuralNetwork of this NeuralNetworkAgent.
    /// Can return <code>null</code> if there is no current network.
    /// </summary>
    /// <returns>NeuralNetwork of this NeuralNetworkAgent.</returns>
    public NeuralNetwork GetNetwork() {
        return net;
    }

    /// <summary>
    /// Directly assigns the network in this NeuralNetworkAgent to the provided network.
    /// The original network is overwritten.
    /// </summary>
    /// <param name="newNet">The new network to assign to this NeuralNetworkAgent</param>
    public void SetNetwork(NeuralNetwork newNet) {
        net = newNet;
    }

    protected void Run() {
        if (isSerial)
        {
            StartCoroutine(RunSerial());
        }
        else
        {
            StartCoroutine(RunParallel());
        }
    }

    protected void Stop() {
        if (isSerial)
        {
            StopCoroutine(RunSerial());
        }
        else
        {
            StopCoroutine(RunParallel());
        }
    }

    private IEnumerator RunSerial() {
        while (!hasFailed)
        {
            float[] inputs = ReadInputs();
            float[] outputs = net.FeedForward(inputs);
            ProcessOutputs(outputs);
            yield return new WaitForFixedUpdate();
        }
    }

    /// <summary>
    /// This is the main loop for the neural network. Every network performs its 
    /// feed forward operation on a separate thread and then yields for one frame
    /// before expecting the thread to finish.
    /// </summary>
    /// <returns>Nothing.</returns>
    private IEnumerator RunParallel() {
        while (!hasFailed)
        {
            float[] inputs = ReadInputs();
            float[] outputs = new float[layers[layers.Length - 1]];
            Thread thread = new Thread(
            () =>
            {
                outputs = net.FeedForward(inputs);
            });
            thread.Start();
            yield return new WaitForFixedUpdate();
            thread.Join();
            ProcessOutputs(outputs);
        }
    }

    protected virtual float[] ReadInputs() { return null; }
    protected virtual void ProcessOutputs(float[] outputs) { }
}
