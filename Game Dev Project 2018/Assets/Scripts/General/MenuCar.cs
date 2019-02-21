using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Car2D))]
public class MenuCar : MonoBehaviour {

    private NeuralNetwork net;
    private int numberOfSightLines;
    private float FOV;
    private float sightDistance;

    private Rigidbody2D rb;
    private Car2D car;

    private bool stopRunning;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        car = GetComponent<Car2D>();
    }

    // Use this for initialization
    void Start () {
        Car2DData[] loadedRacers = LoadSave.LoadRacers("Medium");
        LoadData(loadedRacers[Random.Range(0, loadedRacers.Length)]);
        BeginRacing();
    }
	
	// Update is called once per frame
	void Update () {

	}

    public void LoadData(Car2DData data) {
        net = data.GetNetwork();
        numberOfSightLines = data.GetSightLines();
        FOV = data.GetFOV();
        sightDistance = data.GetSightDistance();
    }

    /// <summary>
    /// ReadInputs is responsible for collecting all necessary data to feed into a
    /// neural network.
    /// </summary>
    /// <returns>Vector of input values.</returns>
    private float[] ReadInputs() {
        float[] inputs = new float[net.GetLayers()[0]];
        int layerMask = Physics2D.DefaultRaycastLayers & ~(1 << 8);

        Vector2 direction = transform.position;
        direction.y += sightDistance;
        direction = MoreMath.RotateVector2(direction, transform.position, transform.eulerAngles.z);
        //Debug.DrawRay(transform.position, direction - (Vector2)transform.position, Color.blue, Time.deltaTime);

        float arcIncrement = FOV / (numberOfSightLines - 1);

        direction = MoreMath.RotateVector2(direction, transform.position, (FOV / 2));

        for (int i = 0; i < numberOfSightLines * 2; i += 2)
        {
            RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, direction - (Vector2)transform.position, sightDistance, layerMask);
            Debug.DrawRay(transform.position, direction - (Vector2)transform.position, Color.red, Time.deltaTime);
            direction = MoreMath.RotateVector2(direction, transform.position, -arcIncrement);

            if (hitInfo.collider != null)
            {
                //Debug.Log("Hit: " + hitInfo.transform.name);
                //Debug.Log("Distance for Ray{" + i + "}: " + hitInfo.distance);

                inputs[i] = hitInfo.distance;
                inputs[i + 1] = hitInfo.collider.gameObject.layer;
            }
            else
            {
                inputs[i] = sightDistance;
                inputs[i + 1] = -1.0f;
            }
        }

        // Tell the network it's current velocity
        inputs[numberOfSightLines * 2] = rb.velocity.x;
        inputs[numberOfSightLines * 2 + 1] = rb.velocity.y;


        //float angle = Vector2.Angle(checkpoints[nextCheckpoint].transform.position - transform.position, transform.up);
        //float checkpointCoeff = Mathf.Cos(angle * Mathf.Deg2Rad);

        inputs[inputs.Length - 1] = 1.0f;

        return inputs;
    }

    /// <summary>
    /// Performs all tasks based on the output of the attached neural network.
    /// </summary>
    /// <param name="outputs">The array of outputs provided by the network</param>
    private void ProcessOutputs(float[] outputs) {
        car.Accelerate(outputs[0]);
        car.Steer(outputs[1]);
    }

    public void BeginRacing() {
        stopRunning = false;
        StartCoroutine(Run());
    }

    public void StopRacing() {
        stopRunning = true;
    }

    private IEnumerator Run() {
        while (!stopRunning)
        {
            float[] inputs = ReadInputs();
            float[] outputs = net.FeedForward(inputs);
            ProcessOutputs(outputs);
            yield return new WaitForFixedUpdate();
        }
    }
}
