using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Car2D))]
public class Car2DTrainer : NeuralNetworkAgent {

    // Velocity x and y
    private const int EXTRA_INPUTS = 2;

    [Range(2, 20)]
    public int numberOfSightLines;
    public float FOV = 90.0f;
    public float sightDistance;
    [Range(0, 1)]
    public float wallBias;
    [Range(0, 1)]
    public float distanceBias;
    public bool touchWalls;

    [HideInInspector]
    public int lap;

    [HideInInspector]
    public int lastCheckpoint;
    [HideInInspector]
    public int nextCheckpoint;
    [HideInInspector]
    public int totalCheckpoints;
    public GameObject[] checkpoints;

    public float fit;
    public float trackProgress;

    [HideInInspector]
    public float throttle;

    private float[] raycastDistances;
    private float wallFitness;
    private float bonusMultiplier;
    private float lapTimeBonus;
    private float penalties;

    private float trackLength;
    private int lastLap;
    private float lastLapTime;
    private float lapTimer;

    private float checkpointTime;
    private float cptCounter;

    private Car2D car;
    private Rigidbody2D rb;
    private int layerMask;

    // Use this for initialization
    void Start () {
        //layerMask = Physics2D.GetLayerCollisionMask(gameObject.layer);
        checkpoints = GameObject.Find("Training Manager").GetComponent<TrainingManager>().checkpoints;   // What the fuck is this mess?
        car = gameObject.GetComponent<Car2D>();
        rb = gameObject.GetComponent<Rigidbody2D>();

        checkpointTime = 20.0f;
        lap = 1;
        lastLap = lap;
        lastCheckpoint = 0;
        nextCheckpoint = 1;

        layers[0] = numberOfSightLines * 2 + EXTRA_INPUTS;

        raycastDistances = new float[numberOfSightLines];
        Run();
	}
	
	// Update is called once per frame
	void Update () {
        // Fitness function
        if (!hasFailed)
            UpdateFitness();
        else
        {
            Destroy(gameObject);
        }

        // Increased fitness for completeing laps faster
        if (lastLap < lap)
        {
            lastLap = lap;
            lastLapTime = lapTimer;
            float minimumBonusTime = trackLength / (car.maxSpeed / 2.0f);
            if (lastLapTime < minimumBonusTime)
            {
                lapTimeBonus = minimumBonusTime - lastLapTime;
            }
        }

        // Took too long, maybe off track
        if (cptCounter >= checkpointTime)
        {
            hasFailed = true;
        }

        lapTimer += Time.deltaTime;
        cptCounter += Time.deltaTime;
        fit = net.GetFitness();
	}

    /// <summary>
    /// ReadInputs is responsible for collecting all necessary data to feed into a
    /// neural network.
    /// </summary>
    /// <returns>Vector of input values.</returns>
    protected override float[] ReadInputs() {
        float[] inputs = new float[layers[0]];
        layerMask = Physics2D.DefaultRaycastLayers & ~(1 << 8);

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
                raycastDistances[i / 2] = hitInfo.distance;
            }
            else
            {
                //Debug.Log("Hit: NOTHING");
                //Debug.Log("Distance for Ray{" + i + "}: " + hitInfo.distance);

                inputs[i] = sightDistance;
                inputs[i + 1] = -1.0f;
                raycastDistances[i / 2] = sightDistance;
            }
        }

        // Tell the network it's current velocity
        inputs[numberOfSightLines * 2] = rb.velocity.x;
        inputs[numberOfSightLines * 2 + 1] = rb.velocity.y;

        Debug.DrawLine(transform.position, checkpoints[nextCheckpoint].transform.position, Color.black);
        Debug.DrawLine(transform.position, transform.up, Color.magenta);


        float angle = Vector2.Angle(checkpoints[nextCheckpoint].transform.position - transform.position, transform.up);
        float checkpointCoeff = Mathf.Cos(angle * Mathf.Deg2Rad);
        //Debug.Log(checkpointCoeff);

        inputs[inputs.Length - 1] = checkpointCoeff;

        return inputs;
    }

    /// <summary>
    /// Performs all tasks based on the output of the attached neural network.
    /// </summary>
    /// <param name="outputs">The array of outputs provided by the network</param>
    protected override void ProcessOutputs(float[] outputs) {
        this.throttle = outputs[0];
        car.Accelerate(outputs[0]);
        car.Steer(outputs[1]);

        // Penalty for driving backwards
        if (rb.velocity.magnitude < 0.0f)
        {
            penalties -= rb.velocity.magnitude * Time.deltaTime;
        }
    }

    /*
    /// <summary>
    /// Linearly interpolates to a new rotation defined by a
    /// percentage of 180 degrees.
    /// </summary>
    /// <param name="percent">The fraction of 180 degrees to rotate
    /// (1.0 = 180, -1.0 = -180).</param>
    private void Rotate(float percent) {
        transform.Rotate(Vector3.back, percent * turnSpeed * Time.deltaTime);
    }
    */

    /*
    /// <summary>
    /// Moves the car forward by a percentage of its max move speed.
    /// Moving backwards hurts fitness.
    /// </summary>
    /// <param name="throttle">How fast to move. 1.0f = full throttle.</param>
    private void Move(float throttle) {
        transform.position += transform.up * throttle * moveSpeed * Time.deltaTime;
        if (throttle < 0.0f)
        {
            penalties -= throttle * moveSpeed * Time.deltaTime;
        }
    }
    */

    /// <summary>
    /// Update fitness calculates the fitness based on multiple parameters
    /// of the car's performance.
    /// </summary>
    private void UpdateFitness() {
        float sum = 0;
        for (int i = 0; i < numberOfSightLines; i++)
        {
            sum += raycastDistances[i];
        }
        float average = sum / numberOfSightLines;
        wallFitness += (average * wallBias) * Time.deltaTime;

        bonusMultiplier = (lastCheckpoint / (float)totalCheckpoints) + lap;

        float fitness =
            wallFitness +
            ((trackProgress + bonusMultiplier) * distanceBias) +
            lapTimeBonus -
            (lapTimer * distanceBias) -
            penalties;

        if (fitness < 0.1f)     // 0.0f makes things crash...
        {
            fitness = 0.1f;
        }

        net.SetFitness(fitness);
    }

    /// <summary>
    /// Used to check if this car has passed a checkpoint.
    /// </summary>
    /// <param name="collision">The overlapping collider</param>
    public void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Checkpoint")
        {
            if (collision.gameObject.GetComponent<Checkpoint>().checkpointIndex == nextCheckpoint)
            {
                if (collision.gameObject.GetComponent<Checkpoint>().isStart)
                {
                    lap++;
                }
                cptCounter = 0.0f;      // Reset checkpoint time
                lastCheckpoint = nextCheckpoint;
                nextCheckpoint = (nextCheckpoint + 1) % totalCheckpoints;
            }
        }
    }

    /// <summary>
    /// Used to destroy the car on collision with a wall.
    /// </summary>
    /// <param name="collision">The overlapping collider</param>
    public void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Car")   // Is this a car?
        {
            return;
        }
        else if (!touchWalls)
        {
            hasFailed = true;
        }
        else
        {
            penalties += 5.0f * Time.deltaTime; // Wall penalty
        }
    }

//-------------------Accessors and Mutators --------------------------
    public void SetTrackProgress(float progress) {
        trackProgress = progress;
    }

    public float GetCheckpointTimer() {
        return cptCounter;
    }

    public void SetCheckpointTime(float cpt) {
        checkpointTime = cpt;
    }

    public void SetTrackLength(float length) {
        trackLength = length;
    }
//---------------------------------------------------------------------
}
