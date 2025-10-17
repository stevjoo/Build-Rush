using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor.ShaderGraph;

public class BuilderController : MonoBehaviour
{
    [Header("UI")] 
    public TextMeshProUGUI modeText;
    public Image[] selectionOutlines;
    public Sprite outlineActiveSprite;
    public Sprite NormalSprite;

    [Header("References")] public GridManager gridManager;
    [Header("Block Placing")] public float behindOffset = 1.3f;
    public float minDistanceAfterDirChange = 1f;
    public float verticalPlaceOffset = 1.2f;
    public float minVerticalJumpHeight = 0.8f;

    [Header("Block Breaking")] public float breakCooldown = 1f;
    public float frontBreakDistance = 1f;

    private Vector3Int lastPlacedPos = new(int.MinValue, int.MinValue, int.MinValue);
    private Vector3Int lastBrokenPos = new(int.MinValue, int.MinValue, int.MinValue);

    private MovementController moveController;
    private CharacterController charController;

    private float breakCooldownTimer = 0f;
    private Vector3 lastPlayerPosition;
    private Vector3Int lastMoveDir;
    private float moveSinceDirChange;
    private bool wasGroundedLastFrame = true;
    private float jumpStartY = 0f;

    private enum BuildMode { None, Place, Break, PlaceAbove }
    private BuildMode currentMode = BuildMode.None;

    void Start()
    {
        moveController = GetComponent<MovementController>();
        charController = GetComponent<CharacterController>();
        lastPlayerPosition = transform.position;
    }

    void Update()
    {
        HandleBlockSelection();
        HandleModeSwitch();

        breakCooldownTimer = Mathf.Max(0f, breakCooldownTimer - Time.deltaTime);

        if (moveController == null || charController == null) return;

        bool isGrounded = charController.isGrounded;
        HandleJumpLandTransition(isGrounded);
        wasGroundedLastFrame = isGrounded;

        switch (currentMode)
        {
            case BuildMode.Place:
                HandlePlaceMode(isGrounded);
                break;
            case BuildMode.Break:
                HandleBreakMode(isGrounded);
                break;
            case BuildMode.PlaceAbove:
                TryPlaceAbovePlayer();
                break;
        }

        lastPlayerPosition = transform.position;
    }

    #region Helpers
    private void HandleBlockSelection()
    {
        for (int i = 0; i < 10; i++)
        {
            KeyCode key = KeyCode.Alpha0 + i;
            if (!Input.GetKeyDown(key)) continue;

            int index = (i == 0) ? 9 : i - 1;
            if (index < gridManager.blockTypes.Length && index < selectionOutlines.Length)
            {
                gridManager.selectedIndex = index;
                Debug.Log("Selected block: " + gridManager.blockTypes[index].name);
                UpdateSelectionUI();
            }
        }
    }

    private void HandleModeSwitch()
    {
        if (Input.GetKeyDown(KeyCode.E)) ToggleMode(BuildMode.Place);
        else if (Input.GetKeyDown(KeyCode.Q)) ToggleMode(BuildMode.Break);
        else if (Input.GetKeyDown(KeyCode.R)) ToggleMode(BuildMode.PlaceAbove);
        else if (Input.GetKeyDown(KeyCode.BackQuote)) currentMode = BuildMode.None;
    }

    private void ToggleMode(BuildMode mode)
    {
        currentMode = currentMode == mode ? BuildMode.None : mode;
        Debug.Log("Builder mode switched to: " + currentMode);
        UpdateModeUI();
    }

    private void HandleJumpLandTransition(bool isGrounded)
    {
        if (!wasGroundedLastFrame && isGrounded) jumpStartY = 0f;
        else if (wasGroundedLastFrame && !isGrounded) jumpStartY = transform.position.y;
    }

    private void HandlePlaceMode(bool isGrounded)
    {
        if (!isGrounded) { TryPlaceVerticalBelow(); return; }

        Vector3Int moveDir = moveController.moveDirectionGrid;
        if (moveDir == Vector3Int.zero) return;

        if (moveDir != lastMoveDir) { moveSinceDirChange = 0f; lastMoveDir = moveDir; }

        moveSinceDirChange += Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                               new Vector3(lastPlayerPosition.x, 0, lastPlayerPosition.z));

