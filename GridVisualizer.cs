using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    public GridManager gridManager;
    public bool visualizeGrid = true;
    public bool visualizeVisitedCells = true;

    private void OnDrawGizmos()
    {
        if (gridManager != null && gridManager.gridSystem != null)
        {
            if (visualizeVisitedCells)
            {
                DrawVisitedCells();
            }
            if (visualizeGrid)
            {
                DrawGrid(gridManager.gridSystem.originPosition);
            }
        }
    }

    private void DrawVisitedCells()
    {
        GridSystem gridSystem = gridManager.gridSystem;

        // Loop through all the cells in the grid
        for (int x = 0; x < gridSystem.rows; x++)
        {
            for (int z = 0; z < gridSystem.cols; z++)
            {
                // Check if the current cell has been visited
                if (gridSystem.HasBeenVisited(new Vector3Int(x, 0, z)))
                {
                    // Calculate the world position of the cell
                    Vector3 cellPosition = gridSystem.CellToWorld(new Vector3Int(x, 0, z));
                    

                    // Colour change
                    Gizmos.color = Color.magenta;

                    // Draw a cube at the cell's position
                    Gizmos.DrawCube(cellPosition, new Vector3(gridSystem.cellSize * 0.9f, 0.1f, gridSystem.cellSize * 0.9f));
                }
            }
        }
    }
    private void DrawGrid(Vector3 originPosition)
    {
        GridSystem gridSystem = gridManager.gridSystem;

        Gizmos.color = Color.yellow;
        for (int x = 0; x <= gridSystem.rows; x++)
        {
            for (int z = 0; z <= gridSystem.cols; z++)
            {
                Vector3 startLineHorizontal = originPosition + new Vector3(x * gridSystem.cellSize, 0, 0);
                Vector3 endLineHorizontal = startLineHorizontal + new Vector3(0, 0, gridSystem.cols * gridSystem.cellSize);

                Vector3 startLineVertical = originPosition + new Vector3(0, 0, z * gridSystem.cellSize);
                Vector3 endLineVertical = startLineVertical + new Vector3(gridSystem.rows * gridSystem.cellSize, 0, 0);

                Gizmos.DrawLine(startLineHorizontal, endLineHorizontal);
                Gizmos.DrawLine(startLineVertical, endLineVertical);
            }
        }
    }
}
