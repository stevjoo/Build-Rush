using Unity.VisualScripting;
using UnityEngine;

public class BuilderController : MonoBehaviour
{
    [Header("References")]
    public GridManager gridManager;

    [Header("Block Placing Settings")]
    public bool placingActive = false; // toggle untuk aktifkan mode place
    public float behindOffset = 1.3f; // jarak minimal placement di belakang player
    public float minDistanceAfterDirChange = 1f; // jarak minimal placement setelah arah player berubah
    public float verticalPlaceOffset = 1.2f; // offset vertikal saat place di udara
    public float minVerticalJumpHeight = 0.8f; // tinggi minimal sebelum boleh place di udara


    [Header("Block Breaking Settings")]
    public float breakCooldown = 1f; // jeda antar break 
    public float frontBreakDistance = 1f; // jarak deteksi block di depan


    // --- VARIABEL INTERNAL ---
    private Vector3Int lastPlacedPos; // Simpan posisi terakhir kali block di-place
    private MovementController moveController; // referensi ke MovementController player
    private CharacterController charController; // referensi ke CharacterController player

    private float breakCooldownTimer = 0f; // timer cooldown break
    private Vector3Int lastBrokenPos = new Vector3Int(int.MinValue, int.MinValue, int.MinValue); // posisi terakhir break

    private Vector3Int lastMoveDir; // arah gerak terakhir
    private float moveSinceDirChange = 0f; // jarak tempuh sejak arah berubah
    private Vector3 lastPlayerPosition; // posisi player terakhir frame sebelumnya
    private const float kMoveEpsilon = 0.005f; // threshold untuk deteksi gerakan

    private bool wasGroundedLastFrame = true; // untuk deteksi transisi grounded -> air atau sebaliknya
    private float jumpStartY = 0f; // catat posisi Y saat mulai lompat



    // --- Build Mode ---
    private enum BuildMode { None, Place, Break, PlaceAbove } // mode build yang bisa dipilih
    private BuildMode currentMode = BuildMode.None; // mode build saat ini
    


    void Start()
    {
        moveController = GetComponent<MovementController>();
        if (moveController == null)
            Debug.LogError("MovementController not found!");

        charController = GetComponent<CharacterController>();
        if (charController == null)
            Debug.LogError("CharacterController not found!");

        lastPlacedPos = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
        lastMoveDir = Vector3Int.zero;
        lastPlayerPosition = transform.position;
    }

    void Update()
    {

        // --- SWITCH BLOCK berdasarkan number key 1..0 ---
        for (int i = 0; i < 10; i++)
        {
            KeyCode key = KeyCode.Alpha0 + i;
            if (Input.GetKeyDown(key))
            {
                int index = (i == 0) ? 9 : i - 1; // Alpha0 = block ke-10 (index 9)
                if (index < gridManager.blockTypes.Length)
                {
                    gridManager.selectedIndex = index;
                    Debug.Log("Selected block: " + gridManager.blockTypes[index].name);
                }
            }
        }


        // --- toggle mode (E untuk place, Q untuk break, ` atau tekan ulang mode saat ini untuk reset None) ---
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentMode == BuildMode.Place)
            {
                currentMode = BuildMode.None;
            }
            else
            {
                currentMode = BuildMode.Place;
            }

