using UnityEngine;

public class TableGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform container;

    [Header("Grid Size")]
    [Min(1)][SerializeField] private int columns = 6; // X
    [Min(1)][SerializeField] private int rows = 8;    // Z

    [Header("Spacing")]
    [SerializeField] private Vector2 spacing = new Vector2(1f, 1f);

    [Header("Rotation")]
    [Tooltip("Rotación aplicada a todos los elementos (Euler angles).")]
    [SerializeField] private Vector3 rotationEuler = new Vector3(0f, -90f, 90f);

    [Header("Rebuild")]
    [SerializeField] private bool clearBeforeBuild = true;

    private void Awake()
    {
        if (container == null)
            container = transform;
    }

    private void Start()
    {
        BuildGrid();
    }

    [ContextMenu("Build Grid")]
    public void BuildGrid()
    {
        if (prefab == null || startPoint == null)
        {
            Debug.LogError("[GridSpawnerXZ] Prefab o StartPoint no asignado.");
            return;
        }

        if (clearBeforeBuild)
            ClearGrid();

        Vector3 origin = startPoint.position;
        float baseY = origin.y;

        Quaternion rotation = Quaternion.Euler(rotationEuler);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                float x = origin.x + c * spacing.x;
                float z = origin.z + r * spacing.y;

                Vector3 pos = new Vector3(x, baseY, z);

                GameObject go = Instantiate(prefab, pos, rotation, container);
                go.name = $"{prefab.name}_{r}_{c}";
            }
        }
    }

    [ContextMenu("Clear Grid")]
    public void ClearGrid()
    {
        
    }
}
