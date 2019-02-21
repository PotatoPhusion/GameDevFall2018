using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Racing_UI : MonoBehaviour {

    public RaceManager raceManager;
    public TextMeshProUGUI positionText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI lapText;
    public TextMeshProUGUI countdownText;
    public Image CDGauge;
    public RectTransform finishPanel;

    [HideInInspector]
    public GameObject player;
    [HideInInspector]
    public int playerPosition;

    private Button backButton;
    private bool countdownFinished;
    private string suffix;

	// Use this for initialization
	void Start () {
        backButton = finishPanel.transform.GetChild(1).gameObject.GetComponent<Button>();
        backButton.onClick.AddListener(Back);
    }
	
	// Update is called once per frame
	void Update () {
		if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            speedText.text = string.Format("{0} mph", (int)(rb.velocity.magnitude * 10.0f));
            switch (playerPosition)
            {
                case 1:
                    suffix = "st";
                    break;
                case 2:
                    suffix = "nd";
                    break;
                case 3:
                    suffix = "rd";
                    break;
                default:
                    suffix = "th";
                    break;
            }
            positionText.text = string.Format("{0}{1}", playerPosition, suffix);
        }
        int lap = player.GetComponent<Car2D>().lap;
        if (lap < 1)
        {
            lap = 1;
        }
        else if (lap > 3)
        {
            lap = 3;
        }
        lapText.text = string.Format("Lap {0}/3", lap);
	}

    public void StartCountdown(float seconds) {
        countdownFinished = false;
        StartCoroutine(Countdown(seconds));
    }

    IEnumerator Countdown(float seconds) {
        float max = seconds;
        while (seconds > 0.0f)
        {
            CDGauge.fillAmount = seconds / max;
            countdownText.text = string.Format("{0}.<size={1}>{2}",
                System.Math.Truncate(seconds),
                countdownText.fontSize / 2,
                seconds.ToString("F2").Substring(2));
            seconds -= Time.deltaTime;
            yield return null;
        }
        countdownText.text = "<b>GO!<b>";
        countdownFinished = true;
        float fadeTime = 1.0f;
        float alpha = 1.0f;
        max = fadeTime;
        while (fadeTime > 0.0f)
        {
            alpha = fadeTime / max;
            countdownText.alpha = alpha;
            fadeTime -= Time.deltaTime;
            yield return null;
        }
        Destroy(countdownText.transform.parent.gameObject);
    }

    public void RaceFinished() {
        finishPanel.gameObject.SetActive(true);
        TextMeshProUGUI finishText = finishPanel.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        finishText.text = string.Format("You finished {0}{1} place!", playerPosition, suffix);
    }

    private void Back() {
        SceneManager.LoadScene(0);
    }

    public bool HasCountdownFinished() {
        return countdownFinished;
    }
}
