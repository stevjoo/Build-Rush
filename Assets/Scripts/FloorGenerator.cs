using UnityEngine;

public class FloorGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;

    [Header("Map Settings")]
    public int radius = 10;       // radius map bulat
    public float floorY = 0f;     // tinggi floor
    public int wallHeight = 2;    // tinggi wall

    void Start()
    {
        BuildFloor();
        BuildWalls();
    }

    void BuildFloor()
    {
        // Buat floor tile berbentuk bulat
        for (int x = -radius; x <= radius; x++)
        {
            for (int z = -radius; z <= radius; z++)
            {
                if (x * x + z * z <= radius * radius) // check dalam lingkaran
                {
                    Vector3 pos = new Vector3(x, floorY, z);
                    Instantiate(floorPrefab, pos, Quaternion.identity, transform);
                }
            }
        } 
    }

    void BuildWalls()
    {
        float wallThickness = 0.5f; // toleransi untuk menutupi grid
        for (int x = -radius - 1; x <= radius + 1; x++)
        {
            for (int z = -radius - 1; z <= radius + 1; z++)
            {
                float dist = Mathf.Sqrt(x * x + z * z);
                if (dist >= radius - wallThickness && dist <= radius + wallThickness)
                {
                    for (int y = 0; y < wallHeight; y++)
                    {
                        Vector3 pos = new Vector3(x, floorY + y, z);
                        Instantiate(wallPrefab, pos, Quaternion.identity, transform);
                    }
                }
            }
        }
    }
}
