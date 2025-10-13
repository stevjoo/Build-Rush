using UnityEngine;
using System.Collections.Generic; // Untuk pakai Dictionary

[System.Serializable]
public class BlockType
{
    public string name;         // Nama blok (misal: "Wood", "Stone")
    public GameObject prefab;   // Prefab blok
    //public Color color = Color.white; // Warna blok, opsional
}

public class GridManager : MonoBehaviour
{

    // Ukuran grid dunia
    public int gridSize = 100; // ukuran world grid 100x100x100

    // Tipe blok yang tersedia
    public BlockType[] blockTypes;

 
    // Simpan pair posisi : GameObject
    private Dictionary<Vector3Int, GameObject> placedBlocks = new();

    // Simpan pair posisi : tipe blok (index)
    private Dictionary<Vector3Int, int> blockIndices = new();



    // Pilihan index blok yang aktif
    public int selectedIndex = 0;



    // placing block di posisi tertentu
    public bool PlaceBlock(Vector3Int gridPos)
    {
        // Cek apakah posisi sudah ada blocknya
        if (placedBlocks.ContainsKey(gridPos))
            return false;

        // Cek apakah posisi di dalam batas grid, center gridnya di (0,0,0)
        if (Mathf.Abs(gridPos.x) >= gridSize / 2 || Mathf.Abs(gridPos.y) >= gridSize / 2 || Mathf.Abs(gridPos.z) >= gridSize / 2)
            return false;

        // Instantiate block baru dan simpan di dictionary
        GameObject newBlock = Instantiate(blockTypes[selectedIndex].prefab, gridPos, Quaternion.identity);
        placedBlocks[gridPos] = newBlock;
        blockIndices[gridPos] = selectedIndex;

        return true;
    }



    // remove block di posisi tertentu
    public bool RemoveBlock(Vector3Int gridPos)
    {
        // Cek apakah posisi ada blocknya
        if (!placedBlocks.ContainsKey(gridPos))
            return false;

        // Hapus block dari scene dan dictionary
        Destroy(placedBlocks[gridPos]);
        placedBlocks.Remove(gridPos);
        blockIndices.Remove(gridPos);
        return true;
    }



    // Cek apakah ada block di posisi tertentu
    public bool HasBlock(Vector3Int gridPos)
    {
        return placedBlocks.ContainsKey(gridPos);
    }

    public int GetBlockType(Vector3Int gridPos)
    {
        if (!blockIndices.ContainsKey(gridPos))
            return -1; // Tidak ada blok

        return blockIndices[gridPos]; // Mengembalikan index tipe blok
    }
}
