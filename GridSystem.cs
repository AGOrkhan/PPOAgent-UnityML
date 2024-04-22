using UnityEngine;
using System.Collections.Generic;

public class GridSystem
{
    public int rows, cols;
    public float cellSize;
    private GameObject[,] grid;
    public bool[,] visitedCells;
    public List<Vector3Int> emptyCells;

    public Vector3 originPosition;
    public GridSystem(int rows, int cols, float cellSize, Vector3 originPosition)
    
    {
        this.rows = rows;
        this.cols = cols;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        grid = new GameObject[rows, cols];
        visitedCells = new bool[rows, cols];
        emptyCells = new List<Vector3Int>();
    }


    // Keeping count of empty cells 
    public void EmptyCellsList()
    {
        emptyCells.Clear();
        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < cols; z++)
            {
                Vector3Int cellPosition = new Vector3Int(x, 0, z);

                // If the cell is not occupied, add it to the list
                if (grid[x, z] == null)
                {
                    emptyCells.Add(cellPosition);
                }
            }
        }
    }

    public Vector3Int WorldToCell(Vector3 worldPosition)
    {
        Vector3 localPosition = worldPosition - originPosition;
        int x = Mathf.RoundToInt(localPosition.x / cellSize);
        int z = Mathf.RoundToInt(localPosition.z / cellSize);
        return new Vector3Int(x, 0, z);
    }

    public Vector3 CellToWorld(Vector3Int cellPosition)
    {
        Vector3 localPosition = new Vector3(cellPosition.x * cellSize, 0f, cellPosition.z * cellSize);
        return localPosition + originPosition;
    }

    public Vector3Int RandomCellPos()
    {
        if (emptyCells.Count == 0)
        {
            // No empty cells available
            return new Vector3Int(-1, -1, -1);
        }

        // Choose a random index
        int randomIndex = Random.Range(0, emptyCells.Count);

        // Return the cell position at the random index
        return emptyCells[randomIndex];
    }

    public void MarkOccupied(Vector3Int cellPosition, GameObject obj)
    {
        if (cellPosition.x >= 0 && cellPosition.x < cols && cellPosition.z >= 0 && cellPosition.z < rows)
        {
            if (grid[cellPosition.x, cellPosition.z] != null)
            {
                Debug.LogWarning("Cell is already occupied");
            }
            else
            {
                grid[cellPosition.x, cellPosition.z] = obj;
                // Remove the cell from the emptyCells list
                emptyCells.Remove(cellPosition);
            }
        }
    }
    
    public void MarkEmpty(Vector3Int cellPosition)
    {
        if (cellPosition.x >= 0 && cellPosition.x < rows && cellPosition.z >= 0 && cellPosition.z < cols)
        {
            grid[cellPosition.x, cellPosition.z] = null;
            emptyCells.Add(cellPosition);
        }
    }

    // May be redundant
    public bool CellOccupuation(Vector3Int cellPosition)
    {
        return grid[cellPosition.x, cellPosition.z] != null;
    }

    public GameObject GetOccupant(Vector3Int cellPosition)
    {   
        if (cellPosition.x >= 0 && cellPosition.x < rows && cellPosition.z >= 0 && cellPosition.z < cols)
        {
            return grid[cellPosition.x, cellPosition.z];
        }
        return null;
    } 

    public void MarkVisited(Vector3Int cellPosition)
    {
        if (cellPosition.x >= 0 && cellPosition.x < rows && cellPosition.z >= 0 && cellPosition.z < cols)
        {
            visitedCells[cellPosition.x, cellPosition.z] = true;
        }
    }

    public bool HasBeenVisited(Vector3Int cellPosition)
    {   
        if (cellPosition.x >= 0 && cellPosition.x < rows && cellPosition.z >= 0 && cellPosition.z < cols){
            return visitedCells[cellPosition.x, cellPosition.z];
        }
        return false;
    }
    public void ResetVisitation()
    {
        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < cols; z++)
            {
                visitedCells[x, z] = false;
            }
        }
    }
}
