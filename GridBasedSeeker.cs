using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class GridBasedSeeker : Agent
{   
    // Grid Related
    [SerializeField] private GridManager gridManager;
    [SerializeField] private int radius = 2;   

    // Agent Related
    Rigidbody rb; // Not used currently

    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private float rotationSpeed = 100f;
    private float moveAmount;
    
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

        // Radius for around player
        Vector3Int cellPosition = gridManager.gridSystem.WorldToCell(transform.position);
        for (int x = -radius; x <= radius; x++)
        {
            for (int z = -radius; z <= radius; z++)
            {
                Vector3Int nearbyCell = new Vector3Int(cellPosition.x + x, cellPosition.y, cellPosition.z + z);
                gridManager.gridSystem.MarkOccupied(nearbyCell, transform.gameObject);
            }
        }

    
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
        /* Make a 1D array of visited cells
        List<float> visitedArray = new List<float>();
        for (int i = 0; i < gridManager.gridSystem.rows; i++)
            {
            for (int j = 0; j < gridManager.gridSystem.cols; j++)
            {
                visitedArray.Add(gridManager.gridSystem.visitedCells[i, j] ? 1.0f : 0.0f);
            }
        }
        // Add visited cells as an observation
        sensor.AddObservation(visitedArray);
        */

        // Add agent's rotation as an observation
        sensor.AddObservation(transform.rotation.eulerAngles.y / 360.0f);

        // Add agent's movement as an observation
        sensor.AddObservation((moveAmount + 1) / 2.0f);

        // Add agent's cell position as an observation
        Vector3Int cellPosition = gridManager.gridSystem.WorldToCell(transform.position);
        sensor.AddObservation((float)cellPosition.x / gridManager.gridSystem.cols);
        sensor.AddObservation((float)cellPosition.z / gridManager.gridSystem.rows);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        /* Transform movement
        float moveAmount = actionBuffers.ContinuousActions[0];
        transform.localPosition += transform.forward * moveAmount * moveSpeed * Time.deltaTime;

        // Rotation action
        float rotationAmount = actionBuffers.ContinuousActions[1];
        transform.Rotate(0, rotationAmount * rotationSpeed * Time.deltaTime, 0);
        */

        // Rb Movement action
        moveAmount = Mathf.Max(0, actionBuffers.ContinuousActions[0]);
        Vector3 newPosition = transform.position + transform.forward * moveAmount * moveSpeed * Time.deltaTime;
        rb.MovePosition(newPosition);

        // Rb Rotation action
        float rotationAmount = actionBuffers.ContinuousActions[1];
        Quaternion newRotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, rotationAmount * rotationSpeed * Time.deltaTime, 0));
        rb.MoveRotation(newRotation);

        // Cell Marking
        Vector3Int cellPosition = gridManager.gridSystem.WorldToCell(transform.position);

        // Check if the cell has been visited before
        if (gridManager.gridSystem.HasBeenVisited(cellPosition))
        {   
            if (cellPosition != oldCellPosition)
            {   
                // Penalty for visiting a previously visited cell
                AddReward(-0.01f);
            }

        }
        else{
            // Mark the cell as visited
            gridManager.gridSystem.MarkVisited(cellPosition);
        }
        // oldCellPosition = cellPosition;

        // Penaly for each step
        if (MaxStep > 0)
        {
            AddReward(1 / MaxStep);
        }
        else
        {
            AddReward(-0.0025f);
        }
    }

    private void OnCollisionEnter(Collision collided)
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
