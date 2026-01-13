using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallPhysics : MonoBehaviour
{
    [Header("Physics Settings")]
    [Tooltip("Force applied to simulate tilted surface")]
    public float tiltForce = 5f;
    [Tooltip("Maximum speed the ball can roll")]
    public float maxSpeed = 8f;

    [Header("References")]
    [Tooltip("Reference to the camera controller")]
    public CameraTiltController cameraController;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Auto-find camera if not assigned
        if (cameraController == null && Camera.main != null)
        {
            cameraController = Camera.main.GetComponent<CameraTiltController>();
        }
    }

    private void FixedUpdate()
    {
        if (cameraController == null)
        {
            Debug.LogError("BallPhysics: CameraController reference missing!");
            return;
        }

        // Get the force direction from camera
        Vector3 forceDirection = cameraController.GetForceDirection();
        float tiltMagnitude = cameraController.GetTiltMagnitude();

        // Apply force in the calculated direction
        Vector3 force = forceDirection * tiltForce * tiltMagnitude;
        rb.AddForce(force, ForceMode.Force);

        // Optional: Limit max speed
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    /// <summary>
    /// Apply an instant dash force to the ball
    /// </summary>
    /// <param name="dashForce">The force vector to apply</param>
    public void ApplyDashForce(Vector3 dashForce)
    {
        if (rb != null)
        {
            rb.AddForce(dashForce, ForceMode.Impulse);
            Debug.Log($"Dash force applied: {dashForce}, Current velocity: {rb.linearVelocity}");
        }
    }
    
    /// <summary>
    /// Get the current velocity of the ball
    /// </summary>
    /// <returns>Current velocity vector</returns>
    public Vector3 GetVelocity()
    {
        return rb != null ? rb.linearVelocity : Vector3.zero;
    }
    
    /// <summary>
    /// Directly add to the ball's velocity
    /// </summary>
    /// <param name="velocityChange">Velocity to add</param>
    public void AddVelocity(Vector3 velocityChange)
    {
        if (rb != null)
        {
            rb.linearVelocity += velocityChange;
        }
    }
}
