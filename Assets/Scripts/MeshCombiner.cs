using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class CombineChildMeshes : MonoBehaviour
{
    [Header("Optional: keep original children?")]
    public bool keepChildren = false;

    void Start()
    {
        CombineMeshesAndColliders();
    }

    public void CombineMeshesAndColliders()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();

        if (meshFilters.Length == 0)
        {
            Debug.LogWarning("No child meshes found to combine.");
            return;
        }

        // Prepare combine array
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        // Create new mesh
        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // kalau banyak vertex
        combinedMesh.CombineMeshes(combine);

        // Assign ke parent
        MeshFilter parentMeshFilter = GetComponent<MeshFilter>();
        MeshRenderer parentMeshRenderer = GetComponent<MeshRenderer>();
        MeshCollider parentCollider = GetComponent<MeshCollider>();

        parentMeshFilter.sharedMesh = combinedMesh;
        parentMeshRenderer.sharedMaterial = meshRenderers[0].sharedMaterial; // ambil material pertama
        parentCollider.sharedMesh = combinedMesh;
        parentCollider.convex = false; // sesuaikan kebutuhan physics

        // Opsional: hapus child lama
        if (!keepChildren)
        {
            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (meshFilters[i].gameObject != this.gameObject)
                    Destroy(meshFilters[i].gameObject);
            }
        }

        Debug.Log("✅ Mesh & collider combined: " + meshFilters.Length + " meshes -> 1 mesh");
    }
}