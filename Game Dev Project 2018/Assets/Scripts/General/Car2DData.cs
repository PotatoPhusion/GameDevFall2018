using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Car2DData {

    private NeuralNetwork net;
    private int numberOfSightLines;
    private float FOV;
    private float sightDistance;

    public Car2DData() {
        this.net = null;
        this.numberOfSightLines = 5;
        this.FOV = 90.0f;
        this.sightDistance = 5.0f;
    }

    public Car2DData(NeuralNetwork net, int sightLines, float FOV, float sightDistance) {
        this.net = net;
        this.numberOfSightLines = sightLines;
        this.FOV = FOV;
        this.sightDistance = sightDistance;
    }

    public NeuralNetwork GetNetwork() {
        return net;
    }

    public void SetNetwork(NeuralNetwork net) {
        this.net = net;
    }

    public int GetSightLines() {
        return numberOfSightLines;
    }

    public void SetSightLines(int sightLines) {
        this.numberOfSightLines = sightLines;
    }

    public float GetFOV() {
        return FOV;
    }

    public void SetFOV(float FOV) {
        this.FOV = FOV;
    }

    public float GetSightDistance() {
        return sightDistance;
    }

    public void SetSightDistance(float sightDistance) {
        this.sightDistance = sightDistance;
    }
}
