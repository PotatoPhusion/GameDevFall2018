using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    private GameObject follow;
    private Car2D car;
    private Rigidbody2D rb;
    private Camera cam;

    private Vector3 zOffset;

	// Use this for initialization
	void Start () {
        zOffset = new Vector3(0.0f, 0.0f, -10.0f);
        cam = GetComponent<Camera>();
        if (follow != null)
        {
            car = follow.GetComponent<Car2D>();
            rb = follow.GetComponent<Rigidbody2D>();
        }
	}

	// Update is called once per frame
	void Update () {
        if (follow != null)
        {
            float zoom = Mathf.Lerp(5.0f, 8.0f, Mathf.Max((rb.velocity.magnitude / car.maxSpeed), 0.5f));

            cam.orthographicSize = zoom;

            transform.position = follow.transform.position + zOffset;
            transform.rotation = follow.transform.rotation;
        }
	}

    public void ChangeTarget(GameObject target) {
        follow = target;
        car = target.GetComponent<Car2D>();
        rb = target.GetComponent<Rigidbody2D>();
    }
}
