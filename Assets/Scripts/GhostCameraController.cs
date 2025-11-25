using UnityEngine;

public class GhostCameraController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float lookSpeed = 2f;

    private float yaw = 0f;
    private float pitch = 0f;

    [HideInInspector] public bool movementLocked = false;

    void Update()
    {
        if (movementLocked) return;
        // Move
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float upDown = 0f;
        if (Input.GetKey(KeyCode.E)) upDown = 1f;
        if (Input.GetKey(KeyCode.Q)) upDown = -1f;

        Vector3 move = new Vector3(h, upDown, v);
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.Self);

        // Look
        yaw += Input.GetAxis("Mouse X") * lookSpeed;
        pitch -= Input.GetAxis("Mouse Y") * lookSpeed;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}