        Vector3Int blockPos = GetOffsetBlockPosition(moveDir, behindOffset);
        if (moveSinceDirChange >= minDistanceAfterDirChange && blockPos != lastPlacedPos && !gridManager.HasBlock(blockPos))
        {
            if (gridManager.PlaceBlock(blockPos)) { lastPlacedPos = blockPos; moveSinceDirChange = 0f; }
        }
    }

    private void HandleBreakMode(bool isGrounded)
    {
        Vector3Int moveDir = moveController.moveDirectionGrid;
        if (moveDir != Vector3Int.zero) TryBreakFront(moveDir);
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S) && isGrounded) TryBreakBelow();
    }

    private Vector3Int GetOffsetBlockPosition(Vector3Int moveDir, float offset)
    {
        Vector3 worldOffset = Vector3.zero;
        if (moveDir.x != 0) worldOffset = new Vector3(-moveDir.x * offset, 0, 0);
        else if (moveDir.z != 0) worldOffset = new Vector3(0, 0, -moveDir.z * offset);

        Vector3 pos = transform.position + worldOffset;
        return new Vector3Int(Mathf.FloorToInt(pos.x + 0.5f), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z + 0.5f));
    }

    private void TryPlaceAbovePlayer()
    {
        Vector3Int pos = new Vector3Int(
            Mathf.FloorToInt(transform.position.x + 0.5f),
            Mathf.FloorToInt(transform.position.y + 2f),
            Mathf.FloorToInt(transform.position.z + 0.5f)
        );

        if (pos == lastPlacedPos || gridManager.HasBlock(pos)) return;

        if (gridManager.PlaceBlock(pos)) lastPlacedPos = pos;
    }

    private void TryPlaceVerticalBelow()
    {
        if (transform.position.y - jumpStartY < minVerticalJumpHeight) return;

        Vector3Int pos = new Vector3Int(
            Mathf.FloorToInt(transform.position.x + 0.5f),
            Mathf.FloorToInt(transform.position.y - verticalPlaceOffset),
            Mathf.FloorToInt(transform.position.z + 0.5f)
        );

        if (pos == lastPlacedPos || gridManager.HasBlock(pos)) return;

        if (gridManager.PlaceBlock(pos)) lastPlacedPos = pos;
    }

    private void TryBreakFront(Vector3Int dir)
    {
        Vector3 origin = transform.position + Vector3.up * 0.15f;
        if (Physics.Raycast(origin, new Vector3(dir.x, 0, dir.z), out RaycastHit hit, frontBreakDistance))
        {
            Vector3Int pos = Vector3Int.RoundToInt(hit.collider.transform.position);
            if (breakCooldownTimer <= 0f && pos != lastBrokenPos && gridManager.HasBlock(pos))
            {
                gridManager.RemoveBlock(pos);
                lastBrokenPos = pos;
                breakCooldownTimer = 0.2f;
            }
        }
    }

    private void TryBreakBelow()
    {
        Vector3 belowPos = transform.position + Vector3.down * verticalPlaceOffset;
        Vector3Int pos = new(Mathf.FloorToInt(belowPos.x + 0.5f),
                              Mathf.FloorToInt(belowPos.y),
                              Mathf.FloorToInt(belowPos.z + 0.5f));

        if (gridManager.HasBlock(pos)) { gridManager.RemoveBlock(pos); breakCooldownTimer = breakCooldown; }
    }

    private void UpdateModeUI()
    {
        if(modeText != null)
            modeText.text = "Mode: " + currentMode.ToString();
    }

    private void UpdateSelectionUI()
    {
        if(selectionOutlines == null || gridManager == null) return;

        for (int i = 0; i < selectionOutlines.Length; i++)
        {
            if (selectionOutlines[i] == null) continue;

            if (i == gridManager.selectedIndex)
            {
                selectionOutlines[i].sprite = outlineActiveSprite;
            }
            else
            {
                selectionOutlines[i].sprite = NormalSprite;
            }
        }
    }
    #endregion
}