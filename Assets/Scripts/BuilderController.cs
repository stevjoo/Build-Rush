using UnityEngine;

public class BuilderController : MonoBehaviour
{
    [Header("References")]
    public GridManager gridManager;

    [Header("Settings")]
    public bool placingActive = true;
    public float behindOffset = 1.3f;

    // Simpan posisi block terakhir yang dipasang untuk menghindari duplikasi
    private Vector3Int lastPlacedPos;
    private MovementController moveController;

    // Simpan arah gerak terakhir untuk deteksi perubahan arah
    private Vector3Int lastMoveDir;
    private float moveSinceDirChange = 0f;
    public float minDistanceAfterDirChange = 1f; // minimal jarak sebelum bisa menaruh block


    // Untuk handle jumping (cek grounded)
    private CharacterController charController;
    private bool wasGroundedLastFrame = true;


    // Untuk menghitung perpindahan aktual pemain
    private Vector3 lastPlayerPosition;
    // threshold kecil untuk mengabaikan jitter
    private const float kMoveEpsilon = 0.005f;

    void Start()
    {
        moveController = GetComponent<MovementController>();
        if (moveController == null)
            Debug.LogError("MovementController not found on this GameObject!");


        charController = GetComponent<CharacterController>();

        lastPlacedPos = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
        lastMoveDir = Vector3Int.zero;
        lastPlayerPosition = transform.position;
    }

    void Update()
    {
        // Toggle placing
        if (Input.GetKeyDown(KeyCode.E))
            placingActive = !placingActive;

        if (!placingActive || moveController == null || charController == null)
        {
            // update lastPlayerPosition anyway to avoid spike when re-enabled
            lastPlayerPosition = transform.position;
            return;
        }


        // --- Cek status grounded ---
        bool isGrounded = charController.isGrounded;

        // Jika baru mendarat dari udara, maka reset jarak
        if (!wasGroundedLastFrame && isGrounded)
        {
            moveSinceDirChange = 0f;
            lastPlayerPosition = transform.position;
        }

        wasGroundedLastFrame = isGrounded;

        // Kalau sedang di udara, skip placing block
        if (!isGrounded)
        {
            lastPlayerPosition = transform.position;
            return;
        }


        // Gunakan snapped movement grid (4 arah)
        Vector3Int moveDir = moveController.moveDirectionGrid;
        if (moveDir == Vector3Int.zero)
        {
            // tidak gerak -> reset lastPlayerPosition supaya hitungan perpindahan mulai dari sini saat bergerak lagi
            lastPlayerPosition = transform.position;
            return;
        }

        // --------------- detect direction change ----------------
        if (moveDir != lastMoveDir)
        {
            // arah berubah: reset counter (tidak langsung place)
            moveSinceDirChange = 0f;
            lastMoveDir = moveDir;
            // reset lastPlayerPosition agar pengukuran perpindahan sejak pergantian arah dimulai dari sini
            lastPlayerPosition = transform.position;
        }

        // --------------- hitung perpindahan aktual pemain ---------------
        // hitung hanya pada bidang XZ (horizontal)
        Vector3 currentPlayerPos = transform.position;
        Vector3 lastPosXZ = new Vector3(lastPlayerPosition.x, 0f, lastPlayerPosition.z);
        Vector3 currPosXZ = new Vector3(currentPlayerPos.x, 0f, currentPlayerPos.z);
        float actualMoved = Vector3.Distance(currPosXZ, lastPosXZ);

        if (actualMoved > kMoveEpsilon)
        {
            moveSinceDirChange += actualMoved;
        }

        // update lastPlayerPosition untuk frame selanjutnya
        lastPlayerPosition = currentPlayerPos;

        // ---------------- snap & placement ----------------
        // Snap player ke grid integer (center pada +0.5)
        Vector3 playerPos = transform.position + new Vector3(0.5f, 0f, 0.5f);
        Vector3Int playerGridPos = new Vector3Int(
            Mathf.FloorToInt(playerPos.x),
            Mathf.FloorToInt(playerPos.y),
            Mathf.FloorToInt(playerPos.z)
        );

        // Tentukan offset block di belakang player (float)
        Vector3 offset = Vector3.zero;
        if (Mathf.Abs(moveDir.x) > 0)
            offset = new Vector3(-moveDir.x * behindOffset, 0f, 0f);
        else if (Mathf.Abs(moveDir.z) > 0)
            offset = new Vector3(0f, 0f, -moveDir.z * behindOffset);

        // Posisi block di world space
        Vector3 blockWorldPos = transform.position + offset;

        // Snap ke grid
        Vector3Int blockGridPos = new Vector3Int(
            Mathf.FloorToInt(blockWorldPos.x + 0.5f),
            Mathf.FloorToInt(blockWorldPos.y),
            Mathf.FloorToInt(blockWorldPos.z + 0.5f)
        );

        // Pasang blok jika sudah bergerak cukup jauh sejak pergantian arah dan belum ada block di sana
        if (moveSinceDirChange >= minDistanceAfterDirChange && blockGridPos != lastPlacedPos && !gridManager.HasBlock(blockGridPos))
        {
            if (gridManager.PlaceBlock(blockGridPos))
            {
                lastPlacedPos = blockGridPos;
                moveSinceDirChange = 0f; // reset counter setelah menaruh block
                // juga reset lastPlayerPosition agar jarak dihitung ulang dari posisi saat ini
                lastPlayerPosition = transform.position;
                Debug.Log("Block placed at: " + blockGridPos);
            }
        }
    }
}
