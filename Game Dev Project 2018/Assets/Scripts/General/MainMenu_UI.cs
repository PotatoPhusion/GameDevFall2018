using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu_UI : MonoBehaviour {

    public Button raceTrack1;
    public Button raceTrack2;
    public Button quitButton;

	// Use this for initialization
	void Start () {
        raceTrack1.onClick.AddListener(RaceTrack1);
        raceTrack2.onClick.AddListener(RaceTrack2);
        quitButton.onClick.AddListener(Quit);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void RaceTrack1() {
        SceneManager.LoadScene(1);
    }

    void RaceTrack2() {
        SceneManager.LoadScene(2);
    }

    void Quit() {
        Application.Quit();
    }
}
