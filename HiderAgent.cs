using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.IO;

public class HiderAgent : Agent
{   
    // Grid Related
    private GridManager gridManager;
    private GridBasedSeeker seekerAgent;
    

    // Agent Related
    Rigidbody rb; // Not used currently

    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private float rotationSpeed = 100f;
    private float moveAmount;

    void Start()
    {
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();

        // Get the GridManager component
        gridManager = GetComponentInParent<GridManager>();

        // Get the SeekerAgent component
        seekerAgent = transform.parent.GetComponentInChildren<GridBasedSeeker>();
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
        }
    }
    
    private void HiderControl()
    {
        // Reset prefabs
        gridManager.ClearPrefabs();

        //* Get a random empty cell position for the start of the agent
        Vector3Int cellPosition = gridManager.gridSystem.RandomCellPos();
        Vector3 worldPosition = gridManager.gridSystem.CellToWorld(cellPosition);
        transform.position = new Vector3(worldPosition.x, 0, worldPosition.z);

        // Set a random rotation
        transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        
        // Clear Radius around agent
        gridManager.ClearRadius(cellPosition);

        Vector3Int seekerCellPosition = gridManager.gridSystem.WorldToCell(seekerAgent.transform.position);
        gridManager.ClearRadius(seekerCellPosition);

        // Let GridManager handle obstacle and target generation
        gridManager.GeneratePrefabs();
    }

    private void SeekerControl()
    {
        // Get a random empty cell position for the start of the agent
        Vector3Int cellPosition = gridManager.gridSystem.RandomCellPos();
        Vector3 worldPosition = gridManager.gridSystem.CellToWorld(cellPosition);
        transform.position = new Vector3(worldPosition.x, 0, worldPosition.z);

        // Set a random rotation
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

        // Add agent's rotation as an observation
        sensor.AddObservation(transform.rotation.eulerAngles.y / 360.0f);

        // Add agent's movement as an observation
        sensor.AddObservation((moveAmount + 1) / 2.0f);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Rb Movement action
        moveAmount = Mathf.Max(0, actionBuffers.ContinuousActions[0]);
        Vector3 newPosition = transform.position + transform.forward * moveAmount * moveSpeed * Time.deltaTime;
        rb.MovePosition(newPosition);

        // Rb Rotation action
        float rotationAmount = actionBuffers.ContinuousActions[1];
        Quaternion newRotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, rotationAmount * rotationSpeed * Time.deltaTime, 0));
        rb.MoveRotation(newRotation);

        // Reward for staying alive 
        AddReward(0.001f);
    }

    private void OnCollisionEnter(Collision collided)
    {
        if (gridManager.currentAgent == GridManager.AgentType.Hider)
        {
            // Check if the collider belongs to a target
            if (collided.gameObject.layer == LayerMask.NameToLayer("Seeker"))
            {
                // Debug.Log("Target Found");
                // Use it when multiple targets are present
                // gridManager.ClearTarget(collided.gameObject.transform.position);

                gridManager.ColourChange(Color.red);
                SetReward(1.0f);
                seekerAgent.EndEpisode();
                EndEpisode();
            }
            else if (collided.gameObject.layer == LayerMask.NameToLayer("ColliderWall"))
            {   
                // Debug.Log("Wall Hit");
                gridManager.ColourChange(Color.red);
                SetReward(-1.0f);
                seekerAgent.EndEpisode();
                EndEpisode();
            }
        }
    }
}
