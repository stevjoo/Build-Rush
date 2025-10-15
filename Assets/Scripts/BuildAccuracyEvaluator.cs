using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class BuildAccuracyEvaluator : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private string targetFile = "target_level"; // tanpa .json
    [SerializeField] private bool autoUpdate = true; // realtime evaluasi
    [SerializeField] private float updateInterval = 1f; // update interval kalau autoUpdate nyala

    private LevelData targetData; // data level (target bangunan)
    private float timer = 0f; // timer untuk trigger update
    private float lastAccuracy = 0f; // simpan hasil evaluasi terakhir

    void Start()
    {
        // Load data target saat mulai
        LoadTargetLevel(targetFile);
    }

    void Update()
    {

        // matikan kalau tidak perlu
        if (!autoUpdate || targetData == null || gridManager == null)
            return;

        // tiap frame cek timer, kalau sudah lewat interval, hitung ulang akurasi
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f; // reset timer
            lastAccuracy = CalculateBestMatchAccuracy(); // hitung akurasi terbaik
            Debug.Log($"Realtime Accuracy: {lastAccuracy:0.0}%"); // cek hasil di console
        }
    }


    // Load target level dari file JSON
    public void LoadTargetLevel(string fileName)
    {
        string path = Path.Combine(Application.dataPath, fileName + ".json");
        if (!File.Exists(path))
        {
            Debug.LogError("Target file not found: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        targetData = JsonUtility.FromJson<LevelData>(json);
        Debug.Log($"Loaded target level: {fileName} with {targetData.blocks.Count} blocks");
    }


    // Cek akurasi bangunan pemain terhadap target
    public float CalculateBestMatchAccuracy()
    {
        if (targetData == null || gridManager == null)
        {
            Debug.LogWarning("Missing target data or GridManager reference!");
            return 0f;
        }

        var playerBlocks = gridManager.placedBlocks.Keys.ToList();
        if (playerBlocks.Count == 0)
            return 0f;

        // Gunakan Dictionary untuk lookup cepat pada data target
        var targetDictionary = targetData.blocks.ToDictionary(b => b.position, b => b.blockIndex);

        float bestAccuracy = 0f;

        foreach (var rotation in GetRotations())
        {
            var rotatedPlayerBlocks = playerBlocks.Select(rotation).ToList();

            // Pilih satu blok acuan dari pemain (misal blok pertama)
            Vector3Int playerAnchor = rotatedPlayerBlocks[0];

            // Hitung semua kemungkinan offset berdasarkan setiap blok target
            var offsets = targetDictionary.Keys.Select(targetPos => targetPos - playerAnchor);

            // Gunakan HashSet untuk menghindari perulangan offset yang sama
            var uniqueOffsets = new HashSet<Vector3Int>(offsets);

            foreach (var offset in uniqueOffsets)
            {
                int matchCount = 0;
                foreach (var playerPos in rotatedPlayerBlocks)
                {
                    Vector3Int shiftedPos = playerPos + offset;

                    // Cek kecocokan menggunakan Dictionary
                    if (targetDictionary.TryGetValue(shiftedPos, out int targetBlockType))
                    {
                        if (gridManager.blockIndices.TryGetValue(playerPos, out int playerBlockType))
                        {
                            if (playerBlockType == targetBlockType)
                            {
                                matchCount++;
                            }
                        }
                    }
                }

                float accuracy = (float)matchCount / targetDictionary.Count;
                if (accuracy > bestAccuracy)
                    bestAccuracy = accuracy;
            }
        }

        return bestAccuracy * 100f;
    }

    // function untuk rotasi posisi blok, ini membuat list fungsi yang berisi daftar rotasi yang mungkin terjadi
    private List<System.Func<Vector3Int, Vector3Int>> GetRotations()
    {
        return new List<System.Func<Vector3Int, Vector3Int>>
        {
            p => p,                                // 0°
            p => new Vector3Int(p.z, p.y, -p.x),   // +90°
            p => new Vector3Int(-p.x, p.y, -p.z),  // +180°
            p => new Vector3Int(-p.z, p.y, p.x)    // +270°
        };
    }

    // --- Opsional: manual panggil dari UI ---
    public void EvaluateNow()
    {
        float acc = CalculateBestMatchAccuracy();
        Debug.Log($"📊 Manual Accuracy Check: {acc:0.0}%");
    }
}