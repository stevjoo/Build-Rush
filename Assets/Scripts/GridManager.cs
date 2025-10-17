using UnityEngine;
using System.Collections.Generic; // Untuk pakai Dictionary

[System.Serializable]
public class BlockType
{
    public string name;         // Nama blok (misal: "Wood", "Stone")
    public GameObject prefab;   // Prefab blok
}


// Struktur data untuk menyimpan informasi blok
[System.Serializable]
public class BlockData
{
    public Vector3Int position;
    public int blockIndex;
}

// Struktur data untuk menyimpan 1 level
[System.Serializable]
public class LevelData
{
    public List<BlockData> blocks = new List<BlockData>();
}


public class GridManager : MonoBehaviour
{

    // Ukuran grid dunia
    public int gridSize = 100; // ukuran world grid 100x100x100

    // Tipe blok yang tersedia
    public BlockType[] blockTypes;



    // Private variable, hanya bisa diakses di gridmanager
    // Simpan pair posisi : GameObject
    private Dictionary<Vector3Int, GameObject> _placedBlocks = new();

    // Simpan pair posisi : tipe blok (index)
    private Dictionary<Vector3Int, int> _blockIndices = new();


    // Event untuk notify perubahan ke evaluator
    public event System.Action<Vector3Int, int> OnBlockPlaced;
    public event System.Action<Vector3Int> OnBlockRemoved;


    // Getter untuk akses dari luar
    public IReadOnlyDictionary<Vector3Int, GameObject> placedBlocks { get { return _placedBlocks; } }
    public IReadOnlyDictionary<Vector3Int, int> blockIndices { get { return _blockIndices; } }


    // Pilihan index blok yang aktif
    public int selectedIndex = 0;



    // placing block di posisi tertentu
    public bool PlaceBlock(Vector3Int gridPos)
    {
        // Cek apakah posisi sudah ada blocknya
        if (_placedBlocks.ContainsKey(gridPos))
            return false;

        // Cek apakah posisi di dalam batas grid, center gridnya di (0,0,0)
        if (Mathf.Abs(gridPos.x) >= gridSize / 2 || Mathf.Abs(gridPos.y) >= gridSize / 2 || Mathf.Abs(gridPos.z) >= gridSize / 2)
            return false;

        // Instantiate block baru dan simpan di dictionary
        GameObject newBlock = Instantiate(blockTypes[selectedIndex].prefab, gridPos, Quaternion.identity);
        _placedBlocks[gridPos] = newBlock;
        _blockIndices[gridPos] = selectedIndex;


        // Notify perubahan ke evaluator
        OnBlockPlaced?.Invoke(gridPos, selectedIndex);

        return true;
    }



    // remove block di posisi tertentu
    public bool RemoveBlock(Vector3Int gridPos)
    {
        // Cek apakah posisi ada blocknya
        if (!_placedBlocks.ContainsKey(gridPos))
        {

            Debug.Log("No block found at: " + gridPos);
            return false;
        }
           

        // Hapus block dari scene dan dictionary
        Destroy(_placedBlocks[gridPos]);
        _placedBlocks.Remove(gridPos);
        _blockIndices.Remove(gridPos);


        // Notify perubahan ke evaluator
        OnBlockRemoved?.Invoke(gridPos);

        return true;
    }



    // Cek apakah ada block di posisi tertentu
    public bool HasBlock(Vector3Int gridPos)
    {
        return _placedBlocks.ContainsKey(gridPos);
    }

    public int GetBlockType(Vector3Int gridPos)
    {
        if (!_blockIndices.ContainsKey(gridPos))
            return -1; // Tidak ada blok

        return _blockIndices[gridPos]; // Mengembalikan index tipe blok
    }

    // Hapus seluruh data block saat ini
    public void ClearInternalDictionaries()
    {
        _placedBlocks.Clear();
        _blockIndices.Clear();
    }



    // Save
    public void SaveLevel(string fileName)
    {
        LevelData level = new LevelData();
        foreach (var kvp in _placedBlocks)
        {
            level.blocks.Add(new BlockData { position = kvp.Key, blockIndex = _blockIndices[kvp.Key] });
        }

        string json = JsonUtility.ToJson(level, true);
        System.IO.File.WriteAllText(Application.dataPath + "/" + fileName + ".json", json);
        Debug.Log("Level saved!");
    }

    // Load
    public void LoadLevel(string fileName)
    {
        string path = Application.dataPath + "/" + fileName + ".json";
        if (!System.IO.File.Exists(path)) return;

        string json = System.IO.File.ReadAllText(path);
        LevelData level = JsonUtility.FromJson<LevelData>(json);

        // Bersihkan block lama
        foreach (var kvp in _placedBlocks.Values)
            Destroy(kvp);
        _placedBlocks.Clear();
        _blockIndices.Clear();

        // Spawn block baru
        foreach (var b in level.blocks)
        {
            selectedIndex = b.blockIndex;
            PlaceBlock(b.position);
        }
        Debug.Log("Level loaded!");
    }


}
