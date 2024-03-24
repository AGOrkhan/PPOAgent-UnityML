using UnityEngine;
using System.Collections.Generic;

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

    // Grid Related
    [SerializeField] private float cellSize = 1f;
    private int rows, cols;
    private int padding = 1;


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
        TargetPlace();
    }

    public void RandomPlace()
{
        for (int i = 0; i < obstacleAmount; i++)
        {
            Vector3Int randomCellPosition;
            Vector3 worldPosition;
            int attempts = 0;
            do
            {
                randomCellPosition = new Vector3Int(Random.Range(0, rows), 0, Random.Range(0, cols));
                worldPosition = gridSystem.CellToWorld(randomCellPosition);
                worldPosition.y = 0f;
                attempts++;
            } while (gridSystem.CellOccupuation(randomCellPosition) && attempts < 100);

            if (attempts < 100)
            {
                GameObject newObj = ObjectPooling.Instance.GetObstacle();
                newObj.transform.position = worldPosition;
                newObj.SetActive(true);
                gridSystem.MarkOccupied(randomCellPosition, newObj);
                obstaclePositions.Add(worldPosition);
            }
        }
    }


    public void WallPlace()
    {
        // Keep track of the number of obstacles used
        int obstaclesUsed = 0;

        // Determine the number of walls to place
        for (int i = 0; i < obstacleAmount; i++)
        {
            // Determine the orientation, position and length of the wall
            bool isHorizontal = Random.value > 0.5f;
            int length = Random.Range(1, isHorizontal ? (rows-rows/2) : (cols-cols/2));
            int startRow = Random.Range(0, isHorizontal ? rows : rows - length);
            int startCol = Random.Range(0, isHorizontal ? cols - length : cols);

            // Instantiate the wall
            for (int j = 0; j < length; j++)
            {
                // Stop creating walls if we've used up all the obstacles
                if (obstaclesUsed >= obstacleAmount)
                {
                    return;
                }

                Vector3Int cellPosition = new Vector3Int(startRow + (isHorizontal ? 0 : j), 0, startCol + (isHorizontal ? j : 0));
                Vector3 worldPosition = gridSystem.CellToWorld(cellPosition);
                worldPosition.y = 0f;
                if (!gridSystem.CellOccupuation(cellPosition))
                {
                    GameObject wall = ObjectPooling.Instance.GetObstacle();
                    wall.transform.position = worldPosition;
                    wall.SetActive(true);
                    
                    // Store the original scale
                    Vector3 originalScale = wall.transform.localScale;

                    // Extend the size of the wall prefab in the direction it grows (agent can't pass through it)
                    float extension = 1.5f * cellSize;
                    Vector3 scale = wall.transform.localScale;
                    if (isHorizontal)
                    {
                        scale.z *= extension;
                    }
                    else
                    {
                        scale.x *= extension;
                    }
                    wall.transform.localScale = scale;

                    gridSystem.MarkOccupied(cellPosition, wall);
                    obstaclePositions.Add(worldPosition);

                    // Increment the count of obstacles used
                    obstaclesUsed++;
                }
            }
        }
    }

    public void MazeGen()
    {
        // For Maze Generation
    }

    public void TargetPlace()
    {
        for (int i = 0; i < targetAmount; i++)
            {
                Vector3Int randomCellPosition;
                Vector3 worldPosition;
                int attempts = 0;
                do
                {
                    randomCellPosition = new Vector3Int(Random.Range(0, rows), 0, Random.Range(0, cols));
                    worldPosition = gridSystem.CellToWorld(randomCellPosition);
                    worldPosition.y = 0f;
                    attempts++;
                } while (gridSystem.CellOccupuation(randomCellPosition) && attempts < 100);

                if (attempts < 100)
                {
                    GameObject newObj = ObjectPooling.Instance.GetTarget(targetPrefab, targetAmount);
                    newObj.transform.position = worldPosition;
                    newObj.SetActive(true);
                    gridSystem.MarkOccupied(randomCellPosition, newObj);
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
                    obstacle.SetActive(false);
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
                    target.SetActive(false);
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

