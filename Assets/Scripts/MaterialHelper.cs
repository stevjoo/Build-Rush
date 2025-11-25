using UnityEngine;

public class MaterialHelper : MonoBehaviour
{
    [SerializeField] private Shader baseShader; // assign shader dari inspector, misal "Universal Render Pipeline/Lit"
    [HideInInspector] public Material runtimeMaterial;

    void Awake()
    {
        if (baseShader == null)
        {
            Debug.LogWarning("Base shader belum di-assign, pakai Standard shader default.");
            baseShader = Shader.Find("Standard");
        }

        // Buat material baru runtime
        runtimeMaterial = new Material(baseShader);

        // Aktifkan GPU instancing
        runtimeMaterial.enableInstancing = true;

        // Optional: tweak warna / properti lain
        runtimeMaterial.color = Color.white;

        Debug.Log("Runtime material siap pakai untuk instancing!");
    }
}