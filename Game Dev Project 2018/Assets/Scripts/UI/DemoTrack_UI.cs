using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DemoTrack_UI : MonoBehaviour {

    public TrainingManager trainingManager;

    public Text generationText;
    public Text testedText;
    public Button saveGenerationButton;
    public Button saveBestButton;
    public Button loadGenerationButton;
    //public Button backButton;

    //public Dropdown trackSelector;

    //public GameObject DontDestroyList;

	// Use this for initialization
	void Start () {
        saveGenerationButton.onClick.AddListener(SaveGeneration);
        saveBestButton.onClick.AddListener(SaveBest);
        loadGenerationButton.onClick.AddListener(LoadGeneration);
        /*
        backButton.onClick.AddListener(Back);
        trackSelector.onValueChanged.AddListener(delegate {
            ChangeMaps();
        });
        */
	}

    void SaveGeneration() {
        trainingManager.SaveLastGeneration();
    }

    void SaveBest() {
        trainingManager.SaveBest();
    }

    void LoadGeneration() {
        trainingManager.LoadGeneration();
    }

    void Update() {
        generationText.text = string.Format("Generation: {0}", trainingManager.GetCurrentGeneration());
        testedText.text = string.Format("Population: {0} / {1}", trainingManager.numSimulated, trainingManager.carsPerGeneration);
    }

    private void ChangeMaps() {

    }

    private void Back() {
        SceneManager.LoadScene("MainMenu");
    }
}
