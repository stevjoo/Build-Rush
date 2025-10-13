using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 6.0f;
    public float gravity = -9.81f;
    public Transform cameraTransform;

    // --- BARU: Variabel untuk Lompat ---
    public float jumpHeight = 1.5f;

    private CharacterController characterController;
    private Vector3 velocity;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Cek jika karakter berada di tanah, reset kecepatan jatuh vertikal.
        // Ini penting agar gravitasi tidak terus menumpuk.
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // --- MODIFIKASI: Logika untuk Gerakan 4 Arah ---
        float x = Input.GetAxis("Horizontal"); // A/D
        float z = Input.GetAxis("Vertical");   // W/S

        // Logika untuk memastikan tidak ada gerakan diagonal
        // Jika input horizontal lebih besar dari vertikal, abaikan vertikal.
        if (Mathf.Abs(x) > Mathf.Abs(z))
        {
            z = 0f;
        }
        // Jika input vertikal lebih besar atau sama, abaikan horizontal.
        else
        {
            x = 0f;
        }
        // Opsional: Untuk gerakan yang lebih "patah-patah" / non-analog, 
        // Anda bisa mengganti Input.GetAxis dengan Input.GetAxisRaw.


        // Arah gerakan relatif terhadap kamera (logika ini tetap sama)
        Vector3 moveDirection = cameraTransform.right * x + cameraTransform.forward * z;
        moveDirection.y = 0;

        // Terapkan gerakan
        characterController.Move(moveDirection.normalized * speed * Time.deltaTime);

        // Putar karakter agar menghadap ke arah ia bergerak (logika ini tetap sama)
        if (moveDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        }

        // --- BARU: Logika untuk Lompat ---
        // Cek jika tombol Jump (default: Spasi) ditekan DAN karakter ada di tanah
        if (Input.GetButtonDown("Jump") && characterController.isGrounded)
        {
            // Ini adalah rumus fisika untuk menghitung kecepatan awal yang dibutuhkan
            // untuk mencapai ketinggian lompat yang diinginkan.
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Terapkan gravitasi (ini akan menarik karakter ke bawah setelah melompat)
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}