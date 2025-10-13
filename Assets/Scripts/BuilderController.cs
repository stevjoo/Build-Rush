using UnityEngine;

public class BuilderController : MonoBehaviour
{
    [Header("References")]
    public GridManager gridManager;

    [Header("Settings")]
    public bool placingActive = true;
    public float behindOffset = 1.3f;
    public float minDistanceAfterDirChange = 1f;
    public float verticalPlaceOffset = 1.2f; // posisi block di bawah player
    public float minVerticalJumpHeight = 0.8f; // tinggi minimal sebelum boleh place di udara

    private Vector3Int lastPlacedPos;
    private MovementController moveController;
    private CharacterController charController;

    private Vector3Int lastMoveDir;
    private float moveSinceDirChange = 0f;
    private Vector3 lastPlayerPosition;
    private const float kMoveEpsilon = 0.005f;

    private bool wasGroundedLastFrame = true;
    private float jumpStartY = 0f;

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
        if (Input.GetKeyDown(KeyCode.E))
            placingActive = !placingActive;

        if (!placingActive || moveController == null || charController == null)
        {
            lastPlayerPosition = transform.position;
            return;
        }

        bool isGrounded = charController.isGrounded;

        // --- TRANSISI: baru mendarat dari lompat ---
        if (!wasGroundedLastFrame && isGrounded)
        {
            moveSinceDirChange = 0f;
            lastPlayerPosition = transform.position;
            jumpStartY = 0f;
        }

        // --- TRANSISI: baru mulai lompat ---
        if (wasGroundedLastFrame && !isGrounded)
        {
            jumpStartY = transform.position.y;
        }

        wasGroundedLastFrame = isGrounded;

        if (!isGrounded)
        {
            TryPlaceVerticalBelow();
            return;
        }

        // ---------- Mode normal (grounded) ----------
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
