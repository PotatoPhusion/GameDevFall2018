using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

/// <summary>
/// I am so sorry to anyone who has to read this file.
/// Even you, future me.
/// </summary>
public class TrainingManager : ManagerBase {

    private bool totalFailure;

    public GameObject carPrefab;
    [HideInInspector]
    public GameObject startLine;

    public int carsPerGeneration;
    public int simultaneousDrivers = 1;
    public int numberOfLaps;

    public float checkpointTime;

    public GameObject[] checkpoints;
    public Transform[] startPositions;

    [HideInInspector]
    public float trackLength;
    [HideInInspector]
    public int numSimulated;

    private int currentGeneration;
    private Car2DData bestCar;

    private GameObject[] cars;
    private Car2DData[] carData;
    private NeuralNetwork[] brains;
    private NeuralNetwork[] smolBrains;

	// Use this for initialization
	void Start () {
        Physics2D.SetLayerCollisionMask(8, Physics2D.GetLayerCollisionMask(8) & ~(1 << 8));
        currentGeneration = 0;
        totalFailure = true;
        Initialize();
	}
	
	// Update is called once per frame
	void Update () {
        if (numSimulated == carsPerGeneration && totalFailure)
        {
            smolBrains = GenerateChildren(brains);
            numSimulated = 0;
            currentGeneration++;
            FindBest();
            Death();
            Cleanup();
            totalFailure = false;
        }
		else if (totalFailure)
        {
            SpawnCars();
            totalFailure = false;
        }
        else
        {
            totalFailure = true;
            // TODO: Optimize me cap'n
            for (int i = 0; i < cars.Length; i++)
            {
                if (cars[i] != null)
                {
                    // Save data and destroy failed cars
                    if (cars[i].GetComponent<Car2DTrainer>().hasFailed)
                    {
                        Car2DTrainer car = cars[i].GetComponent<Car2DTrainer>();

                        // Saving important car info
                        carData[i] = new Car2DData(car.GetNetwork(), car.numberOfSightLines, car.FOV, car.sightDistance);

                        Destroy(cars[i]);
                    }
                }

                if (cars[i] != null)
                {
                    // Update each car's progress along the track
                    TrackProgress(cars[i]);
                    totalFailure = false;

                    if (cars[i].GetComponent<Car2DTrainer>().lap > numberOfLaps)
                    {
                        cars[i].GetComponent<Car2DTrainer>().hasFailed = true;     // Stop a car that has completed all laps
                        Destroy(cars[i]);
                    }
                }
            }
        }

	}

    /// <summary>
    /// Sets up all data structures and loads generation 0 networks into the brains array.
    /// </summary>
    private void Initialize() {
        InitializeCheckpoints();

        cars = new GameObject[carsPerGeneration];
        brains = new NeuralNetwork[carsPerGeneration];
        carData = new Car2DData[carsPerGeneration];
        smolBrains = brains;

        for (int i = 0; i < carsPerGeneration; i++)
        {
            cars[i] = Instantiate(
                carPrefab,
                new Vector3(1000.0f, 1000.0f, 1000.0f),     // Somewhere off screen
                Quaternion.identity
            );
            brains[i] = cars[i].GetComponent<NeuralNetworkAgent>().GetNetwork();
            Destroy(cars[i]);
        }
    }

    /// <summary>
    /// Uses the checkpoints to map out the layout of the track.
    /// This is then used to determine if the cars are making progess.
    /// </summary>
    private void InitializeCheckpoints() {
        startLine = checkpoints[0];
        startLine.GetComponent<Checkpoint>().isStart = true;
        startLine.GetComponent<Checkpoint>().checkpointIndex = 0;

        for (int i = 1; i < checkpoints.Length; i++)
        {
            trackLength += Mathf.Abs((checkpoints[i - 1].transform.position - checkpoints[i].transform.position).magnitude);
            checkpoints[i].GetComponent<Checkpoint>().distanceFromStart = trackLength;
            checkpoints[i].GetComponent<Checkpoint>().checkpointIndex = i;
        }
        trackLength += Mathf.Abs((checkpoints[checkpoints.Length - 1].transform.position - startLine.transform.position).magnitude);
        startLine.GetComponent<Checkpoint>().distanceFromStart = 0.0f;
    }

    /// <summary>
    /// Spawns the training cars at the center of the start line.
    /// </summary>
    private void SpawnCars() {
        for (int i = 0; i < startPositions.Length && numSimulated < carsPerGeneration; i++)
        {
            cars[numSimulated] = Instantiate(
                carPrefab,
                startPositions[i].position,
                startPositions[i].rotation
            );
            cars[numSimulated].GetComponent<NeuralNetworkAgent>().SetNetwork( brains[numSimulated] );
            cars[numSimulated].gameObject.name = "Car " + numSimulated;
            cars[numSimulated].GetComponent<Car2DTrainer>().totalCheckpoints = checkpoints.Length;
            cars[numSimulated].GetComponent<Car2DTrainer>().SetCheckpointTime(checkpointTime);
            cars[numSimulated].GetComponent<Car2DTrainer>().SetTrackLength(trackLength);
            numSimulated++;
        }
    }

    private void ResetGeneration() {
        Cleanup();
        numSimulated = 0;
    }

    private void FindBest() {
        if (currentGeneration < 1)
        {
            Debug.LogError("Cannot find best on generation 0");
            return;
        }

        float fitness = 0;
        int index = 0;
        for (int i = 0; i < carData.Length; i++)
        {
            if (fitness < carData[i].GetNetwork().GetFitness())
            {
                fitness = carData[i].GetNetwork().GetFitness();
                index = i;
            }
        }
        bestCar = carData[index];
    }

