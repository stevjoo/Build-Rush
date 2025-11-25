//using UnityEditor;
//using UnityEngine;
//using System.IO;

//public class PrefabIconGenerator : EditorWindow
//{
//    public GameObject prefab;
//    private Camera iconCamera;
//    private RenderTexture renderTexture;

//    private const int RESOLUTION = 256;
//    private const string OUTPUT_FOLDER = "Assets/GeneratedIcons/";

//    [MenuItem("Tools/Generate Icon from Prefab")]
//    public static void ShowWindow()
//    {
//        GetWindow<PrefabIconGenerator>("Prefab Icon Generator");
//    }

//    private void OnGUI()
//    {
//        GUILayout.Label("Drag your 3D Prefab here", EditorStyles.boldLabel);
//        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);

//        GUILayout.Space(10);

//        if (GUILayout.Button("Generate Icon", GUILayout.Height(40)))
//        {
//            if (prefab != null)
//            {
//                GenerateIcon();
//            }
//            else
//            {
//                EditorUtility.DisplayDialog("Error", "Please assign a prefab!", "OK");
//            }
//        }
//    }

//    private void GenerateIcon()
//    {
//        if (!Directory.Exists(OUTPUT_FOLDER))
//        {
//            Directory.CreateDirectory(OUTPUT_FOLDER);
//        }

//        // Create temporary scene objects
//        GameObject tempObject = Instantiate(prefab);
//        tempObject.layer = 30; // Isolated layer

//        // Disable lighting & shadows
//        Renderer[] renderers = tempObject.GetComponentsInChildren<Renderer>();
//        foreach (Renderer r in renderers)
//        {
//            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
//            r.receiveShadows = false;
//        }

//        // Set up camera
//        iconCamera = new GameObject("IconCamera").AddComponent<Camera>();
//        iconCamera.clearFlags = CameraClearFlags.SolidColor;
//        iconCamera.backgroundColor = new Color(0, 0, 0, 0); // Transparent
//        iconCamera.orthographic = true;
//        iconCamera.orthographicSize = 1f;
//        iconCamera.cullingMask = 1 << 30;

//        // Position camera
//        Bounds bounds = CalculateBounds(tempObject);
//        iconCamera.transform.position = bounds.center + new Vector3(0, 0, -5);
//        iconCamera.transform.LookAt(bounds.center);

//        // Create RenderTexture
//        renderTexture = new RenderTexture(RESOLUTION, RESOLUTION, 24, RenderTextureFormat.ARGB32);
//        iconCamera.targetTexture = renderTexture;

//        // Render
//        iconCamera.Render();

//        // Save PNG
//        SaveRenderTextureToPNG(renderTexture);

//        // Cleanup
//        DestroyImmediate(iconCamera.gameObject);
//        DestroyImmediate(tempObject);

//        AssetDatabase.Refresh();
//        EditorUtility.DisplayDialog("Success", "Icon saved in " + OUTPUT_FOLDER, "OK");
//    }

//    private Bounds CalculateBounds(GameObject obj)
//    {
//        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
//        Bounds bounds = new Bounds(obj.transform.position, Vector3.zero);
//        foreach (Renderer r in renderers)
//        {
//            bounds.Encapsulate(r.bounds);
//        }
//        return bounds;
//    }

//    private void SaveRenderTextureToPNG(RenderTexture rt)
//    {
//        RenderTexture.active = rt;
//        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
//        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
//        tex.Apply();

//        byte[] bytes = tex.EncodeToPNG();

//        string filePath = OUTPUT_FOLDER + prefab.name + "_Icon.png";
//        File.WriteAllBytes(filePath, bytes);

//        // Set import settings to Sprite
//        AssetDatabase.Refresh();
//        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(filePath);
//        importer.textureType = TextureImporterType.Sprite;
//        importer.alphaIsTransparency = true;
//        importer.SaveAndReimport();

//        RenderTexture.active = null;
//    }
//}