            Debug.Log("Builder mode switched to: " + currentMode);
        }

        else if (Input.GetKeyDown(KeyCode.Q))
        {
            if (currentMode == BuildMode.Break)
            {
                currentMode = BuildMode.None;
            }
            else
            {
                currentMode = BuildMode.Break;
            }

            Debug.Log("Builder mode switched to: " + currentMode);
        }

        else if (Input.GetKeyDown(KeyCode.R))
        {
            if (currentMode == BuildMode.PlaceAbove)
            {
                currentMode = BuildMode.None;
            }
            else
            {
                currentMode = BuildMode.PlaceAbove;
            }
        }

        else if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            currentMode = BuildMode.None;
            Debug.Log("Builder mode switched to: " + currentMode);
        }


        // --- update cooldown timer ---
        if (breakCooldownTimer > 0f)
            breakCooldownTimer -= Time.deltaTime;


        // --- Handle kalau navController atau charController kosong ---
        if (moveController == null || charController == null)
        {
            lastPlayerPosition = transform.position;
            return;
        }



        // --- --- --- --- --- ---
        // MODE: NONE
        // --- --- --- --- --- ---
        if (currentMode == BuildMode.None)
        {
            lastPlayerPosition = transform.position;
            return;
        }


        // --- TRANSISI: baru mendarat dari lompat ---
        bool isGrounded = charController.isGrounded; // cek kondisi grounded sekarang
        if (!wasGroundedLastFrame && isGrounded) // transisi dari udara ke tanah
        {
            moveSinceDirChange = 0f; // reset jarak tempuh
            lastPlayerPosition = transform.position; // update posisi terakhir
            jumpStartY = 0f; // reset catatan lompat // TODO: nanti ubah ini
        }

        // --- TRANSISI: baru mulai lompat ---
        else if (wasGroundedLastFrame && !isGrounded)
        {
            jumpStartY = transform.position.y;
        }

        wasGroundedLastFrame = isGrounded;

        // --- --- --- --- --- ---
        // MODE: PLACE
        // --- --- --- --- --- ---
        if (currentMode == BuildMode.Place)
        {
            if (!isGrounded)
            {
                TryPlaceVerticalBelow();
                return;
            }

            Vector3Int moveDir = moveController.moveDirectionGrid;
            if (moveDir == Vector3Int.zero)
            {
                lastPlayerPosition = transform.position;
                return;
            }

            if (moveDir != lastMoveDir)
            {
                moveSinceDirChange = 0f;
                lastMoveDir = moveDir;
                lastPlayerPosition = transform.position;
            }

            Vector3 currentPlayerPos = transform.position;
            Vector3 lastPosXZ = new Vector3(lastPlayerPosition.x, 0f, lastPlayerPosition.z);
            Vector3 currPosXZ = new Vector3(currentPlayerPos.x, 0f, currentPlayerPos.z);
            float actualMoved = Vector3.Distance(currPosXZ, lastPosXZ);

            if (actualMoved > kMoveEpsilon)
                moveSinceDirChange += actualMoved;

            lastPlayerPosition = currentPlayerPos;

            Vector3 offset = Vector3.zero;
            if (Mathf.Abs(moveDir.x) > 0)
                offset = new Vector3(-moveDir.x * behindOffset, 0f, 0f);
            else if (Mathf.Abs(moveDir.z) > 0)
                offset = new Vector3(0f, 0f, -moveDir.z * behindOffset);

            Vector3 blockWorldPos = transform.position + offset;
            Vector3Int blockGridPos = new Vector3Int(
                Mathf.FloorToInt(blockWorldPos.x + 0.5f),
                Mathf.FloorToInt(blockWorldPos.y),
                Mathf.FloorToInt(blockWorldPos.z + 0.5f)
            );

            if (moveSinceDirChange >= minDistanceAfterDirChange &&
                blockGridPos != lastPlacedPos &&
                !gridManager.HasBlock(blockGridPos))
            {
                if (gridManager.PlaceBlock(blockGridPos))
                {
                    lastPlacedPos = blockGridPos;
                    moveSinceDirChange = 0f;
                    lastPlayerPosition = transform.position;
                    Debug.Log("Placed (ground) at: " + blockGridPos);
                }
            }

            return;
        }

        // --- --- --- --- --- ---
        // MODE: BREAK
        // --- --- --- --- --- ---
        if (currentMode == BuildMode.Break)
        {
            Vector3Int moveDir = moveController.moveDirectionGrid;

            // --- Break block di depan player ---
            if (moveDir != Vector3Int.zero)
            {
                Vector3 origin = transform.position + Vector3.up * 0.15f; // sedikit di tengah tubuh
                Vector3 direction = new Vector3(moveDir.x, 0, moveDir.z);
                Ray ray = new Ray(origin, direction);

                // cek debug ray
                float distance = 5f;
                Debug.DrawRay(origin, direction * distance, Color.red);

                if (Physics.Raycast(ray, out RaycastHit hit, frontBreakDistance))
                {
                    Debug.Log("Ray hit point: " + hit.point + " at direction " + direction);
                    Vector3 hitPos = hit.point - direction * 0.1f;
                    Debug.Log("Ray hit: " + hit.collider.name + " at position " + hitPos);
                    Vector3Int gridPos = new Vector3Int(
                        Mathf.RoundToInt(hit.collider.transform.position.x),
                        Mathf.RoundToInt(hit.collider.transform.position.y),
                        Mathf.RoundToInt(hit.collider.transform.position.z)

                    );
                    Debug.Log("Ray hit: " + hit.collider.name + " at position " + gridPos);

                    if (breakCooldownTimer <= 0f && gridPos != lastBrokenPos && gridManager.HasBlock(gridPos))
                    {
                        gridManager.RemoveBlock(gridPos);
                        lastBrokenPos = gridPos;
                        breakCooldownTimer = 0.2f; // cooldown kecil untuk depan
                        Debug.Log("Broke (front) at: " + gridPos);
                    }
                }
            }

            // --- Break block di bawah (Ctrl + S) ---
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
            {
                if (breakCooldownTimer <= 0f && isGrounded)
                {
                    Vector3 belowPos = transform.position + Vector3.down * verticalPlaceOffset;
                    Vector3Int gridPos = new Vector3Int(
                        Mathf.FloorToInt(belowPos.x + 0.5f),
                        Mathf.FloorToInt(belowPos.y),
                        Mathf.FloorToInt(belowPos.z + 0.5f)
                    );

                    if (gridManager.HasBlock(gridPos))
                    {
                        gridManager.RemoveBlock(gridPos);
                        breakCooldownTimer = breakCooldown; // cooldown lebih panjang
                        Debug.Log("Broke (below) at: " + gridPos);
                    }
                }
            }
        }


        // --- Place Above Player
        if (currentMode == BuildMode.PlaceAbove)
        {
            TryPlaceAbovePlayer();
        }


    }

    private void TryPlaceAbovePlayer()
    {
        // Posisi di atas player
        Vector3 abovePos = transform.position + Vector3.up * 2f; // verticalPlaceOffset bisa pakai 1.5f

        Vector3Int blockGridPos = new Vector3Int(
            Mathf.FloorToInt(abovePos.x + 0.5f),
            Mathf.FloorToInt(abovePos.y),
            Mathf.FloorToInt(abovePos.z + 0.5f)
        );

        // Jangan place di posisi yang sama dengan block terakhir
        if (blockGridPos == lastPlacedPos)
            return;

        if (!gridManager.HasBlock(blockGridPos))
        {
            if (gridManager.PlaceBlock(blockGridPos))
            {
                lastPlacedPos = blockGridPos;
                Debug.Log("Placed block above player at: " + blockGridPos);
            }
        }
    }


    private void TryPlaceVerticalBelow()
    {
        float currentY = transform.position.y;
        float verticalGain = jumpStartY > 0f ? (currentY - jumpStartY) : 0f;

        // Pastikan player sudah naik cukup tinggi sebelum bisa place block
        if (verticalGain < minVerticalJumpHeight)
            return;

        Vector3 belowPos = transform.position + Vector3.down * verticalPlaceOffset;

        Vector3Int blockGridPos = new Vector3Int(
            Mathf.FloorToInt(belowPos.x + 0.5f),
            Mathf.FloorToInt(belowPos.y),
            Mathf.FloorToInt(belowPos.z + 0.5f)
        );

        if (blockGridPos == lastPlacedPos)
            return;

        if (!gridManager.HasBlock(blockGridPos))
        {
            if (gridManager.PlaceBlock(blockGridPos))
            {
                lastPlacedPos = blockGridPos;
                Debug.Log("Placed (air) below player at: " + blockGridPos);
            }
        }
    }
}
