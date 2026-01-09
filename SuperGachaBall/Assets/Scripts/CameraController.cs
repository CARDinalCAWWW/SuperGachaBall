using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 5, -10);

    [Header("Rotation Settings")]
    public float rotationSpeed = 2f;
    
    private float yaw = 0f;
    private float pitch = 0f;
    private Vector2 lookInput;

    private void Start()
    {
        // Initialize yaw/pitch from current rotation if needed, 
        // or just start behind player.
       if (offset == Vector3.zero) offset = transform.position - target.position;
    }

    // Input supplied by PlayerController
    public void SetInput(Vector2 input)
    {
        lookInput = input;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleRotation();
        HandleFollow();
    }

    private void HandleRotation()
    {
        // Accumulate input
        yaw += lookInput.x * rotationSpeed;
        // Optional: Pitch control if you want vertical camera movement
        // pitch -= lookInput.y * rotationSpeed; 
        // pitch = Mathf.Clamp(pitch, -10f, 60f);

        // Create rotation
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        
        // We will apply this rotation to the offset positioning logic in HandleFollow
        // But for a simple orbit, we often just rotate the offset vector
    }

    private void HandleFollow()
    {
        // Calculate rotation based on yaw
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // Rotate the original offset by our current rotation
        Vector3 rotatedOffset = rotation * offset;

        // Set position relative to target
        transform.position = target.position + rotatedOffset;
        
        // Always look at the target
        transform.LookAt(target.position);
    }

    public Transform GetCameraTransform()
    {
        return transform;
    }
}
