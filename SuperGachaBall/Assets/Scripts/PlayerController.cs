using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("The force applied to the ball to make it roll.")]
    public float movementForce = 10f;
    [Tooltip("Maximum velocity magnitude to prevent infinite acceleration.")]
    public float maxSpeed = 10f;

    [Header("References")]
    [Tooltip("Reference to the CameraController script.")]
    public CameraController cameraController;

    private Rigidbody rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // If camera reference is missing, try to find it
        if (cameraController == null && Camera.main != null)
        {
            cameraController = Camera.main.GetComponent<CameraController>();
        }
    }

    // Input System Message: Called when the 'Move' action is triggered
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        Debug.Log($"Move Input Received: {moveInput}");
    }

    // Input System Message: Called when the 'Look' action is triggered
    public void OnLook(InputValue value)
    {
        if (cameraController != null)
        {
            cameraController.SetInput(value.Get<Vector2>());
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        if (cameraController == null) 
        {
            Debug.LogError("PlayerController: CameraController reference is missing!");
            return;
        }
        Transform camTransform = cameraController.GetCameraTransform();

        // 1. Get Camera's forward and right vectors
        Vector3 camForward = camTransform.forward;
        Vector3 camRight = camTransform.right;

        // 2. Flatten them on the XZ plane (we don't want to move into the ground)
        camForward.y = 0;
        camRight.y = 0;
        
        // 3. Normalize to ensure consistent speed in all directions
        camForward.Normalize();
        camRight.Normalize();

        // 4. Calculate desired movement direction relative to camera
        Vector3 movementDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;

        // 5. Apply Force
        rb.AddForce(movementDirection * movementForce, ForceMode.Force);

        // Optional: Cap speed?
        // if (rb.linearVelocity.magnitude > maxSpeed)
        // {
        //     rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        // }
    }
}
