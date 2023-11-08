using UnityEngine;

public class IsometricCameraController : MonoBehaviour
{
    public Transform target; // The target point the camera will look at
    public float distance = 10.0f; // Distance from the target
    public float mouseSensitivity = 5.0f; // Mouse movement sensitivity
    public float zoomSpeed = 1.0f; // Speed of zoom in/out
    public float keyboardRotationSpeed = 30.0f; // Speed of rotation with keyboard keys

    private float _horizontalAngle = 0.0f;
    private float _verticalAngle = 30.0f; // Starting angle from horizontal plane to look at the target

    void Start()
    {
        if (!target)
        {
            Debug.LogError("No target assigned for IsometricCameraController.");
            return;
        }
        Camera.main.orthographic = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        UpdateCameraPosition();
    }

    void Update()
    {
        _horizontalAngle += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        _verticalAngle -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        _verticalAngle = Mathf.Clamp(_verticalAngle, 5f, 85f); 
        if (Input.GetKey(KeyCode.A))
            _horizontalAngle -= keyboardRotationSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.D))
            _horizontalAngle += keyboardRotationSpeed * Time.deltaTime;

        // Adjust camera zoom with 'W' and 'S' keys
        if (Input.GetKey(KeyCode.W))
            Camera.main.orthographicSize -= zoomSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S))
            Camera.main.orthographicSize += zoomSpeed * Time.deltaTime;

        // Keep the orthographic size within reasonable limits
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 1f, 100f);

        // Update camera position
        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        // Calculate new camera position based on spherical coordinates
        float x = distance * Mathf.Sin(Mathf.Deg2Rad * _verticalAngle) * Mathf.Sin(Mathf.Deg2Rad * _horizontalAngle);
        float z = distance * Mathf.Sin(Mathf.Deg2Rad * _verticalAngle) * Mathf.Cos(Mathf.Deg2Rad * _horizontalAngle);
        float y = distance * Mathf.Cos(Mathf.Deg2Rad * _verticalAngle);

        // Apply the calculated position and look at the target
        Vector3 newPosition = target.position + new Vector3(x, y, z);
        transform.position = newPosition;
        transform.LookAt(target.position);
    }

    private void OnDisable()
    {
        // When the script is disabled, unlock the cursor and make it visible again
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
