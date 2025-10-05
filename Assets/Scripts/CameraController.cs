using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // Target yang diikuti (Player)
    public float distance = 5.0f; // Jarak kamera dari target
    public float height = 2.0f; // Ketinggian kamera dari target
    public float rotationSpeed = 2.0f;
    public float zoomSpeed = 4.0f;
    public float minDistance = 2.0f;
    public float maxDistance = 10.0f;

    private float currentX = 0.0f;
    private float currentY = 0.0f;

    // Gunakan LateUpdate agar kamera bergerak setelah player selesai bergerak
    void LateUpdate()
    {
        if (target == null) return;

        // Dapatkan input mouse HANYA jika tombol kanan mouse ditekan
        if (Input.GetMouseButton(1)) // 1 = Tombol Kanan Mouse
        {
            currentX += Input.GetAxis("Mouse X") * rotationSpeed;
            currentY -= Input.GetAxis("Mouse Y") * rotationSpeed;
            currentY = Mathf.Clamp(currentY, -20f, 80f); // Batasi sudut vertikal
        }

        // Zoom dengan scroll wheel
        distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Hitung rotasi dan posisi yang diinginkan
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 targetPosition = target.position + new Vector3(0, height, 0);
        Vector3 cameraPosition = targetPosition - (rotation * Vector3.forward * distance);

        // Terapkan ke kamera
        transform.position = cameraPosition;
        transform.LookAt(targetPosition);
    }
}