using UnityEngine;

/// <summary>
/// Shows a ghost trail preview of where the dash will go
/// </summary>
public class DashGhostTrail : MonoBehaviour
{
    [Header("Trail Settings")]
    [Tooltip("LineRenderer for the ghost trail")]
    public LineRenderer trailLine;
    [Tooltip("Number of points in the trail")]
    public int trailPoints = 20;
    [Tooltip("Distance of the trail")]
    public float trailDistance = 10f;
    [Tooltip("Trail color")]
    public Color trailColor = new Color(1f, 1f, 1f, 0.3f);
    [Tooltip("Trail width")]
    public float trailWidth = 0.2f;

    [Header("References")]
    [Tooltip("Ball's rigidbody for velocity")]
    public Rigidbody ballRigidbody;

    private void Start()
    {
        // Setup line renderer
        if (trailLine != null)
        {
            trailLine.positionCount = trailPoints;
            trailLine.startWidth = trailWidth;
            trailLine.endWidth = trailWidth * 0.5f;
            trailLine.startColor = trailColor;
            trailLine.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
            trailLine.enabled = false;
            
            // Use world space
            trailLine.useWorldSpace = true;
            
            // Optional: Set material if needed
            if (trailLine.material == null)
            {
                // Use default material
                trailLine.material = new Material(Shader.Find("Sprites/Default"));
            }
        }
    }

    /// <summary>
    /// Show the ghost trail
    /// </summary>
    /// <param name="startPosition">Starting position (ball position)</param>
    /// <param name="dashDirection">Direction and strength of dash</param>
    /// <param name="powerMultiplier">Power meter value (0-1)</param>
    public void ShowTrail(Vector3 startPosition, Vector3 dashDirection, float powerMultiplier)
    {
        if (trailLine == null || ballRigidbody == null) return;

        trailLine.enabled = true;

        // Get current velocity
        Vector3 currentVelocity = ballRigidbody.linearVelocity;
        
        // Calculate dash velocity (simplified physics)
        Vector3 dashVelocity = dashDirection * powerMultiplier;
        Vector3 totalVelocity = currentVelocity + dashVelocity;

        // Draw trail
        for (int i = 0; i < trailPoints; i++)
        {
            float t = i / (float)(trailPoints - 1);
            float distance = t * trailDistance * powerMultiplier;
            
            // Simple arc with gravity
            Vector3 position = startPosition + totalVelocity.normalized * distance;
            
            // Add slight downward curve (pseudo-gravity)
            position.y -= distance * distance * 0.05f;
            
            trailLine.SetPosition(i, position);
        }
    }

    /// <summary>
    /// Hide the ghost trail
    /// </summary>
    public void HideTrail()
    {
        if (trailLine != null)
        {
            trailLine.enabled = false;
        }
    }
}
