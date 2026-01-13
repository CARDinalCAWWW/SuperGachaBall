using UnityEngine;
using UnityEngine.InputSystem;

public class CameraTiltController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The ball that the camera follows and orbits around")]
    public Transform target;

    [Header("Camera Settings")]
    [Tooltip("Distance from the ball")]
    public float distance = 10f;
    [Tooltip("Height offset above the ball")]
    public float height = 5f;
    [Tooltip("Fixed downward viewing angle (degrees)")]
    public float viewAngle = 30f;
    [Tooltip("Subtle tilt when pressing W/S for visual feedback (degrees)")]
    public float forwardBackTiltAmount = 10f;
    [Tooltip("Maximum bank/roll angle when pressing A/D (degrees)")]
    public float maxBankAngle = 35f;
    
    [Header("Intro Animation")]
    [Tooltip("Enable camera zoom intro")]
    public bool enableIntro = true;
    [Tooltip("Duration of intro zoom (seconds)")]
    public float introDuration = 2f;
    [Tooltip("Starting distance multiplier for intro")]
    public float introDistanceMultiplier = 3f;

    [Header("Smoothing")]
    [Tooltip("How smoothly the camera rotates")]
    public float rotationSmoothness = 5f;
    
    [Header("Lateral Movement")]
    [Tooltip("How much the camera shifts left/right")]
    public float lateralShiftAmount = 3f;
    
    [Header("Camera Look")]
    [Tooltip("Mouse/right stick sensitivity for looking around")]
    public float lookSensitivity = 100f;
    [Tooltip("How far the camera can rotate left/right (degrees)")]
    public float maxYRotation = 60f;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private float currentXRotation = 0f;
    private float currentYRotation = 0f;
    private float currentZRotation = 0f;
    private float currentLateralOffset = 0f;
    private float targetXRotation = 0f;
    private float targetYRotation = 0f;
    private float targetZRotation = 0f;
    private float targetLateralOffset = 0f;
    
    // Intro animation
    private bool introComplete = false;
    private float introTimer = 0f;
    private float introDistanceOffset = 0f;

    // Called by Input System when Move action is triggered
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
    
    // Called by Input System when Look action is triggered
    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("CameraTiltController: No target assigned!");
        }
        
        // Initialize intro animation
        if (enableIntro)
        {
            introDistanceOffset = distance * (introDistanceMultiplier - 1f);
        }
        else
        {
            introComplete = true;
        }
    }

    [Header("Ground Settings")]
    [Tooltip("Minimum camera height above ground")]
    public float minHeight = 1f;

    private void LateUpdate()
    {
        if (target == null) return;
        
        // Update intro animation
        if (!introComplete)
        {
            introTimer += Time.deltaTime;
            float t = Mathf.Clamp01(introTimer / introDuration);
            // Smooth ease-out
            t = 1f - Mathf.Pow(1f - t, 3f);
            introDistanceOffset = Mathf.Lerp(distance * (introDistanceMultiplier - 1f), 0f, t);
            
            if (introTimer >= introDuration)
            {
                introComplete = true;
                introDistanceOffset = 0f;
            }
        }

        // Calculate target rotations and positions based on input
        
        // Y input (up/down) creates subtle tilt for visual feedback
        targetXRotation = viewAngle + (-moveInput.y * forwardBackTiltAmount);
        
        // X input (left/right) creates diagonal tilt (Z rotation)
        targetZRotation = moveInput.x * maxBankAngle;
        
        // X input also shifts camera laterally
        targetLateralOffset = moveInput.x * lateralShiftAmount;
        
        // Look input (mouse/right stick) rotates camera around Y axis
        targetYRotation += lookInput.x * lookSensitivity * Time.deltaTime;
        targetYRotation = Mathf.Clamp(targetYRotation, -maxYRotation, maxYRotation);

        // Smoothly interpolate to target values
        currentXRotation = Mathf.Lerp(currentXRotation, targetXRotation, Time.deltaTime * rotationSmoothness);
        currentYRotation = Mathf.Lerp(currentYRotation, targetYRotation, Time.deltaTime * rotationSmoothness);
        currentZRotation = Mathf.Lerp(currentZRotation, targetZRotation, Time.deltaTime * rotationSmoothness);
        currentLateralOffset = Mathf.Lerp(currentLateralOffset, targetLateralOffset, Time.deltaTime * rotationSmoothness);

        // Apply rotation (fixed downward angle + subtle tilt + look rotation, with Z banking)
        Quaternion rotation = Quaternion.Euler(currentXRotation, currentYRotation, currentZRotation);
        
        // Position camera at offset from target with lateral shift and intro distance
        float currentDistance = distance + introDistanceOffset;
        Vector3 offset = new Vector3(currentLateralOffset, height, -currentDistance);
        Vector3 rotatedOffset = rotation * offset;
        
        Vector3 desiredPosition = target.position + rotatedOffset;
        
        // Clamp camera height to prevent clipping into ground
        if (desiredPosition.y < minHeight)
        {
            desiredPosition.y = minHeight;
        }
        
        transform.position = desiredPosition;
        
        // Manually calculate rotation to preserve Z banking
        // Look at target
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        
        // Apply our tilt and banking on top
        transform.rotation = lookRotation * Quaternion.Euler(0, 0, currentZRotation);
    }

    // Public method to get direction for physics based on input
    public Vector3 GetForceDirection()
    {
        // Calculate movement direction in local space
        Vector3 forward = Vector3.forward * moveInput.y;
        Vector3 right = Vector3.right * moveInput.x;
        Vector3 localDirection = (forward + right).normalized;
        
        // Rotate the direction by the camera's Y rotation to make it camera-relative
        Quaternion cameraYRotation = Quaternion.Euler(0, currentYRotation, 0);
        Vector3 worldDirection = cameraYRotation * localDirection;
        
        return worldDirection;
    }

    public float GetTiltMagnitude()
    {
        return moveInput.magnitude;
    }
}
