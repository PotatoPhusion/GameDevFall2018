using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkVisualizer : MonoBehaviour {

    public GameObject netGO;
    private NeuralNetwork net;

    void OnGUI() {
        if (net != null)
        {
            
            for (int i = 0; i < net.neurons.Length; i++)
            {
                for (int j = 0; j < net.neurons[i].Length; j++)
                {
                    GUI.TextField(new Rect(30 * i, 30 * j, 30, 30), net.neurons[i][j].ToString("F1"));
                }
            }
            
        }
    }

    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        if (netGO != null && net == null)
            net = netGO.GetComponent<NeuralNetworkAgent>().GetNetwork();
	}
}
