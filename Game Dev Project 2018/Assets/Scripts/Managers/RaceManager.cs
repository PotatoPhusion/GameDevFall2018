using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour {

    public Racing_UI UIManager;

    public int humanRacers = 1;
    private int difficulty = 1;

    public new Camera camera;
    public GameObject playerPrefab;
    public GameObject dummyPrefab;
    public GameObject[] checkpoints;
    public Transform[] startPositions;

    private GameObject[] racers;
    private float trackLength;
    private bool countDownFinished;
    private bool hasRaceBegun;
    private bool raceFinished;

    void Awake() {
        Physics2D.SetLayerCollisionMask(8, Physics2D.GetLayerCollisionMask(8) & (1 << 8));
    }

    // Use this for initialization
    void Start () {
        Initialize();
        switch (difficulty)
        {
            case 0:
                LoadEasy();
                break;
            case 1:
                LoadMedium();
                break;
            case 2:
                LoadHard();
                break;
            default:
                LoadMedium();
                break;
        }
        UIManager.player = racers[racers.Length - 1];
        UIManager.StartCountdown(3.0f);
        for (int i = 0; i < racers.Length; i++)
        {
            TrackProgress(racers[i]);
        }
        UIManager.playerPosition = DeterminePosition();
    }
	
	// Update is called once per frame
	void Update () {
        if (UIManager.HasCountdownFinished() && !hasRaceBegun)
        {
            BeginRace();
            hasRaceBegun = true;
        }
        else if (!hasRaceBegun)
        {
            return;
        }
        for (int i = 0; i < racers.Length; i++)
        {
            TrackProgress(racers[i]);
        }
        UIManager.playerPosition = DeterminePosition();
        if (racers[racers.Length - 1].GetComponent<Car2D>().lap == 4 && !raceFinished)
        {
            EndRace();
            UIManager.RaceFinished();
        }
	}

    private void Initialize() {
        InitializeCheckpoints();

        racers = new GameObject[startPositions.Length];
        for (int i = 0; i < racers.Length - humanRacers; i++)
        {
            racers[i] = Instantiate(
                dummyPrefab,
                startPositions[i].position,
                startPositions[i].parent.rotation);
            racers[i].GetComponent<Car2DDummy>().SetCheckpoints(checkpoints);
        }
        for (int i = 1; i <= humanRacers; i++)
        {
            racers[racers.Length - i] = Instantiate(
                playerPrefab,
                startPositions[startPositions.Length - i].position,
                startPositions[startPositions.Length - i].parent.rotation);
            racers[racers.Length - i].GetComponent<Car2D>().SetCheckpoints(checkpoints);
        }

        camera.GetComponent<CameraController>().ChangeTarget(racers[racers.Length - 1]);
    }

    /// <summary>
    /// Uses the checkpoints to map out the layout of the track.
    /// This is then used to determine if the cars are making progess.
    /// </summary>
    private void InitializeCheckpoints() {
        GameObject startLine = checkpoints[0];
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

    private void LoadEasy() {
        Car2DData[] loadedRacers = LoadSave.LoadRacers("Easy");
        for (int i = 0; i < racers.Length - humanRacers; i++)
        {
            racers[i].GetComponent<Car2DDummy>().LoadData(loadedRacers[Random.Range(0, loadedRacers.Length)]);
        }
    }

    private void LoadMedium() {
        Car2DData[] loadedRacers = LoadSave.LoadRacers("Medium");
        for (int i = 0; i < racers.Length - humanRacers; i++)
        {
            racers[i].GetComponent<Car2DDummy>().LoadData(loadedRacers[Random.Range(0, loadedRacers.Length)]);
        }
    }

    private void LoadHard() {
        Car2DData[] loadedRacers = LoadSave.LoadRacers("Hard");
        for (int i = 0; i < racers.Length - humanRacers; i++)
        {
            racers[i].GetComponent<Car2DDummy>().LoadData(loadedRacers[Random.Range(0, loadedRacers.Length)]);
        }
    }

    private void BeginRace() {
        for (int i = 0; i < racers.Length - humanRacers; i++)
        {
            racers[i].GetComponent<Car2DDummy>().BeginRacing();
        }
    }

    private void EndRace() {
        for (int i = 0; i < racers.Length - humanRacers; i++)
        {
            racers[i].GetComponent<Car2DDummy>().StopRacing();
        }
        raceFinished = true;
    }

    private int DeterminePosition() {
        int pos = 1;
        float playerProgress = racers[racers.Length - 1].GetComponent<Car2D>().trackProgress;
        for (int i = racers.Length - 2; i >= 0; i--)
        {
            if (racers[i].GetComponent<Car2D>().trackProgress > playerProgress)
            {
                pos++;
            }
        }
        return pos;
    }

    private void TrackProgress(GameObject car) {

        int lc = car.GetComponent<Car2D>().lastCheckpoint;
        int nc = (lc + 1) % checkpoints.Length;

        Vector2 p = car.transform.position;
        Vector2 gL = checkpoints[lc % checkpoints.Length].transform.position;
        Vector2 gN = checkpoints[nc % checkpoints.Length].transform.position;

        // Black magic Pythagoras.
        float distance =
            Mathf.Abs((p - gL).magnitude) /
            (Mathf.Abs((p - gL).magnitude) + Mathf.Abs((gN - p).magnitude)) *
            Mathf.Abs((gN - gL).magnitude);

        float progress =
            (trackLength * (car.GetComponent<Car2D>().lap - 1)) +           // Lap count
            checkpoints[lc].GetComponent<Checkpoint>().distanceFromStart +  // Last passed checkpoint
            distance;                                                       // Distance from last checkpoint

        car.GetComponent<Car2D>().SetTrackProgress(progress);
    }

    public void SetDifficulty(int difficulty) {
        this.difficulty = difficulty;
    }
}