    /// <summary>
    /// THIS STILL NEEDS SOME TUNING!!
    /// Kills off some of the networks based on their fitness score.
    /// All dead parents are replaced with child networks.
    /// </summary>
    private void Death() {
        float sum = 0.0f;
        float[] roulette = new float[brains.Length];

        for (int i = 0; i < brains.Length; i++)
        {
            float deathChance;
            if (brains[i].GetFitness() == 0.0f)
                deathChance = 10f;
            else
                deathChance = 1.0f / brains[i].GetFitness();
            roulette[i] = deathChance;
            sum += deathChance;
        }

        for (int i = 0; i < brains.Length; i++)
        {
            if (Random.Range(0.0f, 1.0f) <= roulette[i])
            {
                //Debug.Log("Car " + i + " has crashed.");
                brains[i] = smolBrains[i];
            }
        }
    }

    private void Cleanup() {
        for (int i = 0; i < cars.Length; i++)
        {
            Destroy(cars[i]);
        }
    }
    
    /// <summary>
    /// Lets each car know its progress around the track for calculating fitness.
    /// </summary>
    /// <param name="car">The car to determine the position of</param>
    private void TrackProgress(GameObject car) {

        int lc = car.GetComponent<Car2DTrainer>().lastCheckpoint;
        int nc = car.GetComponent<Car2DTrainer>().nextCheckpoint;

        Vector2 p = car.transform.position;
        Vector2 gL = checkpoints[ lc % checkpoints.Length ].transform.position;
        Vector2 gN = checkpoints[ nc % checkpoints.Length ].transform.position;

        // Black magic Pythagoras.
        float distance =
            Mathf.Abs((p - gL).magnitude) /
            (Mathf.Abs((p - gL).magnitude) + Mathf.Abs((gN - p).magnitude)) *
            Mathf.Abs((gN - gL).magnitude);

        float progress =
            (trackLength * (car.GetComponent<Car2DTrainer>().lap - 1)) +           // Lap count
            checkpoints[lc].GetComponent<Checkpoint>().distanceFromStart +  // Last passed checkpoint
            distance;                                                       // Distance from last checkpoint

        car.GetComponent<Car2DTrainer>().SetTrackProgress(progress);
    }

    public void LoadGeneration() {

        GenerationData loadData;
        if (LoadGenerationFromFile(out loadData))
        {
            currentGeneration = loadData.currentGeneration;
            for (int i = 0; i < carData.Length; i++)
            {
                brains[i] = loadData.carData[i].GetNetwork();
            }
            carData = loadData.carData;
            bestCar = loadData.bestCar;
            totalFailure = true;
            ResetGeneration();
        }
        else
        {
            Debug.LogError("File " + Application.persistentDataPath + "/savedGeneration.gen" + " could not be found.");
        }
    }
    
    public void SaveBest() {
        if (currentGeneration < 1)
        {
            Debug.LogError("Cannot save best on generation 0");
            return;
        }
        Car2DData saveData = bestCar;

        Save(saveData);
    }

    public void SaveLastGeneration() {
        if (currentGeneration < 1)
        {
            Debug.LogError("Cannot save last generation while on Generation 0.");
            return;
        }

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/savedGeneration.gen", FileMode.OpenOrCreate);

        GenerationData genData = new GenerationData(carData, currentGeneration - 1, bestCar);
        bf.Serialize(file, genData);
        file.Close();
        Debug.Log("Successfully saved last generation.");
    }

    private void Save(Car2DData data) {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/savedCar.car", FileMode.OpenOrCreate);

        bf.Serialize(file, data);
        file.Close();
        Debug.Log("Successfully Saved!");
    }

    private bool LoadGenerationFromFile(out GenerationData loadData) {
        if (File.Exists(Application.persistentDataPath + "/savedGeneration.gen"))
        {
            // Load file
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/savedGeneration.gen", FileMode.Open);

            loadData = (GenerationData)bf.Deserialize(file);

            return true;
        }
        else
        {
            loadData = null;
            return false;
        }
    }

    private bool LoadTest() {
        Car2DData loadedData;
        if (!Load(out loadedData))
            return false;

        GameObject loadCar = Instantiate(carPrefab,
                                         startLine.transform.position,
                                         startLine.transform.rotation);
        Car2DTrainer carData = loadCar.GetComponent<Car2DTrainer>();
        loadCar.name = "Loaded Car";
        loadCar.GetComponent<Renderer>().material.color = Color.green;

        carData.SetNetwork(loadedData.GetNetwork());
        carData.numberOfSightLines = loadedData.GetSightLines();
        carData.FOV = loadedData.GetFOV();
        carData.sightDistance = loadedData.GetSightDistance();

        return true;
    }

    private bool Load(out Car2DData data) {
        if(File.Exists(Application.persistentDataPath + "/savedNetwork.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/savedNetwork.dat", FileMode.Open);

            data = (Car2DData)bf.Deserialize(file);
            Debug.Log("Successfully Loaded!");

            return true;
        }
        else
        {
            Debug.LogError("File " + Application.persistentDataPath + "/savedNetwork.dat" + " could not be found.");

            data = null;
            return false;
        }
    }

    public int GetCurrentGeneration() {
        return currentGeneration;
    }
}

[System.Serializable]
public class GenerationData {
    public Car2DData[] carData;
    public int currentGeneration;
    public Car2DData bestCar;

    public GenerationData(Car2DData[] data, int gen, Car2DData best) {
        carData = data;
        currentGeneration = gen;
        bestCar = best;
    }
}
