using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.Barracuda;

public class GridBasedHider : Agent
{   
    // Grid Related
    [SerializeField] private GridManager gridManager;
    // Agent Related
    Rigidbody rb;
    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private float rotationSpeed = 100f;
    
    private Vector3Int oldCellPosition;

    void Start()
    {
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // Reset agent position and rotation
        gridManager.ClearPrefabs();
        gridManager.gridSystem.ResetVisitation();
        transform.localPosition = new Vector3(0, 0, 0);
        transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        // Let GridManager handle obstacle and target generation
        gridManager.GeneratePrefabs();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Add agent's y rotation as an observation
        sensor.AddObservation(transform.rotation.eulerAngles.y / 360.0f);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        // Transform movement
        float moveAmount = actionBuffers.ContinuousActions[0];
        transform.localPosition += transform.forward * moveAmount * moveSpeed * Time.deltaTime;

        // Rotation action
        float rotationAmount = actionBuffers.ContinuousActions[1];
        transform.Rotate(0, rotationAmount * rotationSpeed * Time.deltaTime, 0);


        /* Rb Movement action
        float moveAmount = actionBuffers.ContinuousActions[0];
        Vector3 newPosition = transform.position + transform.forward * moveAmount * moveSpeed * Time.deltaTime;
        rb.MovePosition(newPosition);

        // Rb Rotation action
        float rotationAmount = actionBuffers.ContinuousActions[1];
        Quaternion newRotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, rotationAmount * rotationSpeed * Time.deltaTime, 0));
        rb.MoveRotation(newRotation);
        */

        // Cell Marking
        Vector3Int cellPosition = gridManager.gridSystem.WorldToCell(transform.position);

        // Check if the cell has been visited before
        if (gridManager.gridSystem.HasBeenVisited(cellPosition))
        {
            if (cellPosition != oldCellPosition)
            {
                // Penalty for visiting a previously visited cell
                AddReward(-0.5f);
            }
        }
        else{
            // Mark the cell as visited
            gridManager.gridSystem.MarkVisited(cellPosition);
        }
        oldCellPosition = cellPosition;

        // Penaly for each step
        AddReward(-0.0025f);
    }

    private void OnTriggerEnter(Collider collided)
    {
        // Check if the collider belongs to a target
        if (collided.gameObject.layer == LayerMask.NameToLayer("Target"))
        {
            // Debug.Log("Target Found");

            SetReward(1.0f);
            gridManager.ColourChange(Color.green);
            // Use it when multiple targets are present
            // gridManager.ClearTarget(collided.gameObject.transform.position);
            EndEpisode();
        }
        else if (collided.gameObject.layer == LayerMask.NameToLayer("ColliderWall"))
        {   
            // Debug.Log("Wall Hit");
            SetReward(-1.0f);
            gridManager.ColourChange(Color.red);
            EndEpisode();
        }
    }
}
