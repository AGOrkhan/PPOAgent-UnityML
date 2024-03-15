using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.AI;

public class GridManager : MonoBehaviour
{
    [SerializeField] public GridSystem gridSystem;
    [SerializeField] private Renderer floorRenderer;
    [SerializeField] private GameObject obstaclePrefab, targetPrefab;
    [SerializeField] private enum MapType
    {
        RandomGen,
        WallGen,
        MazeGen
    }
    [SerializeField] private MapType currentMap;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private int obstacleAmount = 10, targetAmount = 1;
    [HideInInspector] public int rows, cols;




    private List<Vector3> obstaclePositions = new List<Vector3>();
    private List<Vector3> targetPositions = new List<Vector3>();

    private void Awake()
    {
        // Set grid based on floor size
        Vector3 originPosition = transform.position;
        rows = 0;
        cols = 0;

        if (floorRenderer != null)
        {
                Vector3 size = floorRenderer.bounds.size;
                originPosition = floorRenderer.bounds.min;

                // Calculate the number of rows and columns based on the size of the environmentPrefab
                rows = Mathf.FloorToInt(size.x / cellSize);
                cols = Mathf.FloorToInt(size.z / cellSize);
        }

        gridSystem = new GridSystem(rows, cols, cellSize, originPosition);
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
            // Place objects on the grid by randomly initiating a cell position
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
                GameObject newObj = Instantiate(obstaclePrefab, worldPosition, Quaternion.identity);
                gridSystem.MarkOccupied(randomCellPosition, newObj);
                obstaclePositions.Add(worldPosition);
            }
        }
    }

    public void WallPlace()
    {
        // Determine the number of walls to place
        for (int i = 0; i < obstacleAmount; i++)
        {
            // Determine the orientation, position and length of the wall
            bool isHorizontal = Random.value > 0.5f;
            int length = Random.Range(1, isHorizontal ? (rows-rows/2) : (cols-cols/2));
            int startRow = Random.Range(0, isHorizontal ? rows : rows - length + 1);
            int startCol = Random.Range(0, isHorizontal ? cols - length + 1 : cols);

            // Instantiate the wall
            for (int j = 0; j < length; j++)
            {
                Vector3Int cellPosition = new Vector3Int(startRow + (isHorizontal ? 0 : j), 0, startCol + (isHorizontal ? j : 0));
                Vector3 worldPosition = gridSystem.CellToWorld(cellPosition);
                worldPosition.y = 0f;
                if (!gridSystem.CellOccupuation(cellPosition))
                {
                    GameObject wall = Instantiate(obstaclePrefab, worldPosition, Quaternion.identity);
                    gridSystem.MarkOccupied(cellPosition, wall);
                    obstaclePositions.Add(worldPosition);
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
            // Place objects on the grid by randomly initiating a cell position
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
                GameObject newObj = Instantiate(targetPrefab, worldPosition, Quaternion.identity);
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
                Destroy(gridSystem.GetOccupant(cellPosition));
                gridSystem.MarkEmpty(cellPosition);
            }
            obstaclePositions.Clear();
        }

        if (targetPositions.Count > 0)
        {
            foreach (Vector3 targetPosition in targetPositions)
            {
                Vector3Int cellPosition = gridSystem.WorldToCell(targetPosition);
                Destroy(gridSystem.GetOccupant(cellPosition));
                gridSystem.MarkEmpty(cellPosition);
            }
            targetPositions.Clear();
        }
    }

    public void ClearTarget(Vector3 targetPosition)
    {
        // Clear targets when found
        Vector3Int cellPosition = gridSystem.WorldToCell(targetPosition);
        GameObject targetObject = gridSystem.GetOccupant(cellPosition);
        if (targetObject != null)
        {
            Destroy(targetObject);
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

