using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car2D : MonoBehaviour {

    public float maxSpeed;
    public float acceleration;
    public float steeringPower;
    public AnimationCurve steering;
    [HideInInspector]
    public int lastCheckpoint;
    [HideInInspector]
    public int lap;
    //[HideInInspector]
    public float trackProgress;
    private GameObject[] checkpoints;
    private int nextCheckpoint = 1;

    private Rigidbody2D rb;

	// Use this for initialization
	void Awake () {
        rb = gameObject.GetComponent<Rigidbody2D>();
        lap = 1;
	}
	
	// Update is called once per frame
	void Update () {

	}

    void FixedUpdate() {
        if (rb.velocity.magnitude > maxSpeed)
        {
            Vector2 setSpeed = rb.velocity.normalized * maxSpeed;
            rb.velocity = setSpeed;
        }

        Vector2 forward = new Vector2(0.0f, 0.5f);
        float steeringRightAngle;
        if (rb.angularVelocity > 0)
        {
            steeringRightAngle = -90;
        }
        else
        {
            steeringRightAngle = 90;
        }

        Vector2 rightAngleFromForward = Quaternion.AngleAxis(steeringRightAngle, Vector3.forward) * forward;
        Debug.DrawLine((Vector3)rb.position, (Vector3)rb.GetRelativePoint(rightAngleFromForward), Color.yellow);

        float driftForce = Vector2.Dot(rb.velocity, rb.GetRelativeVector(rightAngleFromForward.normalized));

        Vector2 relativeForce = (rightAngleFromForward.normalized * -1.0f) * (driftForce * 10.0f);


        Debug.DrawLine((Vector3)rb.position, (Vector3)rb.GetRelativePoint(relativeForce), Color.red);

        rb.AddForce(rb.GetRelativeVector(relativeForce));
    }

    public void Accelerate(float throttle) {
        //transform.position += transform.up * throttle * moveSpeed * Time.deltaTime;
        float force = throttle * acceleration * rb.mass;
        rb.AddForce(transform.up * force, ForceMode2D.Force);
    }

    public void Steer(float rot) {
        float speedRatio = rb.velocity.magnitude / maxSpeed;
        float direction = Vector2.Dot(rb.velocity, rb.GetRelativeVector(Vector2.up));
        if (direction >= 0.0f)
        {
            rb.rotation += -rot * steeringPower * steering.Evaluate(speedRatio);
        }
        else
        {
            rb.rotation -= -rot * steeringPower * steering.Evaluate(speedRatio);
        }
    }

    public void SetCheckpoints(GameObject[] checkpoints) {
        this.checkpoints = checkpoints;
    }

    public void SetTrackProgress(float progress) {
        trackProgress = progress;
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
                int totalCheckpoints = checkpoints.Length;
                lastCheckpoint = nextCheckpoint;
                nextCheckpoint = (nextCheckpoint + 1) % totalCheckpoints;
                if (lastCheckpoint == 0 && nextCheckpoint == 1)
                {
                    lap++;
                }
            }
        }
    }
}
