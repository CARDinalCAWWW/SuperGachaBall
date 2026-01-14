using UnityEngine;

/// <summary>
/// Handles camera effects during dash (FOV zoom, shake, etc.)
/// </summary>
public class DashCameraEffects : MonoBehaviour
{
    [Header("FOV Settings")]
    [Tooltip("Normal camera FOV")]
    public float normalFOV = 60f;
    [Tooltip("FOV during dash")]
    public float dashFOV = 75f;
    [Tooltip("How quickly FOV changes")]
    public float fovTransitionSpeed = 10f;

    [Header("Screen Shake")]
    [Tooltip("Enable screen shake on dash")]
    public bool enableScreenShake = true;
    [Tooltip("Shake intensity")]
    public float shakeMagnitude = 0.3f;
    [Tooltip("Shake duration")]
    public float shakeDuration = 0.2f;

    private Camera cam;
    private float targetFOV;
    private bool isShaking = false;
    private float shakeTimer = 0f;
    private Vector3 originalPosition;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("DashCameraEffects: No Camera component found!");
        }
        targetFOV = normalFOV;
    }

    private void Update()
    {
        // Smoothly transition FOV
        if (cam != null)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
        }

        // Handle screen shake
        if (isShaking)
        {
            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0f)
            {
                isShaking = false;
                // Reset position will be handled by CameraTiltController
            }
        }
    }

    private void LateUpdate()
    {
        // Apply shake after camera movement
        if (isShaking && cam != null)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            transform.position += shakeOffset;
        }
    }

    /// <summary>
    /// Trigger FOV zoom for dash
    /// </summary>
    public void StartDashZoom()
    {
        targetFOV = dashFOV;
    }

    /// <summary>
    /// Return to normal FOV
    /// </summary>
    public void EndDashZoom()
    {
        targetFOV = normalFOV;
    }

    /// <summary>
    /// Trigger screen shake
    /// </summary>
    public void TriggerShake()
    {
        if (enableScreenShake)
        {
            isShaking = true;
            shakeTimer = shakeDuration;
            originalPosition = transform.position;
        }
    }
}
