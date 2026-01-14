using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RespawnSystem : MonoBehaviour
{
    [Header("Respawn Settings")]
    [Tooltip("Y position below which the ball will respawn")]
    public float respawnHeight = -10f;
    
    [Tooltip("Position to respawn at. Leave at zero to use starting position.")]
    public Vector3 respawnPosition = Vector3.zero;
    
    [Tooltip("Use the ball's starting position as respawn point")]
    public bool useStartPosition = true;
    
    [Header("Effects")]
    [Tooltip("Time delay before respawning (seconds)")]
    public float respawnDelay = 0.5f;
    
    [Tooltip("Fade out/in effect (requires camera effect - optional)")]
    public bool useFadeEffect = false;

    private Rigidbody rb;
    private Vector3 startPosition;
    private bool isRespawning = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Store starting position
        if (useStartPosition)
        {
            startPosition = transform.position;
        }
        else
        {
            startPosition = respawnPosition;
        }
    }

    private void Update()
    {
        // Check if ball has fallen below respawn height
        if (!isRespawning && transform.position.y < respawnHeight)
        {
            StartRespawn();
        }
    }

    private void StartRespawn()
    {
        isRespawning = true;
        
        if (respawnDelay > 0)
        {
            Invoke(nameof(Respawn), respawnDelay);
        }
        else
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        // Reset position
        transform.position = startPosition;
        
        // Reset physics
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        isRespawning = false;
        
        Debug.Log("Player respawned!");
    }

    // Public method to force respawn (can be called from other scripts)
    public void ForceRespawn()
    {
        if (!isRespawning)
        {
            StartRespawn();
        }
    }

    // Public method to set a new respawn point
    public void SetRespawnPoint(Vector3 newPosition)
    {
        startPosition = newPosition;
        useStartPosition = false;
    }
}
