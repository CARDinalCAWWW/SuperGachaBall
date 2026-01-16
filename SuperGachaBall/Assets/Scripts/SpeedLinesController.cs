using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class SpeedLinesController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Rigidbody to track speed from")]
    public Rigidbody targetRigidbody;
    
    [Header("Settings")]
    [Tooltip("Speed at which lines start appearing")]
    public float minSpeed = 10f;
    
    [Tooltip("Speed at which lines are fully visible")]
    public float maxSpeed = 30f;
    
    [Tooltip("How fast the opacity changes")]
    public float fadeSpeed = 5f;

    [Tooltip("How fast the lines rotate (degrees/sec)")]
    public float rotationSpeed = 20f;
    
    [Tooltip("How much the lines pulse in size")]
    public float pulseAmount = 0.05f;
    
    [Tooltip("Speed of the pulse")]
    public float pulseSpeed = 10f;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 initialScale;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        if (rectTransform != null)
        {
            initialScale = rectTransform.localScale;
        }

        // Auto-fix: Disable Raycast Target so it doesn't block UI buttons!
        Image img = GetComponent<Image>();
        if (img != null)
        {
            img.raycastTarget = false;
        }
    }

    private void Update()
    {
        if (targetRigidbody != null && canvasGroup != null)
        {
            float currentSpeed = targetRigidbody.linearVelocity.magnitude;
            
            // Calculate target alpha
            float targetAlpha = 0f;
            if (currentSpeed > minSpeed)
            {
                // Normalize speed between min and max
                float t = Mathf.InverseLerp(minSpeed, maxSpeed, currentSpeed);
                targetAlpha = t;
                
                // Rotate while active
                if (rectTransform != null)
                {
                    rectTransform.Rotate(Vector3.forward * rotationSpeed * t * Time.deltaTime);
                    
                    // Pulse scale
                    float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount * t;
                    rectTransform.localScale = initialScale * pulse;
                }
            }
            else
            {
                // Reset scale when not moving fast
                if (rectTransform != null)
                {
                    rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, initialScale, Time.deltaTime * fadeSpeed);
                }
            }

            // Smoothly interpolate alpha
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        }
    }

    // Helper to auto-find player if missing (called by GameUI or similar)
    public void SetTarget(Rigidbody newTarget)
    {
        targetRigidbody = newTarget;
    }
}
