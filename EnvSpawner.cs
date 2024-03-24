using UnityEngine;

public class EnvSpawner : MonoBehaviour
{
    [SerializeField] private GameObject environmentPrefab;
    public int environmentAmount = 9;

    private void Start()
    {
        int environmentRows = Mathf.FloorToInt(Mathf.Sqrt(environmentAmount));
        int environmentCols = Mathf.CeilToInt((float)environmentAmount / environmentRows);

        Renderer floorRenderer = environmentPrefab.transform.Find("Floor").GetComponent<Renderer>();
        Vector3 size = floorRenderer.bounds.size;
        float SpacingX = size.x + 2;
        float SpacingZ = size.z + 2;

        for (int i = 0; i < environmentAmount; i++)
        {
            float x = (i % environmentCols) * SpacingX;
            float z = (i / environmentCols) * SpacingZ;
            Vector3 position = new Vector3(x, 0, z);

            Instantiate(environmentPrefab, position, Quaternion.identity);
        }
    }
}