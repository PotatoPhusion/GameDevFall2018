using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarInfoPanel : MonoBehaviour {

    public GameObject infoPanelPrefab;
    public GameObject canvas;

    public float offset;

    private GameObject infoPanel;

	// Use this for initialization
	void Start () {
        canvas = GameObject.Find("Canvas");
        infoPanel = Instantiate(infoPanelPrefab, canvas.transform, false);
	}
	
	// Update is called once per frame
	void Update () {
        // Update UI Element position
        infoPanel.transform.position = Camera.main.WorldToScreenPoint((Camera.main.transform.up * offset) + transform.position);

        // Update the throttle bar
        float throttle = transform.GetComponent<Car2DTrainer>().throttle;
        Image throttleBar = infoPanel.transform.GetChild(0).transform.GetChild(0).GetComponent<Image>();

        if (throttle > 0.0f)
        {
            throttleBar.color = new Color(0.0f, 1.0f, 0.0f);
        }
        else
        {
            throttleBar.color = new Color(1.0f, 0.0f, 0.0f);
        }

        throttleBar.fillAmount = Mathf.Abs(throttle);
	}

    private void OnDestroy() {
        if (infoPanel != null)
            Destroy(infoPanel.gameObject);
    }
}
