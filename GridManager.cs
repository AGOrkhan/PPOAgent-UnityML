using UnityEngine;
using System.Collections.Generic;
using Unity.Barracuda;

public class GridManager : MonoBehaviour
{   
    // Prefabs
    [SerializeField] public GridSystem gridSystem;
    [SerializeField] private Renderer floorRenderer;
    [SerializeField] private GameObject obstaclePrefab, targetPrefab;
    [SerializeField] private int obstacleAmount = 10, targetAmount = 1;


    // Map Gen
    [SerializeField] private enum MapType
    {
        RandomGen,
        WallGen,
        MazeGen
    }
    
    [SerializeField] private MapType currentMap;

    // Agent Choice
    public enum AgentType
    {
        Seeker,
        Hider,
        SelfPlay
    }
    public AgentType currentAgent;

    public int radius = 1;

    // Grid Related
    [SerializeField] private float cellSize = 1f;
    private int rows, cols;
    private int padding = 1;
    
    // Lists to store the positions of the objects
    private List<Vector3> obstaclePositions = new List<Vector3>();
    private List<Vector3> targetPositions = new List<Vector3>();

    // Initialize the grid system
    private void Awake()
    {
        // Set grid based on floor size
        Vector3 originPosition = transform.position;
        rows = 0;
        cols = 0;

        if (floorRenderer != null)
        {
                Vector3 size = floorRenderer.bounds.size;
                originPosition = floorRenderer.bounds.min + new Vector3(padding, 0, padding);

                // Calculate the number of rows and columns based on the size of the environmentPrefab
                rows = Mathf.FloorToInt(size.x / cellSize) - padding * 2;
                cols = Mathf.FloorToInt(size.z / cellSize) - padding * 2;
        }
        gridSystem = new GridSystem(rows, cols, cellSize, originPosition);
        Debug.Log("Grid System Initialized with " + rows + " rows and " + cols + " columns. Overall cells: " + rows * cols);
    
        // Initialize the empty list
        gridSystem.EmptyCellsList();
    }

    // Initialize the object pools
    public void Start()
    {
        ObjectPooling.Instance.InitializePools(obstaclePrefab, targetPrefab, obstacleAmount, targetAmount);
    }

    void Update()
    {
        obstacleAmount = Mathf.Clamp(obstacleAmount, 0, rows * cols);
    }

    public void GeneratePrefabs()
    {   
        switch (currentMap)
        {
            case MapType.RandomGen:
                RandomPlace();
                break;
            case MapType.WallGen:
                WallPlace();
                break;
            case MapType.MazeGen:
                MazeGen();
                break;
        }
        if (currentAgent == AgentType.Seeker)
        {
            TargetPlace();
        }
    }

    public void ClearRadius(Vector3Int cellPosition)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int z = -radius; z <= radius; z++)
            {
                Vector3Int nearbyCell = new Vector3Int(cellPosition.x + x, cellPosition.y, cellPosition.z + z);
                if (nearbyCell.x >= 0 && nearbyCell.z >= 0 && nearbyCell.x < rows && nearbyCell.z < cols)
                {   
                    GameObject occupant = gridSystem.GetOccupant(nearbyCell);
                    if (occupant != null)
                    {
                        // Deactivate the occupant and return it to the object pool
                        ObjectPooling.Instance.ReturnObstacle(occupant);
                        
                        // Mark the cell as empty
                        gridSystem.MarkEmpty(nearbyCell);

                        // Remove the cell from the obstaclePositions list
                        Vector3 worldPosition = gridSystem.CellToWorld(nearbyCell);
                        obstaclePositions.Remove(worldPosition);
                    }
                }
            }
        }
    }

    public void RandomPlace()
    {
        for (int i = 0; i < obstacleAmount; i++)
        {
            Vector3Int cellPosition = gridSystem.RandomCellPos();
            Vector3 worldPosition = gridSystem.CellToWorld(cellPosition);
            worldPosition.y = 0f;

            // If the cell position is valid
            if (cellPosition.x >= 0 && cellPosition.z >= 0)
            {
                GameObject newObj = ObjectPooling.Instance.GetObstacle();
                newObj.transform.position = worldPosition;
                newObj.SetActive(true);
                gridSystem.MarkOccupied(cellPosition, newObj);
                obstaclePositions.Add(worldPosition);
            }
        }
    }
    


    public void WallPlace()
    {
        // For Wall Generation
        
    }

    public void MazeGen()
    {
        // For Maze Generation
    }

    public void TargetPlace()
    {
        for (int i = 0; i < targetAmount; i++)
        {
            Vector3Int cellPosition = gridSystem.RandomCellPos();
            Vector3 worldPosition = gridSystem.CellToWorld(cellPosition);
            worldPosition.y = 0f;

            if (cellPosition.x >= 0 && cellPosition.z >= 0)
            {
                GameObject newObj = ObjectPooling.Instance.GetTarget(targetPrefab, targetAmount);
                newObj.transform.position = worldPosition;
                newObj.SetActive(true);
                gridSystem.MarkOccupied(cellPosition, newObj);
                targetPositions.Add(worldPosition);
            }
        }
    }
    
    public void ClearPrefabs()
    {
        // Clear everything on episode start
        if (obstaclePositions.Count > 0)
        {
            foreach (Vector3 obstaclePosition in obstaclePositions)
            {
                Vector3Int cellPosition = gridSystem.WorldToCell(obstaclePosition);
                GameObject obstacle = gridSystem.GetOccupant(cellPosition);
                if (obstacle != null)
                {
                    ObjectPooling.Instance.ReturnObstacle(obstacle);
                    gridSystem.MarkEmpty(cellPosition);
                }
            }
            obstaclePositions.Clear();
        }

        if (targetPositions.Count > 0)
        {
            foreach (Vector3 targetPosition in targetPositions)
            {
                Vector3Int cellPosition = gridSystem.WorldToCell(targetPosition);
                GameObject target = gridSystem.GetOccupant(cellPosition);
                if (target != null)
                {   
                    ObjectPooling.Instance.ReturnTarget(target);
                    gridSystem.MarkEmpty(cellPosition);
                }
            }
            targetPositions.Clear();
        }
    }

    public void ClearTarget(Vector3 targetPosition)
    {
        // Clear targets when found
        Vector3Int cellPosition = gridSystem.WorldToCell(targetPosition);
        GameObject target = gridSystem.GetOccupant(cellPosition);
        if (target != null)
        {
            target.SetActive(false);
            ObjectPooling.Instance.ReturnTarget(target);
            targetPositions.Remove(targetPosition);
            gridSystem.MarkEmpty(cellPosition);
        }
    }
    
    public void ColourChange(Color Colour){
        floorRenderer.material.color = Colour;
        Invoke(nameof(ColourReset), 0.5f);
    }

    public void ColourReset()
    {
        floorRenderer.material.color = Color.gray;
    }
}

