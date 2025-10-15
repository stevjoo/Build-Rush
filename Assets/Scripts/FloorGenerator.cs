using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class OptimizedCircularMapBuilder : MonoBehaviour
{
    [Header("Prefabs")]
    public Mesh floorMesh;
    public Mesh wallMesh;
    public Material floorMaterial;
    public Material wallMaterial;

    [Header("Map Settings")]
    public int radius = 10;
    public float floorY = 0f;
    public int wallHeight = 2;

    void Start()
    {
        BuildOptimizedMap();
    }

    void BuildOptimizedMap()
    {
        CombineInstance[] floorCombines = BuildCombinedMesh(floorMesh, GetFloorPositions());
        CombineInstance[] wallCombines = BuildCombinedMesh(wallMesh, GetWallPositions());

        // --- Perbaikan untuk Floor Mesh ---
        GameObject floorObject = new GameObject("Floor");
        floorObject.transform.parent = this.transform;
        MeshFilter floorFilter = floorObject.AddComponent<MeshFilter>();
        MeshRenderer floorRenderer = floorObject.AddComponent<MeshRenderer>();
        Mesh newFloorMesh = new Mesh();
        newFloorMesh.indexFormat = IndexFormat.UInt32;
        // Parameter ketiga diubah menjadi true
        newFloorMesh.CombineMeshes(floorCombines, true, true);
        floorFilter.mesh = newFloorMesh;
        floorRenderer.material = floorMaterial;

        // --- Perbaikan untuk Wall Mesh ---
        GameObject wallObject = new GameObject("Walls");
        wallObject.transform.parent = this.transform;
        MeshFilter wallFilter = wallObject.AddComponent<MeshFilter>();
        MeshRenderer wallRenderer = wallObject.AddComponent<MeshRenderer>();
        Mesh newWallMesh = new Mesh();
        newWallMesh.indexFormat = IndexFormat.UInt32;
        // Parameter ketiga diubah menjadi true
        newWallMesh.CombineMeshes(wallCombines, true, true);
        wallFilter.mesh = newWallMesh;
        wallRenderer.material = wallMaterial;
    }

    private CombineInstance[] BuildCombinedMesh(Mesh meshToCombine, List<Vector3> positions)
    {
        CombineInstance[] combines = new CombineInstance[positions.Count];
        for (int i = 0; i < positions.Count; i++)
        {
            combines[i].mesh = meshToCombine;
            combines[i].transform = Matrix4x4.TRS(positions[i], Quaternion.identity, Vector3.one);
        }
        return combines;
    }

    private List<Vector3> GetFloorPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        for (int x = -radius; x <= radius; x++)
        {
            for (int z = -radius; z <= radius; z++)
            {
                if (x * x + z * z <= radius * radius)
                {
                    positions.Add(new Vector3(x, floorY, z));
                }
            }
        }
        return positions;
    }

    private List<Vector3> GetWallPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        float wallThickness = 1f;
        for (int x = -radius - 1; x <= radius + 1; x++)
        {
            for (int z = -radius - 1; z <= radius + 1; z++)
            {
                float dist = Mathf.Sqrt(x * x + z * z);
                if (dist >= radius - wallThickness && dist <= radius + wallThickness)
                {
                    for (int y = 0; y < wallHeight; y++)
                    {
                        positions.Add(new Vector3(x, floorY + y, z));
                    }
                }
            }
        }
        return positions;
    }
}