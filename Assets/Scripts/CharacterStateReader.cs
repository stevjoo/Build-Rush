using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    [Header("References")]
    public MovementController movement;   // drag MovementController dari inspector
    private Animator anim;
    private CharacterController controller;

    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        UpdateAnimationStates();
    }

    void UpdateAnimationStates()
    {
        // --- Kondisi Dasar ---
        bool isGrounded = controller.isGrounded;
        bool isMoving = movement.moveDirection.magnitude > 0.1f;
        float yVelocity = movement.GetComponent<Rigidbody>() ? 
                          movement.GetComponent<Rigidbody>().linearVelocity.y : 0f; // kalau pakai Rigidbody
        if (!movement.GetComponent<Rigidbody>())
            yVelocity = movement.GetComponent<CharacterController>() ? movement.GetComponent<CharacterController>().velocity.y : 0f;

        // --- Set Parameter ---
        anim.SetBool("isWalking", isMoving && isGrounded);
        anim.SetBool("isIdle", !isMoving && isGrounded);
        //anim.SetBool("isJumping", !isGrounded);
        anim.SetFloat("yVelocity", yVelocity);
        anim.SetBool("isGrounded", isGrounded);

        // --- Trigger Jump (opsional) ---
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            anim.SetTrigger("jumpStart"); // kalau kamu punya trigger jumpStart di Animator
        }
    }
}
