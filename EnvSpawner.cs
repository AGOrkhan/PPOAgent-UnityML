using UnityEngine;

public class EnvSpawner : MonoBehaviour
{   

    [SerializeField] private GameObject environmentPrefab;
    [SerializeField] private GameObject seekerPrefab;
    [SerializeField] private GameObject hiderPrefab;
    public int environmentAmount = 9;
    public int spacing = 2;

    private void Awake()
    {
        
        int environmentRows = Mathf.FloorToInt(Mathf.Sqrt(environmentAmount));
        int environmentCols = Mathf.CeilToInt((float)environmentAmount / environmentRows);

        Renderer floorRenderer = environmentPrefab.transform.Find("Floor").GetComponent<Renderer>();
        Vector3 size = floorRenderer.bounds.size;
        float SpacingX = size.x + spacing;
        float SpacingZ = size.z + spacing;

        for (int i = 0; i < environmentAmount; i++)
        {
            float x = (i % environmentCols) * SpacingX;
            float z = (i / environmentCols) * SpacingZ;
            Vector3 position = new Vector3(x, 0, z);

            GameObject environment = Instantiate(environmentPrefab, position, Quaternion.identity);

            // Get the GridManager component from the instantiated environment
            GridManager gridManager = environment.GetComponent<GridManager>();

            // Instantiate the agents as a child of the environment
            Instantiate(seekerPrefab, environment.transform);
            
            if (gridManager.currentAgent == GridManager.AgentType.Hider || gridManager.currentAgent == GridManager.AgentType.SelfPlay)
            {
                Instantiate(hiderPrefab, environment.transform);
            }
        }
    }
}