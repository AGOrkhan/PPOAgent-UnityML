using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class GridBasedSeeker : Agent
{   
    // Grid Related
    private GridManager gridManager; 
    private HiderAgent hiderAgent;

    // Agent Related
    Rigidbody rb; // Not used currently

    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private float rotationSpeed = 100f;
    private float moveAmount;
    private Vector3Int respawnPoint;
    
    private Vector3Int oldCellPosition;

    void Start()
    {
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();
        
        // Get the GridManager component
        gridManager = GetComponentInParent<GridManager>();

        // Get the HiderAgent component
        hiderAgent = transform.parent.GetComponentInChildren<HiderAgent>();
    }


    public override void OnEpisodeBegin()
    {   
        switch (gridManager.currentAgent)
        {
            case GridManager.AgentType.Seeker:
                SeekerControl();
                break;
            case GridManager.AgentType.Hider:
                HiderControl();
                break;
            case GridManager.AgentType.SelfPlay:
                SeekerControl();
                break;
        }
    }

    private void SeekerControl()
    {
        /* Reset agent position and rotation
        transform.localPosition = new Vector3(0, 0, 0);
        transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        
        // Clear Radius around player
        Vector3Int cellPosition = gridManager.gridSystem.WorldToCell(transform.position);
        gridManager.ClearRadius(cellPosition);
        */

        // Reset prefabs
        gridManager.ClearPrefabs();
        gridManager.gridSystem.ResetVisitation();

        // Get a random empty cell position for the start of the agent
        Vector3Int cellPosition = gridManager.gridSystem.RandomCellPos();
        Vector3 worldPosition = gridManager.gridSystem.CellToWorld(cellPosition);
        transform.position = new Vector3(worldPosition.x, 0, worldPosition.z);

        // Set a random rotation
        transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        // Let GridManager handle obstacle and target generation
        gridManager.GeneratePrefabs();
        Debug.Log("SeekerControl spawned obstacles and target");

        // Clear radius for both agents
        gridManager.ClearRadius(gameObject);
    }

    private void HiderControl()
    {
        // Reset visited cells
        gridManager.gridSystem.ResetVisitation();

        // Get a random empty cell position for the start of the agent
        Vector3Int cellPosition = gridManager.gridSystem.RandomCellPos();
        Vector3 worldPosition = gridManager.gridSystem.CellToWorld(cellPosition);
        respawnPoint = cellPosition;    
        transform.position = new Vector3(worldPosition.x, 0, worldPosition.z);
        transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        gridManager.ClearRadius(gameObject);
    }

    public void LightReset()
    {
        // Reset agent to its spawn point
        Vector3 worldPosition = gridManager.gridSystem.CellToWorld(respawnPoint);
        transform.position = new Vector3(worldPosition.x, 0, worldPosition.z);
        transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
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
        Vector3 newPosition = transform.position + transform.forward * moveAmount * Time.deltaTime * (moveSpeed / Time.timeScale);
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
        oldCellPosition = cellPosition;

        // Penaly for each step
        AddReward(-1f / MaxStep);
    }

    private void OnCollisionEnter(Collision collided)
    {
        // Agent penalties 
        if (gridManager.currentAgent == GridManager.AgentType.Seeker)
        {
            // Check if the collider belongs to a target
            if (collided.gameObject.layer == LayerMask.NameToLayer("Target"))
            {
                // Debug.Log("Target Found");
                // Use it when multiple targets are present
                // gridManager.ClearTarget(collided.gameObject.transform.position);
                gridManager.ColourChange(Color.green);
                SetReward(1.0f);
                EndEpisode();
            }
            else if (collided.gameObject.layer == LayerMask.NameToLayer("ColliderWall"))
            {   
                // Debug.Log("Wall Hit");
                gridManager.ColourChange(Color.red);
                SetReward(-1.0f);
                EndEpisode();
            }
        }
        // Agent resetting for hider training
        else if (gridManager.currentAgent == GridManager.AgentType.Hider)
        {
            if (collided.gameObject.layer == LayerMask.NameToLayer("ColliderWall"))
            {
                LightReset();
            }
        }
        else if (gridManager.currentAgent == GridManager.AgentType.SelfPlay)
        {
            if (collided.gameObject.layer == LayerMask.NameToLayer("Target"))
            {  
                gridManager.ColourChange(Color.green);
                SetReward(1.0f);
                hiderAgent.SetReward(-1.0f);
                hiderAgent.EndEpisode();
                EndEpisode();
            }
            else if (collided.gameObject.layer == LayerMask.NameToLayer("ColliderWall"))
            {   
                AddReward(-0.25f);
                LightReset();
            }
        }
    }
}
