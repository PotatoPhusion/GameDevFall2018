using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car2DController : MonoBehaviour {

    private Car2D car;

	// Use this for initialization
	void Start () {
        car = gameObject.GetComponent<Car2D>();
	}
	
	void FixedUpdate () {
        float throttle = Input.GetAxisRaw("Throttle") - Input.GetAxisRaw("Brake/Reverse");
        float steer = Input.GetAxisRaw("Horizontal");

        if (throttle != 0f)
        {
            car.Accelerate(throttle);
        }

        if (steer != 0f)
        {
            car.Steer(steer);
        }
	}
}
