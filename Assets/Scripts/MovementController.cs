using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementController : MonoBehaviour
{
    public float speed = 6.0f;
    public float gravity = -9.81f;
    public Transform cameraTransform;

    [HideInInspector] public Vector3 moveDirection; // world space
    [HideInInspector] public Vector3Int moveDirectionGrid; // snapped direction (x,z)
    [HideInInspector] public bool movementLocked = false;

    public float jumpHeight = 1.5f;

    private CharacterController characterController;
    private Vector3 velocity;

    // === Tambahan untuk SFX ===
    private bool isWalking = false;
    private float stepCooldown = 0.4f; // waktu jeda antar langkah
    private float stepTimer = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (movementLocked) return;

        HandleMovement();
        UpdateMoveDirectionGrid();
    }

    void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 rawInput = new Vector3(x, 0, z);

        if (rawInput.magnitude < 0.01f)
        {
            moveDirection = Vector3.zero;
            isWalking = false; // berhenti jalan → stop bunyi
        }
        else
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 targetDir = camRight * rawInput.x + camForward * rawInput.z;

            if (Mathf.Abs(targetDir.x) > Mathf.Abs(targetDir.z))
                moveDirection = new Vector3(Mathf.Sign(targetDir.x), 0, 0);
            else
                moveDirection = new Vector3(0, 0, Mathf.Sign(targetDir.z));

            // Rotasi karakter
            if (moveDirection.magnitude > 0.01f)
            {
                float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, targetAngle, 0);
            }

            isWalking = true; // sedang bergerak
        }

        // --- Jump & Gravity ---
        if (characterController.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (Input.GetButtonDown("Jump") && characterController.isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;

        // Gabungkan horizontal + vertikal
        Vector3 finalMove = moveDirection * speed + new Vector3(0, velocity.y, 0);
        characterController.Move(finalMove * Time.deltaTime);

        // === Jalankan suara langkah ===
        HandleFootstepSFX();
    }

    void HandleFootstepSFX()
    {
        if (isWalking && characterController.isGrounded)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                AudioManager.Instance.PlayWalkSFX();
                stepTimer = stepCooldown; // reset jeda langkah
            }
        }
    }

    void UpdateMoveDirectionGrid()
    {
        moveDirectionGrid = new Vector3Int(
            Mathf.RoundToInt(moveDirection.x),
            0,
            Mathf.RoundToInt(moveDirection.z)
        );
    }
}
