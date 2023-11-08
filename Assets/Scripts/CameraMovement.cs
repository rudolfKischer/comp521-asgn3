using UnityEngine;

public class IsometricCameraController : MonoBehaviour
{
    public Transform target; 
    public float distance = 20.0f; 
    public float mouseSensitivity = 620.0f; 
    public float zoomSpeed = 20.0f; 
    public float keyboardRotationSpeed = 120.0f; 

    private float _horizontalAngle = 0.0f;
    private float _verticalAngle = 30.0f; 

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
        if (Input.GetKey(KeyCode.W))
            Camera.main.orthographicSize -= zoomSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S))
            Camera.main.orthographicSize += zoomSpeed * Time.deltaTime;

        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 1f, 100f);
        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        float x = distance * Mathf.Sin(Mathf.Deg2Rad * _verticalAngle) * Mathf.Sin(Mathf.Deg2Rad * _horizontalAngle);
        float z = distance * Mathf.Sin(Mathf.Deg2Rad * _verticalAngle) * Mathf.Cos(Mathf.Deg2Rad * _horizontalAngle);
        float y = distance * Mathf.Cos(Mathf.Deg2Rad * _verticalAngle);
        Vector3 newPosition = target.position + new Vector3(x, y, z);
        transform.position = newPosition;
        transform.LookAt(target.position);
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
