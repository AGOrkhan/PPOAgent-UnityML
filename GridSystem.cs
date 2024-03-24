using UnityEngine;
public class GridSystem
{
    public int rows, cols;
    public float cellSize;
    private GameObject[,] grid;
    public bool[,] visitedCells;

    public Vector3 originPosition;
    public GridSystem(int rows, int cols, float cellSize, Vector3 originPosition)
    
    {
        this.rows = rows;
        this.cols = cols;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        grid = new GameObject[rows, cols];
        visitedCells = new bool[rows, cols];

        /*
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Vector3 cellPosition = originPosition + new Vector3(i * cellSize, 0, j * cellSize);
                // Optionally, instantiate a visual representation of the grid here
            }
        }
        */
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

    public void MarkOccupied(Vector3Int cellPosition, GameObject obj)
    {
        if (cellPosition.x >= 0 && cellPosition.x < cols && cellPosition.z >= 0 && cellPosition.z < rows)
        {
            grid[cellPosition.x, cellPosition.z] = obj;
        }
    }
    public void MarkEmpty(Vector3Int cellPosition)
    {
        if (cellPosition.x >= 0 && cellPosition.x < rows && cellPosition.z >= 0 && cellPosition.z < cols)
        {
            grid[cellPosition.x, cellPosition.z] = null;
        }
    }
    public bool CellOccupuation(Vector3Int cellPosition)
    {
        return grid[cellPosition.x, cellPosition.z] != null;
    }

    public GameObject GetOccupant(Vector3Int cellPosition)
    {
        return grid[cellPosition.x, cellPosition.z];
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
