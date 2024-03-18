using UnityEngine;

public class TargetHider : MonoBehaviour
{
    private GridSystem gridSystem;
    private Rigidbody rb;
    private Vector3 randomDirection;
    private float targetDistance;
    private float distanceMoved;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
        {
            // Calculate the new position
            Vector3 moveVector = randomDirection * Time.deltaTime;
            Vector3 newPosition = transform.position + moveVector;

            // Move the target to the new position
            rb.MovePosition(newPosition);

            // Update the distance moved
            distanceMoved += moveVector.magnitude;

            // If the target has moved the target distance, choose a new direction and distance
            if (distanceMoved >= targetDistance)
            {
                // Define the four possible directions the target can move
                Vector3[] directions = new Vector3[] { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

                randomDirection = directions[Random.Range(0, directions.Length)];
                targetDistance = Random.Range(1.0f, 10.0f);

                // Reset the distance moved
                distanceMoved = 0.0f;
            }
        }
    }
